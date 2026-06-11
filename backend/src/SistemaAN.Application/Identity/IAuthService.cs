using SistemaAN.Application.Identity.Models;

namespace SistemaAN.Application.Identity;

/// <summary>Orquestra o fluxo de autenticação: login, refresh e logout.</summary>
public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}
