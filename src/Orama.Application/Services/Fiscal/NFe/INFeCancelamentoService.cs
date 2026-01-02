using Orama.Domain.Entities.Fiscal.NFe;

namespace Orama.Application.Services.Fiscal.NFe
{
    /// <summary>
    /// Interface para cancelamento de NF-e
    /// </summary>
    public interface INFeCancelamentoService
    {
        /// <summary>
        /// Cancela uma NF-e
        /// </summary>
        Task<NFeDocumento> CancelarNFeAsync(int nfeId, string justificativa, int usuarioId);
        
        /// <summary>
        /// Verifica se uma NF-e pode ser cancelada
        /// </summary>
        Task<(bool Pode, string Motivo)> PodeCancelarAsync(int nfeId);
        
        /// <summary>
        /// Lista NF-es que podem ser canceladas
        /// </summary>
        Task<List<NFeDocumento>> ListarCancelaveisAsync(int empresaId);
    }
}