using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities
{
    public class InspecaoQualidade
    {
        public int Id { get; set; }
        
        public int OrdemProducaoId { get; set; }
        public virtual OrdemProducao OrdemProducao { get; set; } = null!;
        
        public int? EtapaId { get; set; }
        public virtual OrdemProducaoEtapa? Etapa { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Titulo { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Descricao { get; set; }
        
        public TipoInspecao Tipo { get; set; }
        
        public decimal QuantidadeInspecionada { get; set; }
        public decimal QuantidadeAprovada { get; set; }
        public decimal QuantidadeRejeitada { get; set; }
        
        [Required]
        public ResultadoInspecao Resultado { get; set; }
        
        public DateTime DataInspecao { get; set; }
        
        public int InspetorId { get; set; }
        public virtual Usuario Inspetor { get; set; } = null!;
        
        [StringLength(1000)]
        public string? Observacoes { get; set; }
        
        [StringLength(500)]
        public string? AcaoCorretiva { get; set; }
        
        public DateTime DataCriacao { get; set; }
        
        // Relacionamentos
        public virtual ICollection<NaoConformidade> NaoConformidades { get; set; } = new List<NaoConformidade>();
    }
    
    public enum TipoInspecao
    {
        Entrada = 1,
        Processo = 2,
        Final = 3,
        Auditoria = 4
    }
    
    public enum ResultadoInspecao
    {
        Aprovado = 1,
        Rejeitado = 2,
        CondicionalmenteAprovado = 3,
        EmAnalise = 4
    }
}