using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities.Fiscal
{
    /// <summary>
    /// Configuração Fiscal da Operação - Evita CFOP hardcoded
    /// Preparado para evoluir sem quebrar
    /// </summary>
    public class OperacaoFiscalConfig
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Tipo de operação fiscal
        /// </summary>
        [Required]
        public TipoOperacaoFiscal TipoOperacao { get; set; }
        
        /// <summary>
        /// CFOP padrão para esta operação
        /// </summary>
        [Required]
        [StringLength(4)]
        public string CFOPPadrao { get; set; } = string.Empty;
        
        /// <summary>
        /// Descrição da operação
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Descricao { get; set; } = string.Empty;
        
        /// <summary>
        /// Indica se a operação gera movimento de estoque
        /// </summary>
        public bool MovimentaEstoque { get; set; } = true;
        
        /// <summary>
        /// Indica se a operação é de entrada ou saída
        /// </summary>
        [Required]
        public DirecaoOperacao Direcao { get; set; }
        
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
    }
    
    /// <summary>
    /// Tipos de operação fiscal suportados
    /// </summary>
    public enum TipoOperacaoFiscal
    {
        Venda = 1,
        Compra = 2,
        Transferencia = 3,
        Devolucao = 4,
        Remessa = 5,
        Retorno = 6
    }
    
    /// <summary>
    /// Direção da operação fiscal
    /// </summary>
    public enum DirecaoOperacao
    {
        Entrada = 1,
        Saida = 2
    }
}