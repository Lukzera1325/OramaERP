using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities
{
    public class OrdemProducaoEtapa
    {
        public int Id { get; set; }
        
        public int OrdemProducaoId { get; set; }
        public virtual OrdemProducao OrdemProducao { get; set; } = null!;
        
        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Descricao { get; set; }
        
        public int Sequencia { get; set; }
        
        public decimal TempoEstimado { get; set; } // em horas
        public decimal TempoRealizado { get; set; }
        
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        
        [Required]
        public StatusEtapaProducao Status { get; set; }
        
        public int? ResponsavelId { get; set; }
        public virtual Usuario? Responsavel { get; set; }
        
        [StringLength(500)]
        public string? Observacoes { get; set; }
        
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        
        // Relacionamentos
        public virtual ICollection<ApontamentoHoras> ApontamentosHoras { get; set; } = new List<ApontamentoHoras>();
    }
    
    public enum StatusEtapaProducao
    {
        Pendente = 1,
        EmAndamento = 2,
        Pausada = 3,
        Concluida = 4,
        Cancelada = 5
    }
}