using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Application.Identity.Models;
using SistemaAN.Domain.Identity;

namespace SistemaAN.Application.Identity;

public sealed class UsuarioService : IUsuarioService
{
    private const int SenhaMinima = 6;

    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public UsuarioService(IApplicationDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public IReadOnlyList<PerfilDto> ListarPerfis()
        => PapeisDoSistema.Todos.Select(p => new PerfilDto(p.Key, p.Value)).ToList();

    public async Task<IReadOnlyList<UsuarioDto>> ListarAsync(string? busca, string? perfil, bool? ativo, CancellationToken cancellationToken = default)
    {
        var q = _db.Usuarios.Include(u => u.Papeis).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var t = busca.Trim();
            q = q.Where(u => u.Nome.Contains(t) || u.Email.Contains(t));
        }
        if (!string.IsNullOrWhiteSpace(perfil))
        {
            q = q.Where(u => u.Papeis.Any(p => p.Nome == perfil));
        }
        if (ativo.HasValue)
        {
            q = q.Where(u => u.Ativo == ativo.Value);
        }

        var usuarios = await q.OrderBy(u => u.Nome).ToListAsync(cancellationToken);
        return usuarios.Select(Map).ToList();
    }

    public async Task<UsuarioDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var usuario = await _db.Usuarios.Include(u => u.Papeis).AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new NotFoundException("Usuário", id);
        return Map(usuario);
    }

    public async Task<UsuarioDto> CriarAsync(CriarUsuarioRequest request, CancellationToken cancellationToken = default)
    {
        var erros = new Dictionary<string, string[]>();
        var nome = (request.Nome ?? string.Empty).Trim();
        var email = Usuario.Normalizar(request.Email ?? string.Empty);

        if (string.IsNullOrWhiteSpace(nome)) erros["nome"] = ["Informe o nome."];
        if (!EmailValido(email)) erros["email"] = ["Informe um e-mail válido."];
        if (string.IsNullOrWhiteSpace(request.Senha) || request.Senha.Length < SenhaMinima)
            erros["senha"] = [$"A senha deve ter ao menos {SenhaMinima} caracteres."];
        if (!PapeisDoSistema.Existe(request.Perfil ?? string.Empty))
            erros["perfil"] = ["Perfil inválido."];
        if (erros.Count == 0 && await _db.Usuarios.AnyAsync(u => u.Email == email, cancellationToken))
            erros["email"] = ["Já existe um usuário com este e-mail."];

        if (erros.Count > 0) throw new ValidationException(erros);

        var papel = await ObterOuCriarPapelAsync(request.Perfil, cancellationToken);
        var usuario = Usuario.Criar(nome, email, _passwordHasher.Hash(request.Senha));
        usuario.AtualizarCadastro(nome, request.Telefone, request.Observacoes);
        usuario.DefinirPapelUnico(papel);

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(usuario.Id, cancellationToken);
    }

    public async Task<UsuarioDto> AtualizarAsync(long id, AtualizarUsuarioRequest request, CancellationToken cancellationToken = default)
    {
        var usuario = await _db.Usuarios.Include(u => u.Papeis)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new NotFoundException("Usuário", id);

        var erros = new Dictionary<string, string[]>();
        var nome = (request.Nome ?? string.Empty).Trim();
        var email = Usuario.Normalizar(request.Email ?? string.Empty);

        if (string.IsNullOrWhiteSpace(nome)) erros["nome"] = ["Informe o nome."];
        if (!EmailValido(email)) erros["email"] = ["Informe um e-mail válido."];
        if (!PapeisDoSistema.Existe(request.Perfil ?? string.Empty)) erros["perfil"] = ["Perfil inválido."];
        if (erros.Count == 0 && email != usuario.Email && await _db.Usuarios.AnyAsync(u => u.Email == email && u.Id != id, cancellationToken))
            erros["email"] = ["Já existe um usuário com este e-mail."];

        if (erros.Count > 0) throw new ValidationException(erros);

        var eraAdmin = usuario.Papeis.Any(p => p.Nome == PapeisDoSistema.Administrador);
        var continuaAdmin = request.Perfil == PapeisDoSistema.Administrador;
        if (eraAdmin && !continuaAdmin)
        {
            await GarantirNaoEhUltimoAdminAsync(id, cancellationToken);
        }

        usuario.AtualizarCadastro(nome, request.Telefone, request.Observacoes);
        usuario.AlterarEmail(email);

        var perfilAtual = usuario.Papeis.Select(p => p.Nome).FirstOrDefault();
        if (perfilAtual != request.Perfil)
        {
            var papel = await ObterOuCriarPapelAsync(request.Perfil, cancellationToken);
            usuario.DefinirPapelUnico(papel);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(usuario.Id, cancellationToken);
    }

    public async Task<UsuarioDto> AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default)
    {
        var usuario = await _db.Usuarios.Include(u => u.Papeis)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new NotFoundException("Usuário", id);

        if (!ativo && usuario.Papeis.Any(p => p.Nome == PapeisDoSistema.Administrador))
        {
            await GarantirNaoEhUltimoAdminAsync(id, cancellationToken);
        }

        if (ativo) usuario.Ativar();
        else usuario.Inativar();

        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(usuario.Id, cancellationToken);
    }

    public async Task RedefinirSenhaAsync(long id, string novaSenha, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(novaSenha) || novaSenha.Length < SenhaMinima)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["novaSenha"] = [$"A senha deve ter ao menos {SenhaMinima} caracteres."],
            });
        }

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new NotFoundException("Usuário", id);

        usuario.DefinirSenha(_passwordHasher.Hash(novaSenha));
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task GarantirNaoEhUltimoAdminAsync(long usuarioId, CancellationToken cancellationToken)
    {
        var outrosAdminsAtivos = await _db.Usuarios
            .CountAsync(u => u.Id != usuarioId && u.Ativo && u.Papeis.Any(p => p.Nome == PapeisDoSistema.Administrador), cancellationToken);
        if (outrosAdminsAtivos == 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["perfil"] = ["Não é possível remover/inativar o último administrador ativo."],
            });
        }
    }

    private async Task<Papel> ObterOuCriarPapelAsync(string nome, CancellationToken cancellationToken)
    {
        var papel = await _db.Papeis.FirstOrDefaultAsync(p => p.Nome == nome, cancellationToken);
        if (papel is null)
        {
            PapeisDoSistema.Todos.TryGetValue(nome, out var descricao);
            papel = Papel.Criar(nome, descricao);
            _db.Papeis.Add(papel);
        }
        return papel;
    }

    private static bool EmailValido(string email)
        => !string.IsNullOrWhiteSpace(email) && email.Contains('@') && email.Contains('.') && email.Length >= 5;

    private static UsuarioDto Map(Usuario u) => new(
        u.Id, u.Nome, u.Email, u.Telefone, u.Observacoes,
        u.Papeis.Select(p => p.Nome).FirstOrDefault() ?? string.Empty,
        u.Ativo, u.UpdatedAt);
}
