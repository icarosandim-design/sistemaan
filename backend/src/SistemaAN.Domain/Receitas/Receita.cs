using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Receitas;

/// <summary>
/// Receita = ficha técnica da comida. Base de 1 kg cozido para a Casa.
/// Entidade unificada: o tipo distingue Casa (reutilizável) de Personalizada
/// (um único pet — futuro). Não guarda status de produção/estoque.
/// </summary>
public class Receita : AuditableEntity
{
    private readonly List<ItemReceita> _itens = [];

    private Receita() { } // EF Core

    private Receita(string codigo, string nome, TipoReceita tipo, long? petId, string observacoes)
    {
        Codigo = codigo;
        Nome = nome;
        Tipo = tipo;
        PetId = petId;
        Observacoes = observacoes;
        Ativo = true;
    }

    public string Codigo { get; private set; } = string.Empty;

    public string Nome { get; private set; } = string.Empty;

    public TipoReceita Tipo { get; private set; }

    /// <summary>Dono (apenas personalizada). FK real quando o módulo Pets existir.</summary>
    public long? PetId { get; private set; }

    public string Observacoes { get; private set; } = string.Empty;

    public bool Ativo { get; private set; }

    public IReadOnlyCollection<ItemReceita> Itens => _itens.AsReadOnly();

    public static Receita CriarCasa(string codigo, string nome, string? observacoes)
        => new(codigo.Trim(), nome.Trim(), TipoReceita.Casa, null, observacoes?.Trim() ?? string.Empty);

    /// <summary>Receita exclusiva de um pet (criada dentro do Plano Alimentar).</summary>
    public static Receita CriarPersonalizada(long petId, string codigo, string nome, string? observacoes)
        => new(codigo.Trim(), nome.Trim(), TipoReceita.Personalizada, petId, observacoes?.Trim() ?? string.Empty);

    public void Atualizar(string codigo, string nome, string? observacoes, bool ativo)
    {
        Codigo = codigo.Trim();
        Nome = nome.Trim();
        Observacoes = observacoes?.Trim() ?? string.Empty;
        Ativo = ativo;
    }

    public void DefinirAtivo(bool ativo) => Ativo = ativo;

    public void SubstituirItens(IEnumerable<ItemReceita> itens)
    {
        _itens.Clear();
        _itens.AddRange(itens);
    }
}
