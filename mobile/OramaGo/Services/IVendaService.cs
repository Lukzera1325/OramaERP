using OramaGo.Models;

namespace OramaGo.Services;

public interface IVendaService : IBaseService<VendaLocal>
{
    // Métodos específicos de vendas
    Task<IEnumerable<VendaLocal>> GetByClienteIdAsync(int clienteId);
    Task<IEnumerable<VendaLocal>> GetByStatusAsync(StatusVendaLocal status);
    Task<IEnumerable<VendaLocal>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim);
    Task<IEnumerable<VendaLocal>> GetPendentesAsync();
    Task<string> GerarNumeroVendaAsync();
    
    // Métodos de cálculo
    Task<decimal> CalcularSubTotalAsync(int vendaId);
    Task<decimal> CalcularTotalAsync(int vendaId);
    Task RecalcularVendaAsync(int vendaId);
    
    // Métodos de status
    Task<VendaLocal> AprovarVendaAsync(int vendaId);
    Task<VendaLocal> CancelarVendaAsync(int vendaId, string motivo);
    Task<VendaLocal> FaturarVendaAsync(int vendaId);
    
    // Validações
    Task<bool> ValidarEstoqueAsync(int vendaId);
    Task<bool> PodeEditarAsync(int vendaId);
    Task<bool> PodeAprovarAsync(int vendaId);
    Task<bool> PodeCancelarAsync(int vendaId);
    
    // Estatísticas
    Task<decimal> GetTotalVendasMesAsync(int mes, int ano);
    Task<int> GetQuantidadeVendasMesAsync(int mes, int ano);
    Task<IEnumerable<VendaLocal>> GetVendasRecentesAsync(int quantidade = 10);
}