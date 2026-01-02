using Orama.Domain.Entities;

namespace Orama.Application.Services;

/// <summary>
/// Interface para serviços de usuário
/// </summary>
public interface IUsuarioService
{
    /// <summary>
    /// Obtém todos os usuários ativos
    /// </summary>
    Task<IEnumerable<Usuario>> ObterTodosAsync();
    
    /// <summary>
    /// Obtém um usuário por ID
    /// </summary>
    Task<Usuario?> ObterPorIdAsync(int id);
    
    /// <summary>
    /// Obtém um usuário por email
    /// </summary>
    Task<Usuario?> ObterPorEmailAsync(string email);
    
    /// <summary>
    /// Cria um novo usuário
    /// </summary>
    Task<Usuario> CriarAsync(Usuario usuario);
    
    /// <summary>
    /// Atualiza um usuário existente
    /// </summary>
    Task<Usuario> AtualizarAsync(Usuario usuario);
    
    /// <summary>
    /// Exclui um usuário (soft delete)
    /// </summary>
    Task ExcluirAsync(int id);
    
    /// <summary>
    /// Verifica se um email já está em uso
    /// </summary>
    Task<bool> EmailExisteAsync(string email, int? usuarioId = null);
    
    /// <summary>
    /// Atualiza a data do último acesso do usuário
    /// </summary>
    Task AtualizarUltimoAcessoAsync(int usuarioId);
}