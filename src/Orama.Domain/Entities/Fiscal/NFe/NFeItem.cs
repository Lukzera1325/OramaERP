using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities.Fiscal.NFe
{
    /// <summary>
    /// Item da NF-e - Isolado do core
    /// </summary>
    public class NFeItem
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Referência ao documento NF-e
        /// </summary>
        public int NFeDocumentoId { get; set; }
        public virtual NFeDocumento NFeDocumento { get; set; } = null!;
        
        /// <summary>
        /// Referência ao produto (SEM navegação - isolamento)
        /// </summary>
        public int ProdutoId { get; set; }
        
        /// <summary>
        /// Número sequencial do item na NF-e
        /// </summary>
        [Required]
        public int NumeroItem { get; set; }
        
        /// <summary>
        /// Código do produto (EAN/GTIN ou código interno)
        /// </summary>
        [StringLength(50)]
        public string? CodigoProduto { get; set; }
        
        /// <summary>
        /// Descrição do produto
        /// </summary>
        [Required]
        [StringLength(120)]
        public string Descricao { get; set; } = string.Empty;
        
        /// <summary>
        /// NCM do produto
        /// </summary>
        [Required]
        [StringLength(8)]
        public string NCM { get; set; } = string.Empty;
        
        /// <summary>
        /// CFOP da operação
        /// </summary>
        [Required]
        [StringLength(4)]
        public string CFOP { get; set; } = string.Empty;
        
        /// <summary>
        /// Unidade comercial
        /// </summary>
        [Required]
        [StringLength(6)]
        public string Unidade { get; set; } = string.Empty;
        
        /// <summary>
        /// Quantidade comercial
        /// </summary>
        [Required]
        public decimal Quantidade { get; set; }
        
        /// <summary>
        /// Valor unitário comercial
        /// </summary>
        [Required]
        public decimal ValorUnitario { get; set; }
        
        /// <summary>
        /// Valor total do item (quantidade * valor unitário)
        /// </summary>
        [Required]
        public decimal ValorTotal { get; set; }
        
        /// <summary>
        /// Origem da mercadoria (0-8)
        /// </summary>
        [Required]
        public int Origem { get; set; }
        
        /// <summary>
        /// CST (Regime Normal) ou CSOSN (Simples Nacional)
        /// </summary>
        [Required]
        [StringLength(3)]
        public string CstCsosn { get; set; } = string.Empty;
        
        /// <summary>
        /// Alíquota do ICMS
        /// </summary>
        public decimal? AliquotaICMS { get; set; }
        
        /// <summary>
        /// Base de cálculo do ICMS
        /// </summary>
        public decimal? BaseCalculoICMS { get; set; }
        
        /// <summary>
        /// Valor do ICMS
        /// </summary>
        public decimal? ValorICMS { get; set; }
        
        /// <summary>
        /// Informações adicionais do item
        /// </summary>
        [StringLength(500)]
        public string? InformacoesAdicionais { get; set; }
        
        /// <summary>
        /// Calcula o valor total baseado na quantidade e valor unitário
        /// </summary>
        public void CalcularValorTotal()
        {
            ValorTotal = Quantidade * ValorUnitario;
        }
        
        /// <summary>
        /// Calcula o ICMS baseado na base de cálculo e alíquota
        /// </summary>
        public void CalcularICMS()
        {
            if (BaseCalculoICMS.HasValue && AliquotaICMS.HasValue)
            {
                ValorICMS = (BaseCalculoICMS.Value * AliquotaICMS.Value) / 100;
            }
        }
        
        /// <summary>
        /// Valida se o item está correto para emissão
        /// </summary>
        public List<string> Validar()
        {
            var erros = new List<string>();
            
            if (string.IsNullOrWhiteSpace(Descricao))
                erros.Add("Descrição é obrigatória");
            
            if (string.IsNullOrWhiteSpace(NCM) || NCM.Length != 8)
                erros.Add("NCM deve ter 8 dígitos");
            
            if (string.IsNullOrWhiteSpace(CFOP) || CFOP.Length != 4)
                erros.Add("CFOP deve ter 4 dígitos");
            
            if (Quantidade <= 0)
                erros.Add("Quantidade deve ser maior que zero");
            
            if (ValorUnitario <= 0)
                erros.Add("Valor unitário deve ser maior que zero");
            
            if (string.IsNullOrWhiteSpace(CstCsosn))
                erros.Add("CST/CSOSN é obrigatório");
            
            return erros;
        }
    }
}