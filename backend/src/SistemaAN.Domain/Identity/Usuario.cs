using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Identity;

/// <summary>
/// Usuário da equipe que opera o sistema. Mantém a relação N:N com <see cref="Papel"/>
/// (preparada para múltiplos perfis no futuro) e seus refresh tokens.
/// </summary>
public class Usuario : AggregateRoot
{
    private readonly List<Papel> _papeis = [];
    private readonly List<RefreshToken> _refreshTokens = [];

    private Usuario() { } // EF Core

    private Usuario(string nome, string email, string senhaHash)
    {
        Nome = nome;
        Email = email;
        SenhaHash = senhaHash;
        Ativo = true;
    }

    public string Nome { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string SenhaHash { get; private set; } = string.Empty;

    public bool Ativo { get; private set; }

    public IReadOnlyCollection<Papel> Papeis => _papeis.AsReadOnly();

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public static Usuario Criar(string nome, string email, string senhaHash)
        => new(nome.Trim(), Normalizar(email), senhaHash);

    public void DefinirSenha(string senhaHash) => SenhaHash = senhaHash;

    public void Ativar() => Ativo = true;

    public void Inativar() => Ativo = false;

    public void AtribuirPapel(Papel papel)
    {
        if (_papeis.Exists(p => p.Nome == papel.Nome))
        {
            return;
        }

        _papeis.Add(papel);
    }

    public void AdicionarRefreshToken(RefreshToken token) => _refreshTokens.Add(token);

    public static string Normalizar(string email) => email.Trim().ToLowerInvariant();
}
