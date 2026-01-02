using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities;

/// <summary>
/// Entidade que representa uma compra/pedido de compra
/// </summary>
public class Compra : BaseEntity
{
    public int EmpresaId { get; set; }
    public virtual Empresa? Empresa { get; set; }

    [StringLength(20)]
    public string NumeroCompra { get; set; } = string.Empty;

    public int FornecedorId { get; set; }
    public virtual Fornecedor Fornecedor { get; set; } = null!;

    [StringLength(50)]
    public string? NumeroNF { get; set; } // Número da nota fiscal do fornecedor

    public DateTime DataCompra { get; set; } = DateTime.Now;
    public DateTime? DataEntrega { get; set; } // Data prevista de entrega
    public DateTime? DataRecebimento { get; set; } // Data de recebimento real

    public StatusCompra Status { get; set; } = StatusCompra.Pedido;

    // Valores
    public decimal SubTotal { get; set; }
    public decimal PercentualDesconto { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal ValorTotal { get; set; }

    // Pagamento
    public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.Boleto;
    public int Parcelas { get; set; } = 1;

    [StringLength(500)]
    public string? Observacoes { get; set; }

    // Relacionamentos
    public virtual ICollection<CompraItem> Itens { get; set; } = new List<CompraItem>();
    public virtual ICollection<ContaPagar> ContasPagar { get; set; } = new List<ContaPagar>();
}

/// <summary>
/// Item de uma compra
/// </summary>
public class CompraItem : BaseEntity
{
    public int CompraId { get; set; }
    public virtual Compra Compra { get; set; } = null!;

    public int ProdutoId { get; set; }
    public virtual Produto Produto { get; set; } = null!;

    public decimal Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }

    [StringLength(200)]
    public string? Observacoes { get; set; }
}

public enum StatusCompra
{
    Pedido = 1,
    Aprovada = 2,
    Recebida = 3,
    Cancelada = 4
}
