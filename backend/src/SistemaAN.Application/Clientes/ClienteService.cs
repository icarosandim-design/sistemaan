using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Clientes;

namespace SistemaAN.Application.Clientes;

public sealed class ClienteService : IClienteService
{
    private readonly IApplicationDbContext _db;

    public ClienteService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<ClienteDto>> ListarAsync(CancellationToken cancellationToken = default)
        => await _db.Clientes.OrderBy(c => c.Nome).Select(c => Map(c)).ToListAsync(cancellationToken);

    public async Task<ClienteDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var c = await _db.Clientes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Cliente", id);
        return Map(c);
    }

    public async Task<ClienteDto> CriarAsync(SalvarClienteRequest request, CancellationToken cancellationToken = default)
    {
        var dados = Validar(request);
        var cliente = Cliente.Criar(dados);
        cliente.DefinirAtivo(request.Ativo);

        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(cliente);
    }

    public async Task<ClienteDto> AtualizarAsync(long id, SalvarClienteRequest request, CancellationToken cancellationToken = default)
    {
        var cliente = await _db.Clientes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Cliente", id);

        var dados = Validar(request);
        cliente.Atualizar(dados, request.Ativo);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(cliente);
    }

    public async Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default)
    {
        var cliente = await _db.Clientes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Cliente", id);
        cliente.DefinirAtivo(ativo);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static DadosCliente Validar(SalvarClienteRequest r)
    {
        var erros = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(r.Nome))
        {
            erros["nome"] = ["Informe o nome."];
        }

        if (r.DiaCobranca is < 1 or > 31)
        {
            erros["diaCobranca"] = ["O dia de cobrança deve estar entre 1 e 31."];
        }

        if (r.ValorRecorrenteMensal < 0)
        {
            erros["valorRecorrenteMensal"] = ["O valor não pode ser negativo."];
        }

        if (!Enum.TryParse<TipoCliente>(r.TipoCliente, true, out var tipo))
        {
            erros["tipoCliente"] = ["Tipo de cliente inválido."];
        }

        if (!Enum.TryParse<StatusFinanceiro>(r.StatusFinanceiro, true, out var status))
        {
            erros["statusFinanceiro"] = ["Status financeiro inválido."];
        }

        FormaPagamento? forma = null;
        if (!string.IsNullOrWhiteSpace(r.FormaPagamento))
        {
            if (Enum.TryParse<FormaPagamento>(r.FormaPagamento, true, out var f))
            {
                forma = f;
            }
            else
            {
                erros["formaPagamento"] = ["Forma de pagamento inválida."];
            }
        }

        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }

        return new DadosCliente(
            r.Nome, r.Telefone, r.Email, r.Endereco, r.Bairro, r.Cidade, r.Observacoes,
            tipo, forma, r.DiaCobranca, r.ValorRecorrenteMensal, status, r.ObservacoesFinanceiras);
    }

    private static ClienteDto Map(Cliente c) => new(
        c.Id, c.Nome, c.Telefone, c.Email, c.Endereco, c.Bairro, c.Cidade, c.Observacoes, c.Ativo,
        c.TipoCliente.ToString(),
        c.FormaPagamento?.ToString(),
        c.DiaCobranca,
        c.ValorRecorrenteMensal,
        c.StatusFinanceiro.ToString(),
        c.ObservacoesFinanceiras);
}
