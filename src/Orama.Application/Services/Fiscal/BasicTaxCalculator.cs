using Orama.Domain.Entities.Fiscal;
using Orama.Domain.Interfaces;

namespace Orama.Application.Services.Fiscal
{
    /// <summary>
    /// Implementação básica do motor fiscal - Apenas para demonstração
    /// Implementações reais e complexas vêm depois
    /// </summary>
    public class BasicTaxCalculator : ITaxCalculator
    {
        public string Versao => "Basic-1.0";
        
        public async Task<ResultadoFiscal> CalcularAsync(ContextoFiscal contexto)
        {
            // Validar contexto primeiro
            var erros = await ValidarContextoAsync(contexto);
            if (erros.Any())
            {
                return ResultadoFiscal.CriarErro(contexto, string.Join(", ", erros));
            }
            
            try
            {
                // Cálculo básico - apenas demonstrativo
                var resultado = new ResultadoFiscal
                {
                    Contexto = contexto,
                    CFOP = contexto.OperacaoFiscalConfig.CFOPPadrao,
                    CSTouCSOSN = contexto.ObterCSTOuCSOSN() ?? "000",
                    AliquotaICMS = contexto.ProdutoFiscalConfig.AliquotaICMSPadrao ?? 0,
                    BaseCalculoICMS = contexto.ValorOperacao,
                    Sucesso = true,
                    VersaoMotor = Versao
                };
                
                // Cálculo do ICMS (básico)
                resultado.ValorICMS = (resultado.BaseCalculoICMS * resultado.AliquotaICMS) / 100;
                
                // Adicionar observações baseadas no regime
                if (contexto.RegimeTributario == RegimeTributario.SimplesNacional)
                {
                    resultado.AdicionarAviso("Cálculo básico para Simples Nacional");
                    
                    // No Simples Nacional, muitas vezes o ICMS é 0
                    if (resultado.CSTouCSOSN == "102" || resultado.CSTouCSOSN == "103")
                    {
                        resultado.ValorICMS = 0;
                        resultado.AliquotaICMS = 0;
                        resultado.Observacoes = "ICMS isento pelo Simples Nacional";
                    }
                }
                else
                {
                    resultado.AdicionarAviso("Cálculo básico para Regime Normal");
                }
                
                // Verificar operação interestadual
                if (contexto.IsOperacaoInterestadual)
                {
                    resultado.AdicionarAviso($"Operação interestadual: {contexto.UFOrigem} → {contexto.UFDestino}");
                    // Aqui viriam regras específicas de ICMS interestadual
                }
                
                return resultado;
            }
            catch (Exception ex)
            {
                return ResultadoFiscal.CriarErro(contexto, $"Erro no cálculo fiscal: {ex.Message}");
            }
        }
        
        public async Task<List<string>> ValidarContextoAsync(ContextoFiscal contexto)
        {
            var erros = new List<string>();
            
            if (contexto == null)
            {
                erros.Add("Contexto fiscal é obrigatório");
                return erros;
            }
            
            if (!contexto.IsValido())
            {
                erros.Add("Contexto fiscal inválido");
            }
            
            if (contexto.EmpresaFiscalConfig == null)
                erros.Add("Configuração da empresa é obrigatória");
            
            if (contexto.ProdutoFiscalConfig == null)
                erros.Add("Configuração do produto é obrigatória");
            
            if (contexto.OperacaoFiscalConfig == null)
                erros.Add("Configuração da operação é obrigatória");
            
            if (contexto.ValorOperacao <= 0)
                erros.Add("Valor da operação deve ser maior que zero");
            
            // Validações específicas do motor básico
            if (contexto.EmpresaFiscalConfig != null)
            {
                if (!SuportaRegime(contexto.RegimeTributario))
                    erros.Add($"Regime tributário {contexto.RegimeTributario} não suportado");
                
                if (!SuportaUF(contexto.UFOrigem))
                    erros.Add($"UF {contexto.UFOrigem} não suportada");
            }
            
            return erros;
        }
        
        public bool SuportaRegime(RegimeTributario regime)
        {
            // Motor básico suporta ambos os regimes
            return regime == RegimeTributario.SimplesNacional || 
                   regime == RegimeTributario.RegimeNormal;
        }
        
        public bool SuportaUF(string uf)
        {
            // Motor básico suporta todas as UFs (simplificado)
            var ufsValidas = new[]
            {
                "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA",
                "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN",
                "RS", "RO", "RR", "SC", "SP", "SE", "TO"
            };
            
            return ufsValidas.Contains(uf?.ToUpper());
        }
    }
}