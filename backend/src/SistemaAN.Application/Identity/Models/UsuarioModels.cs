namespace SistemaAN.Application.Identity.Models;

/// <summary>Usuário para listagem/detalhe (perfil único nesta fase).</summary>
public sealed record UsuarioDto(
    long Id,
    string Nome,
    string Email,
    string? Telefone,
    string? Observacoes,
    string Perfil,
    bool Ativo,
    DateTimeOffset? AtualizadoEm);

public sealed record CriarUsuarioRequest(
    string Nome,
    string Email,
    string Senha,
    string Perfil,
    string? Telefone,
    string? Observacoes);

public sealed record AtualizarUsuarioRequest(
    string Nome,
    string Email,
    string Perfil,
    string? Telefone,
    string? Observacoes);

public sealed record RedefinirSenhaRequest(string NovaSenha);

public sealed record AlternarStatusUsuarioRequest(bool Ativo);

public sealed record PerfilDto(string Nome, string Descricao);
