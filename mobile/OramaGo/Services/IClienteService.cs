using OramaGo.Models;

namespace OramaGo.Services;

public interface IClienteService
{
    Task<IEnumerable<ClienteLocal>> GetAllAsync();
    Task<ClienteLocal?> GetByIdAsync(int id);
    Task<IEnumerable<ClienteLocal>> SearchAsync(string termo);
    Task<ClienteLocal> CreateAsync(ClienteLocal cliente);
    Task<ClienteLocal> UpdateAsync(ClienteLocal cliente);
    Task<bool> DeleteAsync(int id);
    Task<int> GetCountAsync();
    Task<IEnumerable<ClienteLocal>> GetPendingSyncAsync();
    Task<bool> ExistsByCpfCnpjAsync(string cpfCnpj, int? excludeId = null);
    Task<IEnumerable<ClienteLocal>> GetByVendedorAsync(int vendedorId);
    Task<decimal> GetTotalCreditoDisponivelAsync();
}