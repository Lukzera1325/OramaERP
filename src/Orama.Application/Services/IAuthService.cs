using Orama.Domain.Entities;

namespace Orama.Application.Services;

/// <summary>
/// Interface para serviços de autenticação
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Autentica um usuário com email e senha
    /// </summary>
    Task<Usuario?> AutenticarAsync(string email, string senha);
    
    /// <summary>
    /// Valida credenciais do usuário (para API)
    /// </summary>
    Task<Usuario?> ValidarCredenciaisAsync(string email, string senha);
    
    /// <summary>
    /// Verifica se o usuário tem uma permissão específica
    /// </summary>
    Task<bool> TemPermissaoAsync(int usuarioId, string nomePermissao);
    
    /// <summary>
    /// Obtém todas as permissões de um usuário
    /// </summary>
    Task<IEnumerable<string>> ObterPermissoesUsuarioAsync(int usuarioId);
    
    /// <summary>
    /// Obtém todas as empresas que o usuário tem acesso
    /// </summary>
    Task<IEnumerable<Empresa>> ObterEmpresasUsuarioAsync(int usuarioId);
    
    /// <summary>
    /// Criptografa uma senha
    /// </summary>
    string CriptografarSenha(string senha);
    
    /// <summary>
    /// Verifica se uma senha corresponde ao hash
    /// </summary>
    bool VerificarSenha(string senha, string hash);
}