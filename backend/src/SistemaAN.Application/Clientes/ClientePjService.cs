using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Clientes;

namespace SistemaAN.Application.Clientes;

public sealed class ClientePjService : IClientePjService
{
    private static readonly IReadOnlyDictionary<TipoClientePJ, string> Rotulos = new Dictionary<TipoClientePJ, string>
    {
        [TipoClientePJ.Mercado] = "Mercado",
        [TipoClientePJ.PetShop] = "Pet shop",
        [TipoClientePJ.ClinicaVeterinaria] = "Clínica veterinária",
        [TipoClientePJ.Revendedor] = "Revendedor",
        [TipoClientePJ.Parceiro] = "Parceiro",
        [TipoClientePJ.Outro] = "Outro",
    };

    private readonly IApplicationDbContext _db;

    public ClientePjService(IApplicationDbContext db) => _db = db;

    public IReadOnlyList<TipoPjOpcaoDto> ListarTipos()
        => Rotulos.Select(r => new TipoPjOpcaoDto(r.Key.ToString(), r.Value)).ToList();

    public async Task<IReadOnlyList<ClientePjResumoDto>> ListarAsync(string? busca, bool? ativo, CancellationToken cancellationToken = default)
    {
        var q = from c in _db.Clientes
                join pj in _db.ClientesPj on c.Id equals pj.ClienteId
                where c.Natureza == NaturezaCliente.PessoaJuridica
                select new { c, pj };

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var t = busca.Trim();
            q = q.Where(x => x.pj.NomeFantasia.Contains(t) || x.pj.RazaoSocial.Contains(t) || x.pj.Cnpj.Contains(t));
        }
        if (ativo.HasValue)
        {
            q = q.Where(x => x.c.Ativo == ativo.Value);
        }

