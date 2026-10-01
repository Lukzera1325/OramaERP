using Orama.Domain.Entities.Fiscal;

namespace Orama.Application.Services.Fiscal
{
    /// <summary>
    /// Interface para gerenciamento de configurações fiscais da empresa
    /// </summary>
    public interface IEmpresaFiscalService
    {
        /// <summary>
        /// Obtém a configuração fiscal vigente da empresa
        /// </summary>
        Task<EmpresaFiscalConfig?> ObterConfiguracaoVigenteAsync(int empresaId, DateTime data);
        
        /// <summary>
        /// Cria uma nova configuração fiscal para a empresa
        /// </summary>
        Task<EmpresaFiscalConfig> CriarConfiguracaoAsync(EmpresaFiscalConfig configuracao);
        
        /// <summary>
        /// Atualiza uma configuração fiscal existente
        /// </summary>
        Task<EmpresaFiscalConfig> AtualizarConfiguracaoAsync(EmpresaFiscalConfig configuracao);
        
        /// <summary>
        /// Encerra a vigência de uma configuração fiscal
        /// </summary>
        Task EncerrarVigenciaAsync(int configuracaoId, int empresaId, DateTime dataEncerramento);
        
        /// <summary>
        /// Lista todas as configurações fiscais da empresa
        /// </summary>
        Task<List<EmpresaFiscalConfig>> ListarConfiguracoesAsync(int empresaId);
        
        /// <summary>
        /// Valida se uma configuração fiscal está correta
        /// </summary>
        Task<List<string>> ValidarConfiguracaoAsync(EmpresaFiscalConfig configuracao);
    }
}
