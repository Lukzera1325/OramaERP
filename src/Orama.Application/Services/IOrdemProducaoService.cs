using Orama.Domain.Entities;

namespace Orama.Application.Services
{
    public interface IOrdemProducaoService
    {
        Task<IEnumerable<OrdemProducao>> ObterTodosAsync(int empresaId);
        Task<OrdemProducao?> ObterPorIdAsync(int id, int empresaId);
        Task<OrdemProducao> CriarAsync(OrdemProducao ordemProducao);
        Task<OrdemProducao> AtualizarAsync(OrdemProducao ordemProducao);
        Task<bool> ExcluirAsync(int id, int empresaId);
        Task<string> GerarProximoNumeroAsync(int empresaId);
        
        // Operações específicas
        Task<bool> LiberarOrdemAsync(int id, int empresaId, int usuarioId);
        Task<bool> IniciarProducaoAsync(int id, int empresaId, int usuarioId);
        Task<bool> PausarProducaoAsync(int id, int empresaId, int usuarioId, string motivo);
        Task<bool> FinalizarProducaoAsync(int id, int empresaId, int usuarioId);
        Task<bool> CancelarOrdemAsync(int id, int empresaId, int usuarioId, string motivo);
        
        // Relatórios e consultas
        Task<IEnumerable<OrdemProducao>> ObterPorStatusAsync(int empresaId, StatusOrdemProducao status);
        Task<IEnumerable<OrdemProducao>> ObterPorPeriodoAsync(int empresaId, DateTime dataInicio, DateTime dataFim);
        Task<IEnumerable<OrdemProducao>> ObterPorProdutoAsync(int empresaId, int produtoId);
        Task<decimal> CalcularCustoTotalAsync(int id, int empresaId);
        
        // Dashboard
        Task<int> ObterQuantidadeOrdensAtivasAsync(int empresaId);
        Task<int> ObterQuantidadeOrdensAtrasadasAsync(int empresaId);
        Task<decimal> ObterCustoProducaoMesAsync(int empresaId, int mes, int ano);
    }
}