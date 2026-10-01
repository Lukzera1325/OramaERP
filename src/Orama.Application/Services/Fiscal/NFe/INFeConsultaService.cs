using Orama.Domain.Entities.Fiscal.NFe;

namespace Orama.Application.Services.Fiscal.NFe
{
    /// <summary>
    /// Interface para consulta de NF-e
    /// </summary>
    public interface INFeConsultaService
    {
        /// <summary>
        /// Consulta situação da NF-e na SEFAZ
        /// </summary>
        Task<NFeDocumento> ConsultarSituacaoAsync(int nfeId, int empresaId, int usuarioId);
        
        /// <summary>
        /// Consulta situação por chave de acesso
        /// </summary>
        Task<ResultadoConsultaNFe> ConsultarPorChaveAsync(string chaveAcesso, int empresaId);
        
        /// <summary>
        /// Verifica status do serviço SEFAZ
        /// </summary>
        Task<bool> VerificarStatusServicoAsync(int empresaId);
        
        /// <summary>
        /// Atualiza status de todas as NF-es pendentes
        /// </summary>
        Task<int> AtualizarStatusPendentesAsync(int empresaId);
    }
}
