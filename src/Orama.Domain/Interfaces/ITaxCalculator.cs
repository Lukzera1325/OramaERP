using Orama.Domain.Entities.Fiscal;

namespace Orama.Domain.Interfaces
{
    /// <summary>
    /// Interface do Motor Fiscal - Contrato para cálculos fiscais
    /// Implementações reais vêm depois, agora é só o contrato
    /// </summary>
    public interface ITaxCalculator
    {
        /// <summary>
        /// Calcula impostos baseado no contexto fiscal
        /// </summary>
        /// <param name="contexto">Contexto fiscal completo</param>
        /// <returns>Resultado do cálculo fiscal</returns>
        Task<ResultadoFiscal> CalcularAsync(ContextoFiscal contexto);
        
        /// <summary>
        /// Valida se o contexto fiscal está adequado para cálculo
        /// </summary>
        /// <param name="contexto">Contexto a ser validado</param>
        /// <returns>Lista de erros de validação</returns>
        Task<List<string>> ValidarContextoAsync(ContextoFiscal contexto);
        
        /// <summary>
        /// Versão do motor fiscal
        /// </summary>
        string Versao { get; }
        
        /// <summary>
        /// Indica se o motor suporta o regime tributário
        /// </summary>
        bool SuportaRegime(RegimeTributario regime);
        
        /// <summary>
        /// Indica se o motor suporta a UF
        /// </summary>
        bool SuportaUF(string uf);
    }
}