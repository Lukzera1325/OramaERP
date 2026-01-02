using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities.Fiscal
{
    /// <summary>
    /// Configuração Fiscal do Produto - Apenas dados declarativos
    /// Isolada do core, sem lógica fiscal
    /// </summary>
    public class ProdutoFiscalConfig
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Referência ao produto do core (sem contaminar)
        /// </summary>
        public int ProdutoId { get; set; }
        
        /// <summary>
        /// Nomenclatura Comum do Mercosul
        /// </summary>
        [Required]
        [StringLength(8)]
        public string NCM { get; set; } = string.Empty;
        
        /// <summary>
        /// Origem da mercadoria (0-8)
        /// </summary>
        [Required]
        public OrigemMercadoria Origem { get; set; }
        
        /// <summary>
        /// CST para Regime Normal
        /// </summary>
        public string? CST { get; set; }
        
        /// <summary>
        /// CSOSN para Simples Nacional
        /// </summary>
        public string? CSOSN { get; set; }
        
        /// <summary>
        /// Alíquota ICMS padrão (apenas declarativa)
        /// </summary>
        public decimal? AliquotaICMSPadrao { get; set; }
        
        /// <summary>
        /// Controle de vigência - início
        /// </summary>
        [Required]
        public DateTime VigenteDe { get; set; }
        
        /// <summary>
        /// Controle de vigência - fim (null = vigente indefinidamente)
        /// </summary>
        public DateTime? VigenteAte { get; set; }
        
        /// <summary>
        /// Auditoria
        /// </summary>
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public int CriadoPor { get; set; }
        
        /// <summary>
        /// Verifica se a configuração está vigente em uma data específica
        /// </summary>
        public bool EstaVigente(DateTime data)
        {
            return data >= VigenteDe && (VigenteAte == null || data <= VigenteAte);
        }
        
        /// <summary>
        /// Obtém o CST/CSOSN apropriado baseado no regime tributário
        /// </summary>
        public string? ObterCSTOuCSOSN(RegimeTributario regime)
        {
            return regime == RegimeTributario.SimplesNacional ? CSOSN : CST;
        }
    }
    
    /// <summary>
    /// Origem da mercadoria conforme tabela SEFAZ
    /// </summary>
    public enum OrigemMercadoria
    {
        /// <summary>
        /// Nacional, exceto as indicadas nos códigos 3, 4, 5 e 8
        /// </summary>
        Nacional = 0,
        
        /// <summary>
        /// Estrangeira - Importação direta, exceto a indicada no código 6
        /// </summary>
        EstrangeiraImportacaoDireta = 1,
        
        /// <summary>
        /// Estrangeira - Adquirida no mercado interno, exceto a indicada no código 7
        /// </summary>
        EstrangeiraMercadoInterno = 2,
        
        /// <summary>
        /// Nacional, mercadoria ou bem com Conteúdo de Importação superior a 40% e inferior ou igual a 70%
        /// </summary>
        NacionalConteudoImportacao40a70 = 3,
        
        /// <summary>
        /// Nacional, cuja produção tenha sido feita em conformidade com os processos produtivos básicos
        /// </summary>
        NacionalProcessosBasicos = 4,
        
        /// <summary>
        /// Nacional, mercadoria ou bem com Conteúdo de Importação inferior ou igual a 40%
        /// </summary>
        NacionalConteudoImportacaoAte40 = 5,
        
        /// <summary>
        /// Estrangeira - Importação direta, sem similar nacional, constante em lista da CAMEX
        /// </summary>
        EstrangeiraImportacaoSemSimilar = 6,
        
        /// <summary>
        /// Estrangeira - Adquirida no mercado interno, sem similar nacional, constante lista CAMEX
        /// </summary>
        EstrangeiraMercadoInternoSemSimilar = 7,
        
        /// <summary>
        /// Nacional, mercadoria ou bem com Conteúdo de Importação superior a 70%
        /// </summary>
        NacionalConteudoImportacaoAcima70 = 8
    }
}