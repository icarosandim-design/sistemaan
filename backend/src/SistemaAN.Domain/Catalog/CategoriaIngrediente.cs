using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Catalog;

/// <summary>
/// Categoria de ingrediente (tabela extensível — novas categorias sem migração de código).
/// </summary>
public class CategoriaIngrediente : Entity
{
    private CategoriaIngrediente() { } // EF Core

    private CategoriaIngrediente(string nome, string? descricao, int ordem)
    {
        Nome = nome;
        Descricao = descricao;
        Ordem = ordem;
        Ativo = true;
    }

    public string Nome { get; private set; } = string.Empty;

    public string? Descricao { get; private set; }

    /// <summary>Ordem de exibição (menor primeiro).</summary>
    public int Ordem { get; private set; }

    public bool Ativo { get; private set; }

    public static CategoriaIngrediente Criar(string nome, string? descricao = null, int ordem = 0)
        => new(nome.Trim(), Texto(descricao), ordem);

    public void Atualizar(string nome, string? descricao, int ordem, bool ativo)
    {
        Nome = nome.Trim();
        Descricao = Texto(descricao);
        Ordem = ordem;
        Ativo = ativo;
    }

    public void DefinirAtivo(bool ativo) => Ativo = ativo;

    private static string? Texto(string? v)
    {
        var t = v?.Trim();
        return string.IsNullOrEmpty(t) ? null : t;
    }
}
