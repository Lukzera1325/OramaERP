using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities;

/// <summary>
/// Estrutura de Produto (BOM - Bill of Materials)
/// Define quais componentes são necessários para produzir um produto acabado
/// </summary>
public class EstruturaProduto : BaseEntity
{
    // Multi-Tenant
    public int EmpresaId { get; set; }
    public virtual Empresa? Empresa { get; set; }

    // Produto Pai (o que será produzido)
    [Display(Name = "Produto Acabado")]
    public int ProdutoPaiId { get; set; }
    public virtual Produto ProdutoPai { get; set; } = null!;

    // Produto Componente (o que é necessário)
    [Display(Name = "Componente")]
    public int ProdutoComponenteId { get; set; }
    public virtual Produto ProdutoComponente { get; set; } = null!;

    // Quantidade necessária do componente
    [Display(Name = "Quantidade Necessária")]
    [Range(0.001, double.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
    public decimal QuantidadeNecessaria { get; set; }

    [Display(Name = "Unidade")]
    [StringLength(10)]
    public string? Unidade { get; set; }

    [Display(Name = "Observações")]
    [StringLength(500)]
    public string? Observacoes { get; set; }

    // Métodos de negócio
    public decimal CalcularQuantidadeTotal(decimal quantidadeProduzir)
    {
        return QuantidadeNecessaria * quantidadeProduzir;
    }

    public bool ComponenteTemEstoqueSuficiente(decimal quantidadeProduzir)
    {
        if (!ProdutoComponente.ControlaEstoque) return true;
        
        var quantidadeNecessaria = CalcularQuantidadeTotal(quantidadeProduzir);
        return ProdutoComponente.PodeVender(quantidadeNecessaria);
    }
}