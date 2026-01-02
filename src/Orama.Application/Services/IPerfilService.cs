using Orama.Domain.Entities;

namespace Orama.Application.Services;

/// <summary>
/// Interface para serviço de gerenciamento de perfis
/// </summary>
public interface IPerfilService
{
    /// <summary>
    /// Obtém todos os perfis ativos
    /// </summary>
    Task<IEnumerable<Perfil>> ObterTodosAsync();

    /// <summary>
    /// Obtém um perfil por ID
    /// </summary>
    Task<Perfil?> ObterPorIdAsync(int id);

    /// <summary>
    /// Obtém um perfil por nome
    /// </summary>
    Task<Perfil?> ObterPorNomeAsync(string nome);

    /// <summary>
    /// Cria um novo perfil
    /// </summary>
    Task<Perfil> CriarAsync(Perfil perfil);

    /// <summary>
    /// Atualiza um perfil existente
    /// </summary>
    Task<Perfil> AtualizarAsync(Perfil perfil);

    /// <summary>
    /// Exclui um perfil (soft delete)
    /// </summary>
    Task<bool> ExcluirAsync(int id);

    /// <summary>
    /// Verifica se um perfil pode ser excluído (não tem usuários vinculados)
    /// </summary>
    Task<bool> PodeExcluirAsync(int id);

    /// <summary>
    /// Obtém as permissões de um perfil
    /// </summary>
    Task<IEnumerable<Permissao>> ObterPermissoesAsync(int perfilId);

    /// <summary>
    /// Atualiza as permissões de um perfil
    /// </summary>
    Task AtualizarPermissoesAsync(int perfilId, IEnumerable<int> permissaoIds);
}