using System.ComponentModel.DataAnnotations;

namespace OramaGo.Models;

public class VendaLocal : BaseLocalModel
{
    [Required]
    [StringLength(20)]
    public string Numero { get; set; } = string.Empty;

    [Required]
    public int ClienteId { get; set; }

    public int? VendedorId { get; set; }

    [Required]
    public DateTime DataVenda { get; set; } = DateTime.Today;

    public DateTime? DataEntrega { get; set; }

    public StatusVendaLocal Status { get; set; } = StatusVendaLocal.Orcamento;

    public decimal SubTotal { get; set; }

    public decimal ValorDesconto { get; set; }

    [Range(0, 100)]
    public decimal PercentualDesconto { get; set; }

    public decimal ValorFrete { get; set; }

    public decimal ValorTotal { get; set; }

    public FormaPagamentoLocal FormaPagamento { get; set; } = FormaPagamentoLocal.AVista;

    [Range(1, 48)]
    public int Parcelas { get; set; } = 1;

    [StringLength(500)]
    public string? Observacoes { get; set; }

    // Propriedades de navegação
    public ClienteLocal? Cliente { get; set; }
    public List<VendaItemLocal> Itens { get; set; } = new();

    // Propriedades calculadas
    public string StatusDescricao => Status switch
    {
        StatusVendaLocal.Orcamento => "Orçamento",
        StatusVendaLocal.Aprovado => "Aprovado",
        StatusVendaLocal.Faturado => "Faturado",
        StatusVendaLocal.Entregue => "Entregue",
        StatusVendaLocal.Cancelado => "Cancelado",
        _ => "Desconhecido"
    };

    public string FormaPagamentoDescricao => FormaPagamento switch
    {
        FormaPagamentoLocal.AVista => "À Vista",
        FormaPagamentoLocal.Boleto => "Boleto",
        FormaPagamentoLocal.CartaoCredito => "Cartão de Crédito",
        FormaPagamentoLocal.CartaoDebito => "Cartão de Débito",
        FormaPagamentoLocal.Pix => "PIX",
        FormaPagamentoLocal.Cheque => "Cheque",
        FormaPagamentoLocal.Parcelado => "Parcelado",
        _ => "Outro"
    };

    public int QuantidadeItens => Itens?.Count ?? 0;
    public string ValorTotalFormatado => ValorTotal.ToString("C2");
    public bool PodeEditar => Status == StatusVendaLocal.Orcamento || Status == StatusVendaLocal.Aprovado;
    public bool PodeAprovar => Status == StatusVendaLocal.Orcamento;
    public bool PodeFaturar => Status == StatusVendaLocal.Aprovado;
    public bool PodeCancelar => Status != StatusVendaLocal.Cancelado && Status != StatusVendaLocal.Faturado;
}

public class VendaItemLocal : BaseLocalModel
{
    [Required]
    public int VendaId { get; set; }

    [Required]
    public int ProdutoId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Quantidade { get; set; } = 1;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal PrecoUnitario { get; set; }

    [Range(0, 100)]
    public decimal PercentualDesconto { get; set; }

    public decimal ValorDesconto { get; set; }

    public decimal ValorTotal { get; set; }

    [StringLength(200)]
    public string? Observacoes { get; set; }

    // Propriedades de navegação
    public VendaLocal? Venda { get; set; }
    public ProdutoLocal? Produto { get; set; }

    // Propriedades calculadas
    public decimal ValorUnitarioComDesconto => PrecoUnitario - (PrecoUnitario * PercentualDesconto / 100);
    public string QuantidadeFormatada => $"{Quantidade:N2} {Produto?.Unidade ?? "UN"}";
    public string PrecoUnitarioFormatado => PrecoUnitario.ToString("C2");
    public string ValorTotalFormatado => ValorTotal.ToString("C2");
}

public enum StatusVendaLocal
{
    Orcamento = 0,
    Aprovado = 1,
    Faturado = 2,
    Entregue = 3,
    Cancelado = 4
}

public enum FormaPagamentoLocal
{
    AVista = 0,
    Boleto = 1,
    CartaoCredito = 2,
    CartaoDebito = 3,
    Pix = 4,
    Cheque = 5,
    Parcelado = 6
}