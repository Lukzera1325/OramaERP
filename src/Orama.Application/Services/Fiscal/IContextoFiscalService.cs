using Orama.Domain.Entities.Fiscal;

namespace Orama.Application.Services.Fiscal
{
    /// <summary>
    /// Interface para montagem de contextos fiscais
    /// </summary>
    public interface IContextoFiscalService
    {
        /// <summary>
        /// Monta um contexto fiscal completo para uma operação
        /// </summary>
        Task<ContextoFiscal> MontarContextoAsync(
            int empresaId,
            int produtoId,
            TipoOperacaoFiscal tipoOperacao,
            DateTime dataOperacao,
            decimal valorOperacao,
            decimal quantidade = 1,
            string? ufDestino = null);
        
        /// <summary>
        /// Valida se um contexto fiscal está completo e correto
        /// </summary>
        Task<List<string>> ValidarContextoAsync(ContextoFiscal contexto);
        
        /// <summary>
        /// Obtém configurações fiscais vigentes para debug/auditoria
        /// </summary>
        Task<(EmpresaFiscalConfig? empresa, ProdutoFiscalConfig? produto, OperacaoFiscalConfig? operacao)> 
            ObterConfiguracoesVigentesAsync(int empresaId, int produtoId, TipoOperacaoFiscal tipoOperacao, DateTime data);
    }
}