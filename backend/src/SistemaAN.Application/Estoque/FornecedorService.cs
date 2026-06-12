using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Estoque;

namespace SistemaAN.Application.Estoque;

public sealed class FornecedorService : IFornecedorService
{
    private readonly IApplicationDbContext _db;

    public FornecedorService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<FornecedorDto>> ListarAsync(bool apenasAtivos = false, CancellationToken cancellationToken = default)
    {
        var query = _db.Fornecedores.AsQueryable();
        if (apenasAtivos)
        {
            query = query.Where(f => f.Ativo);
        }

        return await query
            .OrderBy(f => f.Nome)
            .Select(f => Map(f))
            .ToListAsync(cancellationToken);
    }

    public async Task<FornecedorDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var f = await _db.Fornecedores.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Fornecedor", id);
        return Map(f);
    }

    public async Task<FornecedorDto> CriarAsync(SalvarFornecedorRequest request, CancellationToken cancellationToken = default)
    {
        var categoria = ValidarEMapear(request);

        var fornecedor = Fornecedor.Criar(
            request.Nome, request.NomeFantasia, request.Documento, request.Telefone, request.WhatsApp,
            request.Email, request.PessoaContato, request.Endereco, request.Cidade, request.Estado,
            categoria, request.Observacoes, request.PrazoPagamentoDias, request.FormaPagamentoPreferida,
            request.ChavePix, request.DadosBancarios);
        fornecedor.DefinirAtivo(request.Ativo);

        _db.Fornecedores.Add(fornecedor);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(fornecedor);
    }

    public async Task<FornecedorDto> AtualizarAsync(long id, SalvarFornecedorRequest request, CancellationToken cancellationToken = default)
    {
        var fornecedor = await _db.Fornecedores.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Fornecedor", id);

        var categoria = ValidarEMapear(request);

        fornecedor.Atualizar(
            request.Nome, request.NomeFantasia, request.Documento, request.Telefone, request.WhatsApp,
            request.Email, request.PessoaContato, request.Endereco, request.Cidade, request.Estado,
            categoria, request.Observacoes, request.PrazoPagamentoDias, request.FormaPagamentoPreferida,
            request.ChavePix, request.DadosBancarios, request.Ativo);

        await _db.SaveChangesAsync(cancellationToken);
        return Map(fornecedor);
    }

    public async Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default)
    {
        var fornecedor = await _db.Fornecedores.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Fornecedor", id);

        fornecedor.DefinirAtivo(ativo);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static CategoriaFornecedor? ValidarEMapear(SalvarFornecedorRequest request)
    {
        var erros = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            erros["nome"] = ["Informe o nome / razão social do fornecedor."];
        }

        CategoriaFornecedor? categoria = null;
        if (!string.IsNullOrWhiteSpace(request.Categoria))
        {
            if (Enum.TryParse<CategoriaFornecedor>(request.Categoria, out var c))
            {
                categoria = c;
            }
            else
            {
                erros["categoria"] = ["Categoria de fornecedor inválida."];
            }
        }

        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }

        return categoria;
    }

    private static FornecedorDto Map(Fornecedor f)
        => new(
            f.Id, f.Nome, f.NomeFantasia, f.Documento, f.Telefone, f.WhatsApp, f.Email, f.PessoaContato,
            f.Endereco, f.Cidade, f.Estado, f.Categoria?.ToString(), f.Observacoes, f.PrazoPagamentoDias,
            f.FormaPagamentoPreferida, f.ChavePix, f.DadosBancarios, f.Ativo);
}
