using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Identity;
using SistemaAN.Application.Identity.Models;
using SistemaAN.Domain.Identity;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Autentica por e-mail e senha, retornando access token e refresh token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResult>> Login(LoginRequest request, CancellationToken cancellationToken)
        => Ok(await _authService.LoginAsync(request, cancellationToken));

    /// <summary>Renova os tokens a partir de um refresh token válido (rotação).</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResult>> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
        => Ok(await _authService.RefreshAsync(request.RefreshToken, cancellationToken));

    /// <summary>Revoga o refresh token informado (logout).</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    /// <summary>Retorna os dados do usuário autenticado (a partir das claims do token).</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(new
    {
        id = User.FindFirstValue("sub"),
        nome = User.FindFirstValue("nome"),
        email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email"),
        papeis = User.FindAll(JwtRoleClaim).Select(c => c.Value),
    });

    /// <summary>Endpoint de exemplo de autorização por papel (somente Administrador).</summary>
    [HttpGet("admin-check")]
    [Authorize(Roles = PapeisDoSistema.Administrador)]
    public IActionResult AdminCheck() => Ok(new { message = "Acesso de administrador confirmado." });

    private const string JwtRoleClaim = "role";
}
