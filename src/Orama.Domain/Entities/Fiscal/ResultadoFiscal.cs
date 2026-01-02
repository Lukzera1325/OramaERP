namespace Orama.Domain.Entities.Fiscal
{
    /// <summary>
    /// Resultado do cálculo fiscal - Estrutura para receber dados do motor fiscal
    /// </summary>
    public class ResultadoFiscal
    {
        /// <summary>
        /// Contexto que originou este resultado
        /// </summary>
        public ContextoFiscal Contexto { get; set; } = null!;
        
        /// <summary>
        /// CFOP calculado para a operação
        /// </summary>
        public string CFOP { get; set; } = string.Empty;
        
        /// <summary>
        /// CST/CSOSN calculado
        /// </summary>
        public string CSTouCSOSN { get; set; } = string.Empty;
        
        /// <summary>
        /// Alíquota ICMS aplicada
        /// </summary>
        public decimal AliquotaICMS { get; set; }
        
        /// <summary>
        /// Valor do ICMS calculado
        /// </summary>
        public decimal ValorICMS { get; set; }
        
        /// <summary>
        /// Base de cálculo do ICMS
        /// </summary>
        public decimal BaseCalculoICMS { get; set; }
        
        /// <summary>
        /// Observações fiscais
        /// </summary>
        public string? Observacoes { get; set; }
        
        /// <summary>
        /// Indica se o cálculo foi bem-sucedido
        /// </summary>
        public bool Sucesso { get; set; }
        
        /// <summary>
        /// Mensagens de erro ou aviso
        /// </summary>
        public List<string> Mensagens { get; set; } = new();
        
        /// <summary>
        /// Data/hora do cálculo
        /// </summary>
        public DateTime CalculadoEm { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Versão do motor fiscal que fez o cálculo
        /// </summary>
        public string VersaoMotor { get; set; } = string.Empty;
        
        /// <summary>
        /// Adiciona uma mensagem de erro
        /// </summary>
        public void AdicionarErro(string mensagem)
        {
            Sucesso = false;
            Mensagens.Add($"ERRO: {mensagem}");
        }
        
        /// <summary>
        /// Adiciona uma mensagem de aviso
        /// </summary>
        public void AdicionarAviso(string mensagem)
        {
            Mensagens.Add($"AVISO: {mensagem}");
        }
        
        /// <summary>
        /// Cria um resultado de erro
        /// </summary>
        public static ResultadoFiscal CriarErro(ContextoFiscal contexto, string mensagem)
        {
            var resultado = new ResultadoFiscal
            {
                Contexto = contexto,
                Sucesso = false,
                VersaoMotor = "Base-1.0"
            };
            resultado.AdicionarErro(mensagem);
            return resultado;
        }
        
        /// <summary>
        /// Cria um resultado básico de sucesso (para testes)
        /// </summary>
        public static ResultadoFiscal CriarSucesso(ContextoFiscal contexto)
        {
            return new ResultadoFiscal
            {
                Contexto = contexto,
                CFOP = contexto.OperacaoFiscalConfig.CFOPPadrao,
                CSTouCSOSN = contexto.ObterCSTOuCSOSN() ?? "000",
                AliquotaICMS = contexto.ProdutoFiscalConfig.AliquotaICMSPadrao ?? 0,
                BaseCalculoICMS = contexto.ValorOperacao,
                ValorICMS = (contexto.ValorOperacao * (contexto.ProdutoFiscalConfig.AliquotaICMSPadrao ?? 0)) / 100,
                Sucesso = true,
                VersaoMotor = "Base-1.0"
            };
        }
    }
}