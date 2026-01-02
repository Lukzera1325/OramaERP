using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

/// <summary>
/// Serviço para gerenciamento de usuários
/// </summary>
public class UsuarioService : IUsuarioService
{
    private readonly OramaDbContext _context;
    private readonly IAuthService _authService;

    public UsuarioService(OramaDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    /// <summary>
    /// Obtém todos os usuários ativos
    /// </summary>
    public async Task<IEnumerable<Usuario>> ObterTodosAsync()
    {
        return await _context.Usuarios
            .Include(u => u.Perfil)
            .Where(u => u.Ativo)
            .OrderBy(u => u.Nome)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém um usuário por ID
    /// </summary>
    public async Task<Usuario?> ObterPorIdAsync(int id)
    {
        return await _context.Usuarios
            .Include(u => u.Perfil)
            .FirstOrDefaultAsync(u => u.Id == id && u.Ativo);
    }

    /// <summary>
    /// Obtém um usuário por email
    /// </summary>
    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        return await _context.Usuarios
            .Include(u => u.Perfil)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.Ativo);
    }

    /// <summary>
    /// Cria um novo usuário
    /// </summary>
    public async Task<Usuario> CriarAsync(Usuario usuario)
    {
        // Verificar se email já existe
        if (await EmailExisteAsync(usuario.Email))
        {
            throw new InvalidOperationException("Email já está em uso por outro usuário.");
        }

        // Criptografar senha
        usuario.Senha = _authService.CriptografarSenha(usuario.Senha);
        
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        
        return usuario;
    }

    /// <summary>
    /// Atualiza um usuário existente
    /// </summary>
    public async Task<Usuario> AtualizarAsync(Usuario usuario)
    {
        var usuarioExistente = await _context.Usuarios.FindAsync(usuario.Id);
        if (usuarioExistente == null)
        {
            throw new InvalidOperationException("Usuário não encontrado.");
        }

        // Verificar se email já existe (exceto para o próprio usuário)
        if (await EmailExisteAsync(usuario.Email, usuario.Id))
        {
            throw new InvalidOperationException("Email já está em uso por outro usuário.");
        }

        // Atualizar propriedades
        usuarioExistente.Nome = usuario.Nome;
        usuarioExistente.Email = usuario.Email;
        usuarioExistente.Telefone = usuario.Telefone;
        usuarioExistente.PerfilId = usuario.PerfilId;

        // Atualizar senha apenas se foi fornecida uma nova
        if (!string.IsNullOrWhiteSpace(usuario.Senha))
        {
            usuarioExistente.Senha = _authService.CriptografarSenha(usuario.Senha);
        }

        await _context.SaveChangesAsync();
        return usuarioExistente;
    }

    /// <summary>
    /// Exclui um usuário (soft delete)
    /// </summary>
    public async Task ExcluirAsync(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario != null)
        {
            usuario.Ativo = false;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Verifica se um email já está em uso
    /// </summary>
    public async Task<bool> EmailExisteAsync(string email, int? usuarioId = null)
    {
        var query = _context.Usuarios.Where(u => u.Email.ToLower() == email.ToLower() && u.Ativo);
        
        if (usuarioId.HasValue)
        {
            query = query.Where(u => u.Id != usuarioId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Atualiza a data do último acesso do usuário
    /// </summary>
    public async Task AtualizarUltimoAcessoAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario != null)
        {
            usuario.DataUltimoAcesso = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
}