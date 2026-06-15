namespace SistemaAN.Domain.Identity;

/// <summary>
/// Nomes dos papéis conhecidos do sistema. Perfis fixos nesta fase.
/// </summary>
public static class PapeisDoSistema
{
    public const string Administrador = "Administrador";
    public const string Operador = "Operador";
    public const string Cozinha = "Cozinha";

    /// <summary>Todos os perfis válidos, com uma breve descrição.</summary>
    public static readonly IReadOnlyDictionary<string, string> Todos = new Dictionary<string, string>
    {
        [Administrador] = "Acesso total ao sistema.",
        [Operador] = "Acesso operacional; na Produção do dia/Cozinha apenas visualiza.",
        [Cozinha] = "Acesso somente à Produção do dia e à Cozinha.",
    };

    public static bool Existe(string nome) => Todos.ContainsKey(nome);
}
