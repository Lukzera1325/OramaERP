namespace Orama.Domain.Entities.Fiscal
{
    /// <summary>
    /// Contexto Fiscal - Objeto chave que será a entrada do motor fiscal
    /// Montado no momento da operação, contém todas as informações necessárias
    /// </summary>
    public class ContextoFiscal
    {
        /// <summary>
        /// Configuração fiscal da empresa
        /// </summary>
        public EmpresaFiscalConfig EmpresaFiscalConfig { get; set; } = null!;
        
        /// <summary>
        /// Configuração fiscal do produto
        /// </summary>
        public ProdutoFiscalConfig ProdutoFiscalConfig { get; set; } = null!;
        
        /// <summary>
        /// Configuração fiscal da operação
        /// </summary>
        public OperacaoFiscalConfig OperacaoFiscalConfig { get; set; } = null!;
        
        /// <summary>
        /// Data da operação (NUNCA usar DateTime.Now)
        /// </summary>
        public DateTime DataOperacao { get; set; }
        
        /// <summary>
        /// Valor da operação para cálculos
        /// </summary>
        public decimal ValorOperacao { get; set; }
        
        /// <summary>
        /// Quantidade para cálculos
        /// </summary>
        public decimal Quantidade { get; set; }
        
        /// <summary>
        /// UF de destino (para operações interestaduais)
        /// </summary>
        public string? UFDestino { get; set; }
        
        /// <summary>
        /// Propriedades derivadas para facilitar o uso
        /// </summary>
        public AmbienteFiscal AmbienteFiscal => EmpresaFiscalConfig.AmbienteFiscal;
        public RegimeTributario RegimeTributario => EmpresaFiscalConfig.RegimeTributario;
        public string VersaoFiscal => EmpresaFiscalConfig.VersaoFiscal;
        public string UFOrigem => EmpresaFiscalConfig.UF;
        
        /// <summary>
        /// Verifica se a operação é interestadual
        /// </summary>
        public bool IsOperacaoInterestadual => 
            !string.IsNullOrEmpty(UFDestino) && UFDestino != UFOrigem;
        
        /// <summary>
        /// Obtém o CST/CSOSN apropriado baseado no regime
        /// </summary>
        public string? ObterCSTOuCSOSN() => 
            ProdutoFiscalConfig.ObterCSTOuCSOSN(RegimeTributario);
        
        /// <summary>
        /// Validação básica do contexto
        /// </summary>
        public bool IsValido()
        {
            return EmpresaFiscalConfig != null &&
                   ProdutoFiscalConfig != null &&
                   OperacaoFiscalConfig != null &&
                   DataOperacao != default &&
                   ValorOperacao > 0 &&
                   Quantidade > 0 &&
                   EmpresaFiscalConfig.EstaVigente(DataOperacao) &&
                   ProdutoFiscalConfig.EstaVigente(DataOperacao) &&
                   OperacaoFiscalConfig.EstaVigente(DataOperacao);
        }
        
        /// <summary>
        /// Cria um contexto fiscal básico para testes
        /// </summary>
        public static ContextoFiscal CriarParaTeste(
            DateTime dataOperacao,
            decimal valorOperacao,
            decimal quantidade = 1,
            RegimeTributario regime = RegimeTributario.SimplesNacional)
        {
            return new ContextoFiscal
            {
                DataOperacao = dataOperacao,
                ValorOperacao = valorOperacao,
                Quantidade = quantidade,
                EmpresaFiscalConfig = new EmpresaFiscalConfig
                {
                    RegimeTributario = regime,
                    CRT = regime == RegimeTributario.SimplesNacional ? CRT.SimplesNacional : CRT.LucroPresumido,
                    UF = "SP",
                    AmbienteFiscal = AmbienteFiscal.Homologacao,
                    VersaoFiscal = "2025.1",
                    VigenteDe = dataOperacao.AddDays(-30),
                    VigenteAte = null
                },
                ProdutoFiscalConfig = new ProdutoFiscalConfig
                {
                    NCM = "12345678",
                    Origem = OrigemMercadoria.Nacional,
                    CST = "00",
                    CSOSN = "102",
                    AliquotaICMSPadrao = 18.00m,
                    VigenteDe = dataOperacao.AddDays(-30),
                    VigenteAte = null
                },
                OperacaoFiscalConfig = new OperacaoFiscalConfig
                {
                    TipoOperacao = TipoOperacaoFiscal.Venda,
                    CFOPPadrao = "5102",
                    Descricao = "Venda de mercadoria adquirida ou recebida de terceiros",
                    Direcao = DirecaoOperacao.Saida,
                    VigenteDe = dataOperacao.AddDays(-30),
                    VigenteAte = null
                }
            };
        }
    }
}