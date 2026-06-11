using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Identity;

/// <summary>
/// Papel (role) atribuível a usuários. Nesta fase há apenas "Administrador",
/// mas o modelo já suporta múltiplos papéis.
/// </summary>
public class Papel : Entity
{
    private Papel() { } // EF Core

    private Papel(string nome, string? descricao)
    {
        Nome = nome;
        Descricao = descricao;
    }

    public string Nome { get; private set; } = string.Empty;

    public string? Descricao { get; private set; }

    public static Papel Criar(string nome, string? descricao = null) => new(nome.Trim(), descricao);
}
