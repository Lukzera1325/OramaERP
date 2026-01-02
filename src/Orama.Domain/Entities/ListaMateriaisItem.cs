using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities
{
    public class ListaMateriaisItem
    {
        public int Id { get; set; }
        
        public int ListaMateriaisId { get; set; }
        public virtual ListaMateriais ListaMateriais { get; set; } = null!;
        
        public int ProdutoId { get; set; }
        public virtual Produto Produto { get; set; } = null!;
        
        [Required]
        public decimal Quantidade { get; set; }
        
        [StringLength(10)]
        public string UnidadeMedida { get; set; } = "UN";
        
        public decimal CustoUnitario { get; set; }
        
        public decimal CustoTotal => Quantidade * CustoUnitario;
        
        public TipoComponente Tipo { get; set; }
        
        public bool Critico { get; set; }
        
        public decimal PercentualPerda { get; set; }
        
        public decimal QuantidadeLiquida => Quantidade * (1 + PercentualPerda / 100);
        
        [StringLength(200)]
        public string? Observacoes { get; set; }
        
        public DateTime DataCriacao { get; set; }
    }
    
    public enum TipoComponente
    {
        MateriaPrima = 1,
        Componente = 2,
        Subconjunto = 3,
        Embalagem = 4,
        Insumo = 5,
        Ferramenta = 6
    }
}