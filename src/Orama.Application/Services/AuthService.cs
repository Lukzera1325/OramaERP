using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

/// <summary>
/// Serviço de autenticação e autorização
/// </summary>
public class AuthService : IAuthService
{
    private readonly OramaDbContext _context;

    public AuthService(OramaDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Autentica um usuário com email e senha
    /// </summary>
    public async Task<Usuario?> AutenticarAsync(string email, string senha)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            return null;

        var usuario = await _context.Usuarios
            .Include(u => u.Perfil)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.Ativo);

        if (usuario == null)
            return null;

        // Verificar senha
        if (!VerificarSenha(senha, usuario.Senha))
            return null;

        // Atualizar último login
        usuario.UltimoLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return usuario;
    }

    /// <summary>
    /// Valida credenciais do usuário (para API)
    /// </summary>
    public async Task<Usuario?> ValidarCredenciaisAsync(string email, string senha)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            return null;

        var usuario = await _context.Usuarios
            .Include(u => u.Perfil)
            .Include(u => u.Empresa)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.Ativo);

        if (usuario == null)
            return null;

        // Verificar senha
        if (!VerificarSenha(senha, usuario.Senha))
            return null;

        // Carregar permissões do usuário
        usuario.Permissoes = (await ObterPermissoesUsuarioAsync(usuario.Id)).ToList();

        return usuario;
    }

    /// <summary>
    /// Verifica se o usuário tem uma permissão específica
    /// </summary>
    public async Task<bool> TemPermissaoAsync(int usuarioId, string nomePermissao)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Perfil)
                .ThenInclude(p => p.PerfilPermissoes)
                    .ThenInclude(pp => pp.Permissao)
            .FirstOrDefaultAsync(u => u.Id == usuarioId && u.Ativo);

        if (usuario == null)
            return false;

        // Administrador tem todas as permissões
        if (usuario.Perfil.Nome == "Administrador")
            return true;

        // Verificar permissão específica
        return usuario.Perfil.PerfilPermissoes
            .Any(pp => pp.Permissao.Nome == nomePermissao && pp.Concedida && pp.Ativo);
    }

    /// <summary>
    /// Obtém todas as permissões de um usuário
    /// </summary>
    public async Task<IEnumerable<string>> ObterPermissoesUsuarioAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Perfil)
                .ThenInclude(p => p.PerfilPermissoes)
                    .ThenInclude(pp => pp.Permissao)
            .FirstOrDefaultAsync(u => u.Id == usuarioId && u.Ativo);

        if (usuario == null)
            return Enumerable.Empty<string>();

        // Administrador tem todas as permissões
        if (usuario.Perfil.Nome == "Administrador")
        {
            return await _context.Permissoes
                .Where(p => p.Ativo)
                .Select(p => p.Nome)
                .ToListAsync();
        }

        // Retornar permissões específicas do perfil
        return usuario.Perfil.PerfilPermissoes
            .Where(pp => pp.Concedida && pp.Ativo)
            .Select(pp => pp.Permissao.Nome)
            .ToList();
    }

    /// <summary>
    /// Obtém todas as empresas que o usuário tem acesso
    /// </summary>
    public async Task<IEnumerable<Empresa>> ObterEmpresasUsuarioAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Empresas)
                .ThenInclude(ue => ue.Empresa)
            .FirstOrDefaultAsync(u => u.Id == usuarioId && u.Ativo);

        if (usuario == null)
            return Enumerable.Empty<Empresa>();

        return usuario.Empresas
            .Where(ue => ue.Empresa.Ativo)
            .Select(ue => ue.Empresa)
            .ToList();
    }

    /// <summary>
    /// Criptografa uma senha usando BCrypt
    /// </summary>
    public string CriptografarSenha(string senha)
    {
        return BCrypt.Net.BCrypt.HashPassword(senha);
    }

    /// <summary>
    /// Verifica se uma senha corresponde ao hash
    /// </summary>
    public bool VerificarSenha(string senha, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(senha, hash);
        }
        catch
        {
            return false;
        }
    }
}