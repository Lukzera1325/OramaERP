using Orama.Domain.Entities;

namespace Orama.Application.Services;

/// <summary>
/// Interface para serviços de cliente
/// </summary>
public interface IClienteService
{
    /// <summary>
    /// Obtém todos os clientes ativos
    /// </summary>
    Task<IEnumerable<Cliente>> ObterTodosAsync();
    
    /// <summary>
    /// Obtém todos os clientes ativos por empresa
    /// </summary>
    Task<IEnumerable<Cliente>> ObterTodosAsync(int empresaId);
    
    /// <summary>
    /// Obtém um cliente por ID
    /// </summary>
    Task<Cliente?> ObterPorIdAsync(int id);
    
    /// <summary>
    /// Obtém um cliente por ID e empresa
    /// </summary>
    Task<Cliente?> ObterPorIdAsync(int id, int empresaId);
    
    /// <summary>
    /// Obtém um cliente por CPF/CNPJ
    /// </summary>
    Task<Cliente?> ObterPorCpfCnpjAsync(string cpfCnpj);
    
    /// <summary>
    /// Obtém um cliente por CPF/CNPJ e empresa
    /// </summary>
    Task<Cliente?> ObterPorCpfCnpjAsync(string cpfCnpj, int empresaId);
    
    /// <summary>
    /// Cria um novo cliente
    /// </summary>
    Task<Cliente> CriarAsync(Cliente cliente);
    
    /// <summary>
    /// Atualiza um cliente existente
    /// </summary>
    Task<Cliente> AtualizarAsync(Cliente cliente);
    
    /// <summary>
    /// Exclui um cliente (soft delete)
    /// </summary>
    Task ExcluirAsync(int id);
    
    /// <summary>
    /// Verifica se um CPF/CNPJ já está em uso
    /// </summary>
    Task<bool> CpfCnpjExisteAsync(string cpfCnpj, int? clienteId = null);
    
    /// <summary>
    /// Busca clientes por nome ou CPF/CNPJ
    /// </summary>
    Task<IEnumerable<Cliente>> BuscarAsync(string termo);
    
    /// <summary>
    /// Consulta dados do CNPJ na Receita Federal
    /// </summary>
    Task<DadosCnpj?> ConsultarCnpjAsync(string cnpj);
}