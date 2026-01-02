using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities
{
    public class ListaMateriais
    {
        public int Id { get; set; }
        
        public int EmpresaId { get; set; }
        public virtual Empresa Empresa { get; set; } = null!;
        
        public int ProdutoId { get; set; }
        public virtual Produto Produto { get; set; } = null!;
        
        [Required]
        [StringLength(20)]
        public string Versao { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Descricao { get; set; }
        
        public bool Ativo { get; set; } = true;
        
        public DateTime DataVigencia { get; set; }
        public DateTime? DataVencimento { get; set; }
        
        public decimal QuantidadeBase { get; set; } = 1;
        
        [StringLength(10)]
        public string UnidadeMedida { get; set; } = "UN";
        
        public decimal CustoTotal { get; set; }
        
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        
        public int UsuarioCriacaoId { get; set; }
        public virtual Usuario UsuarioCriacao { get; set; } = null!;
        
        // Relacionamentos
        public virtual ICollection<ListaMateriaisItem> Itens { get; set; } = new List<ListaMateriaisItem>();
    }
}