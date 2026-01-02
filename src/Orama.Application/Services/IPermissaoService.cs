using Orama.Domain.Entities;

namespace Orama.Application.Services;

/// <summary>
/// Interface para serviço de gerenciamento de permissões
/// </summary>
public interface IPermissaoService
{
    /// <summary>
    /// Obtém todas as permissões ativas
    /// </summary>
    Task<IEnumerable<Permissao>> ObterTodasAsync();

    /// <summary>
    /// Obtém uma permissão por ID
    /// </summary>
    Task<Permissao?> ObterPorIdAsync(int id);

    /// <summary>
    /// Obtém permissões por módulo
    /// </summary>
    Task<IEnumerable<Permissao>> ObterPorModuloAsync(string modulo);

    /// <summary>
    /// Obtém todos os módulos disponíveis
    /// </summary>
    Task<IEnumerable<string>> ObterModulosAsync();

    /// <summary>
    /// Cria uma nova permissão
    /// </summary>
    Task<Permissao> CriarAsync(Permissao permissao);

    /// <summary>
    /// Atualiza uma permissão existente
    /// </summary>
    Task<Permissao> AtualizarAsync(Permissao permissao);

    /// <summary>
    /// Exclui uma permissão (soft delete)
    /// </summary>
    Task<bool> ExcluirAsync(int id);

    /// <summary>
    /// Verifica se uma permissão pode ser excluída
    /// </summary>
    Task<bool> PodeExcluirAsync(int id);

    /// <summary>
    /// Verifica se um usuário tem uma permissão específica
    /// </summary>
    Task<bool> UsuarioTemPermissaoAsync(int usuarioId, string nomePermissao);

    /// <summary>
    /// Obtém todas as permissões de um usuário
    /// </summary>
    Task<IEnumerable<string>> ObterPermissoesUsuarioAsync(int usuarioId);
}