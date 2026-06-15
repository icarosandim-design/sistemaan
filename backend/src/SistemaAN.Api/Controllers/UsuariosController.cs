using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Identity;
using SistemaAN.Application.Identity.Models;
using SistemaAN.Domain.Identity;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize(Roles = PapeisDoSistema.Administrador)]
[Route("api/usuarios")]
public sealed class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;

    public UsuariosController(IUsuarioService service) => _service = service;

    /// <summary>Lista os usuários (filtros opcionais por busca, perfil e status).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> Listar(
        [FromQuery] string? busca, [FromQuery] string? perfil, [FromQuery] bool? ativo, CancellationToken ct)
        => Ok(await _service.ListarAsync(busca, perfil, ativo, ct));

    /// <summary>Perfis fixos disponíveis.</summary>
    [HttpGet("perfis")]
    public ActionResult<IReadOnlyList<PerfilDto>> Perfis() => Ok(_service.ListarPerfis());

    /// <summary>Obtém um usuário.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<UsuarioDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria um usuário.</summary>
    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Criar(CriarUsuarioRequest request, CancellationToken ct)
        => Ok(await _service.CriarAsync(request, ct));

    /// <summary>Atualiza dados/cadastro e perfil de um usuário.</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<UsuarioDto>> Atualizar(long id, AtualizarUsuarioRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Ativa/inativa um usuário (sem exclusão física).</summary>
    [HttpPut("{id:long}/status")]
    public async Task<ActionResult<UsuarioDto>> AlternarStatus(long id, AlternarStatusUsuarioRequest request, CancellationToken ct)
        => Ok(await _service.AlternarStatusAsync(id, request.Ativo, ct));

    /// <summary>Redefine a senha de um usuário.</summary>
    [HttpPut("{id:long}/senha")]
    public async Task<IActionResult> RedefinirSenha(long id, RedefinirSenhaRequest request, CancellationToken ct)
    {
        await _service.RedefinirSenhaAsync(id, request.NovaSenha, ct);
        return NoContent();
    }
}
