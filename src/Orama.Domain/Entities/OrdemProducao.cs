using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities
{
    public class OrdemProducao
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Numero { get; set; } = string.Empty;
        
        public int EmpresaId { get; set; }
        public virtual Empresa Empresa { get; set; } = null!;
        
        public int ProdutoId { get; set; }
        public virtual Produto Produto { get; set; } = null!;
        
        [Required]
        public decimal QuantidadePlanejada { get; set; }
        
        public decimal QuantidadeProduzida { get; set; }
        
        public DateTime DataPlanejada { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        
        [Required]
        public StatusOrdemProducao Status { get; set; }
        
        public PrioridadeOrdemProducao Prioridade { get; set; }
        
        [StringLength(500)]
        public string? Observacoes { get; set; }
        
        public decimal CustoMaterial { get; set; }
        public decimal CustoMaoObra { get; set; }
        public decimal CustoTotal => CustoMaterial + CustoMaoObra;
        
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        
        public int UsuarioCriacaoId { get; set; }
        public virtual Usuario UsuarioCriacao { get; set; } = null!;
        
        // Relacionamentos
        public virtual ICollection<OrdemProducaoItem> Itens { get; set; } = new List<OrdemProducaoItem>();
        public virtual ICollection<OrdemProducaoEtapa> Etapas { get; set; } = new List<OrdemProducaoEtapa>();
        public virtual ICollection<ApontamentoHoras> ApontamentosHoras { get; set; } = new List<ApontamentoHoras>();
        public virtual ICollection<InspecaoQualidade> InspecoesQualidade { get; set; } = new List<InspecaoQualidade>();
    }
    
    public enum StatusOrdemProducao
    {
        Planejada = 1,
        Liberada = 2,
        EmAndamento = 3,
        Pausada = 4,
        Finalizada = 5,
        Cancelada = 6
    }
    
    public enum PrioridadeOrdemProducao
    {
        Baixa = 1,
        Normal = 2,
        Alta = 3,
        Urgente = 4
    }
}