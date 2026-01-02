using Orama.Domain.Entities;

namespace Orama.Application.Services
{
    public interface IInspecaoQualidadeService
    {
        Task<IEnumerable<InspecaoQualidade>> ObterTodosAsync(int empresaId);
        Task<InspecaoQualidade?> ObterPorIdAsync(int id, int empresaId);
        Task<InspecaoQualidade> CriarAsync(InspecaoQualidade inspecao);
        Task<InspecaoQualidade> AtualizarAsync(InspecaoQualidade inspecao);
        Task<bool> ExcluirAsync(int id, int empresaId);
        
        // Consultas
        Task<IEnumerable<InspecaoQualidade>> ObterPorOrdemProducaoAsync(int ordemProducaoId, int empresaId);
        Task<IEnumerable<InspecaoQualidade>> ObterPorTipoAsync(int empresaId, TipoInspecao tipo);
        Task<IEnumerable<InspecaoQualidade>> ObterPorResultadoAsync(int empresaId, ResultadoInspecao resultado);
        Task<IEnumerable<InspecaoQualidade>> ObterPorPeriodoAsync(int empresaId, DateTime dataInicio, DateTime dataFim);
        
        // Relatórios de qualidade
        Task<decimal> CalcularPercentualAprovacaoAsync(int empresaId, DateTime dataInicio, DateTime dataFim);
        Task<decimal> CalcularPercentualRejeicaoAsync(int empresaId, DateTime dataInicio, DateTime dataFim);
        Task<IEnumerable<(string TipoInspecao, int Quantidade, decimal PercentualAprovacao)>> 
            ObterEstatisticasPorTipoAsync(int empresaId, DateTime dataInicio, DateTime dataFim);
        
        // Dashboard
        Task<int> ObterQuantidadeInspecoesPendentesAsync(int empresaId);
        Task<decimal> ObterPercentualQualidadeMesAsync(int empresaId, int mes, int ano);
        Task<IEnumerable<(string Produto, int QuantidadeRejeitada)>> 
            ObterProdutosMaisRejeitadosAsync(int empresaId, DateTime dataInicio, DateTime dataFim, int top = 10);
    }
}