using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities;

/// <summary>
/// Entidade que representa uma venda/pedido de venda
/// </summary>
public class Venda : BaseEntity
{
    public int EmpresaId { get; set; }
    public virtual Empresa? Empresa { get; set; }

    [StringLength(20)]
    public string Numero { get; set; } = string.Empty;
    
    // Propriedade para compatibilidade
    public string NumeroVenda => Numero;

    public int ClienteId { get; set; }
    public virtual Cliente Cliente { get; set; } = null!;

    public int? VendedorId { get; set; }
    public virtual Usuario? Vendedor { get; set; }

    public DateTime DataVenda { get; set; } = DateTime.Now;
    public DateTime? DataEntrega { get; set; }

    public StatusVenda Status { get; set; } = StatusVenda.Orcamento;

    // Valores
    public decimal SubTotal { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal PercentualDesconto { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal ValorTotal { get; set; }

    // Pagamento
    public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.Dinheiro;
    public int Parcelas { get; set; } = 1;

    [StringLength(500)]
    public string? Observacoes { get; set; }

    // Relacionamentos
    public virtual ICollection<VendaItem> Itens { get; set; } = new List<VendaItem>();
    public virtual ICollection<ContaReceber> ContasReceber { get; set; } = new List<ContaReceber>();
}

/// <summary>
/// Item de uma venda
/// </summary>
public class VendaItem : BaseEntity
{
    public int VendaId { get; set; }
    public virtual Venda Venda { get; set; } = null!;

    public int ProdutoId { get; set; }
    public virtual Produto Produto { get; set; } = null!;

    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    
    // Propriedade para compatibilidade
    public decimal ValorUnitario => PrecoUnitario;
    public decimal PercentualDesconto { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }

    [StringLength(200)]
    public string? Observacoes { get; set; }
}

public enum StatusVenda
{
    Orcamento = 1,
    Aprovado = 2,
    Faturada = 3,
    Entregue = 4,
    Cancelado = 5
}

public enum FormaPagamento
{
    Dinheiro = 1,
    CartaoCredito = 2,
    CartaoDebito = 3,
    Pix = 4,
    Boleto = 5,
    Transferencia = 6,
    Cheque = 7
}