        var rows = await q.OrderBy(x => x.pj.NomeFantasia).ToListAsync(cancellationToken);
        return rows.Select(x => new ClientePjResumoDto(
            x.c.Id, x.pj.NomeFantasia, x.pj.RazaoSocial, x.pj.Cnpj, x.c.Cidade, x.c.Telefone,
            x.pj.TipoPJ.ToString(), x.c.Ativo)).ToList();
    }

    public async Task<ClientePjDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var row = await (from c in _db.Clientes
                         join pj in _db.ClientesPj on c.Id equals pj.ClienteId
                         where c.Id == id && c.Natureza == NaturezaCliente.PessoaJuridica
                         select new { c, pj }).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Cliente PJ", id);
        return Map(row.c, row.pj);
    }

    public async Task<ClientePjDto> CriarAsync(SalvarClientePjRequest request, CancellationToken cancellationToken = default)
    {
        var (dadosCliente, dadosPj) = await ValidarAsync(request, null, cancellationToken);

        var cliente = Cliente.Criar(dadosCliente);
        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync(cancellationToken);

        var pj = ClientePj.Criar(cliente.Id, dadosPj);
        _db.ClientesPj.Add(pj);
        await _db.SaveChangesAsync(cancellationToken);

        return await ObterAsync(cliente.Id, cancellationToken);
    }

    public async Task<ClientePjDto> AtualizarAsync(long id, SalvarClientePjRequest request, CancellationToken cancellationToken = default)
    {
        var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.Natureza == NaturezaCliente.PessoaJuridica, cancellationToken)
            ?? throw new NotFoundException("Cliente PJ", id);
        var pj = await _db.ClientesPj.FirstOrDefaultAsync(p => p.ClienteId == id, cancellationToken)
            ?? throw new NotFoundException("Cliente PJ", id);

        var (dadosCliente, dadosPj) = await ValidarAsync(request, id, cancellationToken);
        cliente.Atualizar(dadosCliente);
        pj.Atualizar(dadosPj);
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(id, cancellationToken);
    }

    public async Task<ClientePjDto> AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default)
    {
        var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.Natureza == NaturezaCliente.PessoaJuridica, cancellationToken)
            ?? throw new NotFoundException("Cliente PJ", id);
        if (ativo)
        {
            cliente.Reativar();
        }
        else
        {
            cliente.Cancelar("Inativado pelo cadastro", DateTimeOffset.UtcNow);
        }
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(id, cancellationToken);
    }

    private async Task<(DadosCliente, DadosClientePj)> ValidarAsync(SalvarClientePjRequest r, long? idAtual, CancellationToken ct)
    {
        var erros = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(r.RazaoSocial)) erros["razaoSocial"] = ["Informe a razão social."];
        if (string.IsNullOrWhiteSpace(r.NomeFantasia)) erros["nomeFantasia"] = ["Informe o nome fantasia."];

        var cnpj = new string((r.Cnpj ?? string.Empty).Where(char.IsDigit).ToArray());
        if (cnpj.Length != 14) erros["cnpj"] = ["O CNPJ deve ter 14 dígitos."];

        if (!Enum.TryParse<TipoClientePJ>(r.TipoPJ, true, out var tipoPj)) erros["tipoPJ"] = ["Tipo de cliente PJ inválido."];

        if (erros.Count > 0) throw new ValidationException(erros);

        var cnpjEmUso = await _db.ClientesPj.AnyAsync(p => p.Cnpj == cnpj && (idAtual == null || p.ClienteId != idAtual), ct);
        if (cnpjEmUso)
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["cnpj"] = ["Já existe um Cliente PJ com este CNPJ."] });
        }

        var dadosCliente = new DadosCliente(
            Nome: r.NomeFantasia.Trim(),
            Cpf: null,
            Telefone: r.Telefone,
            Email: r.Email,
            OrigemVenda: null,
            Observacoes: r.Observacoes,
            Rua: r.Rua,
            Numero: r.Numero,
            Complemento: r.Complemento,
            Cep: r.Cep,
            Bairro: r.Bairro,
            Cidade: r.Cidade,
            Estado: r.Estado,
            FrequenciaEntregaId: null,
            PrimeiraEntrega: null,
            TipoCliente: TipoCliente.Avulso,
            FormaPagamento: null,
            DiaCobranca: null,
            ValorRecorrenteMensal: 0m,
            StatusFinanceiro: StatusFinanceiro.EmDia,
            ObservacoesFinanceiras: null,
            Natureza: NaturezaCliente.PessoaJuridica);

        var dadosPj = new DadosClientePj(
            r.RazaoSocial, r.NomeFantasia, cnpj, r.InscricaoEstadual, r.Whatsapp, r.PessoaContato, r.CargoContato,
            r.EntregaRua, r.EntregaNumero, r.EntregaComplemento, r.EntregaBairro, r.EntregaCidade, r.EntregaEstado, r.EntregaCep,
            tipoPj, r.CondicaoComercial, r.PrazoPagamento, r.DiaEntregaPreferencial, r.FrequenciaCompra, r.ObservacoesComerciais);

        return (dadosCliente, dadosPj);
    }

    private static ClientePjDto Map(Cliente c, ClientePj pj) => new(
        c.Id, pj.RazaoSocial, pj.NomeFantasia, pj.Cnpj, pj.InscricaoEstadual,
        c.Telefone, pj.Whatsapp, c.Email, pj.PessoaContato, pj.CargoContato,
        c.Rua, c.Numero, c.Complemento, c.Bairro, c.Cidade, c.Estado, c.Cep,
        pj.EntregaRua, pj.EntregaNumero, pj.EntregaComplemento, pj.EntregaBairro, pj.EntregaCidade, pj.EntregaEstado, pj.EntregaCep,
        pj.TipoPJ.ToString(), pj.CondicaoComercial, pj.PrazoPagamento, pj.DiaEntregaPreferencial, pj.FrequenciaCompra,
        c.Observacoes, pj.ObservacoesComerciais, c.Ativo, c.UpdatedAt);
}
