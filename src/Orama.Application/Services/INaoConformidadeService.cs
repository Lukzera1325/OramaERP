using Orama.Domain.Entities;

namespace Orama.Application.Services
{
    public interface INaoConformidadeService
    {
        Task<IEnumerable<NaoConformidade>> ObterTodosAsync(int empresaId);
        Task<NaoConformidade?> ObterPorIdAsync(int id, int empresaId);
        Task<NaoConformidade> CriarAsync(NaoConformidade naoConformidade);
        Task<NaoConformidade> AtualizarAsync(NaoConformidade naoConformidade);
        Task<bool> ExcluirAsync(int id, int empresaId);
        Task<string> GerarProximoNumeroAsync(int empresaId);
        
        // Operações específicas
        Task<bool> AtribuirResponsavelAsync(int id, int responsavelId, int empresaId);
        Task<bool> DefinirAcaoCorretivaAsync(int id, string acaoCorretiva, DateTime dataPrazo, int empresaId);
        Task<bool> ResolverNaoConformidadeAsync(int id, string observacoesResolucao, int empresaId);
        Task<bool> FecharNaoConformidadeAsync(int id, int empresaId);
        
        // Consultas
        Task<IEnumerable<NaoConformidade>> ObterPorStatusAsync(int empresaId, StatusNaoConformidade status);
        Task<IEnumerable<NaoConformidade>> ObterPorTipoAsync(int empresaId, TipoNaoConformidade tipo);
        Task<IEnumerable<NaoConformidade>> ObterPorSeveridadeAsync(int empresaId, SeveridadeNaoConformidade severidade);
        Task<IEnumerable<NaoConformidade>> ObterPorResponsavelAsync(int responsavelId, int empresaId);
        Task<IEnumerable<NaoConformidade>> ObterVencidasAsync(int empresaId);
        Task<IEnumerable<NaoConformidade>> ObterPorPeriodoAsync(int empresaId, DateTime dataInicio, DateTime dataFim);
        
        // Relatórios
        Task<IEnumerable<(string Tipo, int Quantidade)>> ObterEstatisticasPorTipoAsync(int empresaId, 
            DateTime dataInicio, DateTime dataFim);
        Task<IEnumerable<(string Severidade, int Quantidade)>> ObterEstatisticasPorSeveridadeAsync(int empresaId, 
            DateTime dataInicio, DateTime dataFim);
        Task<decimal> CalcularTempoMedioResolucaoAsync(int empresaId, DateTime dataInicio, DateTime dataFim);
        
        // Dashboard
        Task<int> ObterQuantidadeAbertasAsync(int empresaId);
        Task<int> ObterQuantidadeVencidasAsync(int empresaId);
        Task<int> ObterQuantidadeCriticasAsync(int empresaId);
        Task<IEnumerable<(string Responsavel, int QuantidadeAberta)>> 
            ObterNaoConformidadesPorResponsavelAsync(int empresaId);
    }
}