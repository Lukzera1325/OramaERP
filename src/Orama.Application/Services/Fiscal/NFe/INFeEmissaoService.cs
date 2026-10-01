using Orama.Domain.Entities.Fiscal.NFe;

namespace Orama.Application.Services.Fiscal.NFe
{
    /// <summary>
    /// Interface para emissão de NF-e
    /// </summary>
    public interface INFeEmissaoService
    {
        /// <summary>
        /// Gera NF-e a partir de uma venda
        /// </summary>
        Task<NFeDocumento> GerarNFeAsync(int vendaId, int empresaId, int usuarioId);
        
        /// <summary>
        /// Assina e envia NF-e para SEFAZ
        /// </summary>
        Task<NFeDocumento> AssinarEEnviarAsync(int nfeId, int empresaId, int usuarioId);
        
        /// <summary>
        /// Obtém próximo número de NF-e para uma série
        /// </summary>
        Task<int> ObterProximoNumeroAsync(int empresaId, int serie = 1);
        
        /// <summary>
        /// Valida se uma venda pode gerar NF-e
        /// </summary>
        Task<List<string>> ValidarVendaParaNFeAsync(int vendaId, int empresaId);
        
        /// <summary>
        /// Lista NF-es de uma empresa
        /// </summary>
        Task<List<NFeDocumento>> ListarNFesAsync(int empresaId, DateTime? dataInicio = null, DateTime? dataFim = null);
        
        /// <summary>
        /// Obtém NF-e por ID
        /// </summary>
        Task<NFeDocumento?> ObterNFePorIdAsync(int nfeId, int empresaId);
        
        /// <summary>
        /// Obtém NF-e por venda
        /// </summary>
        Task<NFeDocumento?> ObterNFePorVendaAsync(int vendaId, int empresaId);
    }
}
