using Orama.Domain.Entities;

namespace Orama.Application.Services
{
    public interface IApontamentoHorasService
    {
        Task<IEnumerable<ApontamentoHoras>> ObterTodosAsync(int empresaId);
        Task<ApontamentoHoras?> ObterPorIdAsync(int id, int empresaId);
        Task<ApontamentoHoras> CriarAsync(ApontamentoHoras apontamento);
        Task<ApontamentoHoras> AtualizarAsync(ApontamentoHoras apontamento);
        Task<bool> ExcluirAsync(int id, int empresaId);
        
        // Operações específicas
        Task<ApontamentoHoras> IniciarApontamentoAsync(int ordemProducaoId, int? etapaId, int funcionarioId, 
            TipoApontamento tipo, int usuarioCriacaoId, int empresaId);
        Task<ApontamentoHoras> FinalizarApontamentoAsync(int id, int empresaId, string? observacoes = null);
        
        // Consultas
        Task<IEnumerable<ApontamentoHoras>> ObterPorOrdemProducaoAsync(int ordemProducaoId, int empresaId);
        Task<IEnumerable<ApontamentoHoras>> ObterPorFuncionarioAsync(int funcionarioId, int empresaId, 
            DateTime? dataInicio = null, DateTime? dataFim = null);
        Task<IEnumerable<ApontamentoHoras>> ObterPorPeriodoAsync(int empresaId, DateTime dataInicio, DateTime dataFim);
        
        // Relatórios
        Task<decimal> CalcularHorasTrabalhadasAsync(int ordemProducaoId, int empresaId);
        Task<decimal> CalcularCustoMaoObraAsync(int ordemProducaoId, int empresaId);
        Task<decimal> CalcularHorasFuncionarioAsync(int funcionarioId, int empresaId, DateTime dataInicio, DateTime dataFim);
        
        // Dashboard
        Task<decimal> ObterHorasTrabalhadasHojeAsync(int empresaId);
        Task<decimal> ObterCustoMaoObraMesAsync(int empresaId, int mes, int ano);
        Task<IEnumerable<(string NomeFuncionario, decimal HorasTrabalhadas)>> 
            ObterRankingFuncionariosAsync(int empresaId, DateTime dataInicio, DateTime dataFim);
    }
}