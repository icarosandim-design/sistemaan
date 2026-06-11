namespace SistemaAN.Application.Identity.Models;

/// <summary>Credenciais de login.</summary>
public sealed record LoginRequest(string Email, string Senha);

/// <summary>Payload para renovação/logout com refresh token.</summary>
public sealed record RefreshTokenRequest(string RefreshToken);

/// <summary>Resultado de uma autenticação bem-sucedida.</summary>
public sealed record AuthResult(
    string AccessToken,
    DateTimeOffset AccessTokenExpiraEm,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiraEm,
    UsuarioAutenticado Usuario);

/// <summary>Resumo do usuário autenticado retornado ao cliente.</summary>
public sealed record UsuarioAutenticado(
    long Id,
    string Nome,
    string Email,
    IReadOnlyCollection<string> Papeis);
