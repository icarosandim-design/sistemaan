using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Catalog;

/// <summary>
/// Categoria de ingrediente (tabela extensível — novas categorias sem migração).
/// </summary>
public class CategoriaIngrediente : Entity
{
    private CategoriaIngrediente() { } // EF Core

    private CategoriaIngrediente(string nome)
    {
        Nome = nome;
        Ativo = true;
    }

    public string Nome { get; private set; } = string.Empty;

    public bool Ativo { get; private set; }

    public static CategoriaIngrediente Criar(string nome) => new(nome.Trim());
}
