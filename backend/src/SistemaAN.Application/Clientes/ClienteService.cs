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
        var dados = await ValidarAsync(request, null, cancellationToken);
        var cliente = Cliente.Criar(dados);
        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(cliente);
    }

    public async Task<ClienteDto> AtualizarAsync(long id, SalvarClienteRequest request, CancellationToken cancellationToken = default)
    {
        var cliente = await _db.Clientes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Cliente", id);

        var dados = await ValidarAsync(request, id, cancellationToken);
        cliente.Atualizar(dados);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(cliente);
    }

    public async Task CancelarAsync(long id, string motivo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["motivo"] = ["Informe o motivo do cancelamento."] });
        }

        var cliente = await _db.Clientes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Cliente", id);

        cliente.Cancelar(motivo, DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task ReativarAsync(long id, CancellationToken cancellationToken = default)
    {
        var cliente = await _db.Clientes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Cliente", id);
        cliente.Reativar();
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<DadosCliente> ValidarAsync(SalvarClienteRequest r, long? idAtual, CancellationToken cancellationToken)
    {
        var erros = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(r.Nome))
        {
            erros["nome"] = ["Informe o nome."];
        }

        var cpf = r.Cpf is null ? null : new string(r.Cpf.Where(char.IsDigit).ToArray());
        if (!string.IsNullOrEmpty(cpf) && cpf.Length != 11)
        {
            erros["cpf"] = ["O CPF deve ter 11 dígitos."];
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

        if (!string.IsNullOrEmpty(cpf))
        {
            var cpfEmUso = await _db.Clientes.AnyAsync(
                c => c.Cpf == cpf && (idAtual == null || c.Id != idAtual),
                cancellationToken);
            if (cpfEmUso)
            {
                throw new ValidationException(new Dictionary<string, string[]> { ["cpf"] = ["Já existe um cliente com este CPF."] });
            }
        }

        return new DadosCliente(
            r.Nome, cpf, r.Telefone, r.Email, r.OrigemVenda, r.Observacoes,
            r.Rua, r.Numero, r.Complemento, r.Cep, r.Bairro, r.Cidade, r.Estado,
            tipo, forma, r.DiaCobranca, r.ValorRecorrenteMensal, status, r.ObservacoesFinanceiras);
    }

    private static ClienteDto Map(Cliente c) => new(
        c.Id, c.Nome, c.Cpf, c.Telefone, c.Email, c.OrigemVenda, c.Observacoes,
        c.Rua, c.Numero, c.Complemento, c.Cep, c.Bairro, c.Cidade, c.Estado,
        c.Ativo, c.MotivoCancelamento, c.DataCancelamento,
        c.TipoCliente.ToString(),
        c.FormaPagamento?.ToString(),
        c.DiaCobranca,
        c.ValorRecorrenteMensal,
        c.StatusFinanceiro.ToString(),
        c.ObservacoesFinanceiras);
}
