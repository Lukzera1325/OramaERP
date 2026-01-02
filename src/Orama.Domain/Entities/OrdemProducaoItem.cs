using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities
{
    public class OrdemProducaoItem
    {
        public int Id { get; set; }
        
        public int OrdemProducaoId { get; set; }
        public virtual OrdemProducao OrdemProducao { get; set; } = null!;
        
        public int ProdutoId { get; set; }
        public virtual Produto Produto { get; set; } = null!;
        
        [Required]
        public decimal QuantidadeNecessaria { get; set; }
        
        public decimal QuantidadeConsumida { get; set; }
        
        [Required]
        public decimal CustoUnitario { get; set; }
        
        public decimal CustoTotal => QuantidadeNecessaria * CustoUnitario;
        
        public TipoItemProducao Tipo { get; set; }
        
        [StringLength(200)]
        public string? Observacoes { get; set; }
        
        public DateTime DataCriacao { get; set; }
    }
    
    public enum TipoItemProducao
    {
        MateriaPrima = 1,
        Componente = 2,
        Embalagem = 3,
        Insumo = 4
    }
}