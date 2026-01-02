using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities
{
    public class NaoConformidade
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Numero { get; set; } = string.Empty;
        
        public int EmpresaId { get; set; }
        public virtual Empresa Empresa { get; set; } = null!;
        
        public int? InspecaoQualidadeId { get; set; }
        public virtual InspecaoQualidade? InspecaoQualidade { get; set; }
        
        public int? OrdemProducaoId { get; set; }
        public virtual OrdemProducao? OrdemProducao { get; set; }
        
        public int? ProdutoId { get; set; }
        public virtual Produto? Produto { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Descricao { get; set; } = string.Empty;
        
        public TipoNaoConformidade Tipo { get; set; }
        
        public SeveridadeNaoConformidade Severidade { get; set; }
        
        [Required]
        public StatusNaoConformidade Status { get; set; }
        
        public DateTime DataDeteccao { get; set; }
        
        public int DetectadoPorId { get; set; }
        public virtual Usuario DetectadoPor { get; set; } = null!;
        
        public int? ResponsavelId { get; set; }
        public virtual Usuario? Responsavel { get; set; }
        
        [StringLength(1000)]
        public string? CausaRaiz { get; set; }
        
        [StringLength(1000)]
        public string? AcaoCorretiva { get; set; }
        
        [StringLength(1000)]
        public string? AcaoPreventiva { get; set; }
        
        public DateTime? DataPrazo { get; set; }
        public DateTime? DataResolucao { get; set; }
        
        [StringLength(500)]
        public string? ObservacoesResolucao { get; set; }
        
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
    
    public enum TipoNaoConformidade
    {
        Produto = 1,
        Processo = 2,
        Sistema = 3,
        Documentacao = 4,
        Equipamento = 5
    }
    
    public enum SeveridadeNaoConformidade
    {
        Baixa = 1,
        Media = 2,
        Alta = 3,
        Critica = 4
    }
    
    public enum StatusNaoConformidade
    {
        Aberta = 1,
        EmAnalise = 2,
        EmAndamento = 3,
        Resolvida = 4,
        Fechada = 5,
        Cancelada = 6
    }
}