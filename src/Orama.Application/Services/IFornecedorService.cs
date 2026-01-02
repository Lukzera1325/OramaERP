using Orama.Domain.Entities;

namespace Orama.Application.Services;

/// <summary>
/// Interface para serviços de fornecedor
/// </summary>
public interface IFornecedorService
{
    /// <summary>
    /// Obtém todos os fornecedores ativos de uma empresa
    /// </summary>
    Task<IEnumerable<Fornecedor>> ObterTodosAsync(int empresaId);
    
    /// <summary>
    /// Obtém um fornecedor por ID
    /// </summary>
    Task<Fornecedor?> ObterPorIdAsync(int id, int empresaId);
    
    /// <summary>
    /// Obtém um fornecedor por CNPJ
    /// </summary>
    Task<Fornecedor?> ObterPorCnpjAsync(string cnpj, int empresaId);
    
    /// <summary>
    /// Inclui um novo fornecedor
    /// </summary>
    Task<Fornecedor> IncluirAsync(Fornecedor fornecedor);
    
    /// <summary>
    /// Altera um fornecedor existente
    /// </summary>
    Task<Fornecedor> AlterarAsync(Fornecedor fornecedor);
    
    /// <summary>
    /// Exclui um fornecedor (soft delete)
    /// </summary>
    Task<bool> ExcluirAsync(int id, int empresaId);
    
    /// <summary>
    /// Verifica se um CNPJ já está cadastrado
    /// </summary>
    Task<bool> CnpjJaCadastradoAsync(string cnpj, int empresaId, int? fornecedorId = null);
}