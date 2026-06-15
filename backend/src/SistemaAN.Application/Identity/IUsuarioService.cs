using SistemaAN.Application.Identity.Models;

namespace SistemaAN.Application.Identity;

/// <summary>Gestão de usuários e perfis (somente Administrador).</summary>
public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioDto>> ListarAsync(string? busca, string? perfil, bool? ativo, CancellationToken cancellationToken = default);

    Task<UsuarioDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<UsuarioDto> CriarAsync(CriarUsuarioRequest request, CancellationToken cancellationToken = default);

    Task<UsuarioDto> AtualizarAsync(long id, AtualizarUsuarioRequest request, CancellationToken cancellationToken = default);

    Task<UsuarioDto> AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default);

    Task RedefinirSenhaAsync(long id, string novaSenha, CancellationToken cancellationToken = default);

    IReadOnlyList<PerfilDto> ListarPerfis();
}
