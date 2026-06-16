using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Catalog;

/// <summary>Onde a categoria pode ser usada (lista única compartilhada).</summary>
public enum EscopoCategoria
{
    /// <summary>Categorias de alimento (aparecem no cadastro de ingredientes).</summary>
    Alimento,

    /// <summary>Categorias de material/insumo não-alimentar (embalagens, etiquetas, etc.).</summary>
    Material,

    /// <summary>Serve para os dois contextos.</summary>
    Ambos,
}

/// <summary>
/// Categoria compartilhada por Ingredientes e Estoque (tabela extensível — novas
/// categorias sem migração de código). O <see cref="Escopo"/> define onde aparece.
/// </summary>
public class CategoriaIngrediente : Entity
{
    private CategoriaIngrediente() { } // EF Core

    private CategoriaIngrediente(string nome, string? descricao, int ordem, EscopoCategoria escopo)
    {
        Nome = nome;
        Descricao = descricao;
        Ordem = ordem;
        Escopo = escopo;
        Ativo = true;
    }

    public string Nome { get; private set; } = string.Empty;

    public string? Descricao { get; private set; }

    /// <summary>Ordem de exibição (menor primeiro).</summary>
    public int Ordem { get; private set; }

    /// <summary>Onde a categoria é usada (Alimento / Material / Ambos).</summary>
    public EscopoCategoria Escopo { get; private set; }

    public bool Ativo { get; private set; }

    public static CategoriaIngrediente Criar(string nome, string? descricao = null, int ordem = 0, EscopoCategoria escopo = EscopoCategoria.Alimento)
        => new(nome.Trim(), Texto(descricao), ordem, escopo);

    public void Atualizar(string nome, string? descricao, int ordem, bool ativo, EscopoCategoria escopo)
    {
        Nome = nome.Trim();
        Descricao = Texto(descricao);
        Ordem = ordem;
        Ativo = ativo;
        Escopo = escopo;
    }

    public void DefinirAtivo(bool ativo) => Ativo = ativo;

    private static string? Texto(string? v)
    {
        var t = v?.Trim();
        return string.IsNullOrEmpty(t) ? null : t;
    }
}
