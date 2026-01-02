using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orama.Domain.Entities;

namespace Orama.Web.Models;

/// <summary>
/// ViewModel para compras
/// </summary>
public class CompraViewModel
{
    public int Id { get; set; }
    
    [Display(Name = "Número da Compra")]
    public string NumeroCompra { get; set; } = string.Empty;

    [Required(ErrorMessage = "Fornecedor é obrigatório")]
    [Display(Name = "Fornecedor")]
    public int FornecedorId { get; set; }
    
    [Display(Name = "Fornecedor")]
    public string? FornecedorNome { get; set; }

    [Display(Name = "Número da NF")]
    [StringLength(50, ErrorMessage = "Número da NF deve ter no máximo 50 caracteres")]
    public string? NumeroNF { get; set; }

    [Required(ErrorMessage = "Data da compra é obrigatória")]
    [Display(Name = "Data da Compra")]
    [DataType(DataType.Date)]
    public DateTime DataCompra { get; set; } = DateTime.Now;

    [Display(Name = "Data de Entrega")]
    [DataType(DataType.Date)]
    public DateTime? DataEntrega { get; set; }

    [Display(Name = "Data de Recebimento")]
    [DataType(DataType.Date)]
    public DateTime? DataRecebimento { get; set; }

    [Display(Name = "Status")]
    public StatusCompra Status { get; set; }

    [Display(Name = "Status")]
    public string StatusDescricao => Status switch
    {
        StatusCompra.Pedido => "Pedido",
        StatusCompra.Aprovada => "Aprovada",
        StatusCompra.Recebida => "Recebida",
        StatusCompra.Cancelada => "Cancelada",
        _ => "Desconhecido"
    };

    [Display(Name = "Subtotal")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal SubTotal { get; set; }

    [Display(Name = "% Desconto")]
    [Range(0, 100, ErrorMessage = "Percentual de desconto deve estar entre 0 e 100")]
    public decimal PercentualDesconto { get; set; }

    [Display(Name = "Valor Desconto")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal ValorDesconto { get; set; }

    [Display(Name = "Valor Frete")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal ValorFrete { get; set; }

    [Display(Name = "Valor Total")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal ValorTotal { get; set; }

    [Display(Name = "Forma de Pagamento")]
    public FormaPagamento FormaPagamento { get; set; }

    [Display(Name = "Forma de Pagamento")]
    public string FormaPagamentoDescricao => FormaPagamento switch
    {
        FormaPagamento.Dinheiro => "Dinheiro",
        FormaPagamento.CartaoCredito => "Cartão de Crédito",
        FormaPagamento.CartaoDebito => "Cartão de Débito",
        FormaPagamento.Pix => "PIX",
        FormaPagamento.Boleto => "Boleto",
        FormaPagamento.Transferencia => "Transferência",
        FormaPagamento.Cheque => "Cheque",
        _ => "Não Informado"
    };

    [Display(Name = "Parcelas")]
    [Range(1, 60, ErrorMessage = "Número de parcelas deve estar entre 1 e 60")]
    public int Parcelas { get; set; } = 1;

    [Display(Name = "Observações")]
    [StringLength(500, ErrorMessage = "Observações devem ter no máximo 500 caracteres")]
    public string? Observacoes { get; set; }

    [Display(Name = "Data de Criação")]
    public DateTime DataCriacao { get; set; }

    // Propriedades para os selects
    public SelectList? Fornecedores { get; set; }
    public SelectList? FormasPagamento { get; set; }

    // Itens da compra
    public List<CompraItemViewModel> Itens { get; set; } = new();

    // Propriedades auxiliares
    public bool PodeEditar => Status == StatusCompra.Pedido;
    public bool PodeAprovar => Status == StatusCompra.Pedido;
    public bool PodeReceber => Status == StatusCompra.Aprovada || Status == StatusCompra.Pedido;
    public bool PodeCancelar => Status != StatusCompra.Cancelada;
    public bool EstaRecebida => Status == StatusCompra.Recebida;
    public bool EstaCancelada => Status == StatusCompra.Cancelada;

    public string StatusBadgeClass => Status switch
    {
        StatusCompra.Pedido => "bg-warning",
        StatusCompra.Aprovada => "bg-info",
        StatusCompra.Recebida => "bg-success",
        StatusCompra.Cancelada => "bg-danger",
        _ => "bg-secondary"
    };
}

/// <summary>
/// ViewModel para itens de compra
/// </summary>
public class CompraItemViewModel
{
    public int Id { get; set; }
    public int CompraId { get; set; }

    [Required(ErrorMessage = "Produto é obrigatório")]
    [Display(Name = "Produto")]
    public int ProdutoId { get; set; }

    [Display(Name = "Produto")]
    public string? ProdutoNome { get; set; }

    [Display(Name = "Código")]
    public string? ProdutoCodigo { get; set; }

    [Required(ErrorMessage = "Quantidade é obrigatória")]
    [Display(Name = "Quantidade")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
    public decimal Quantidade { get; set; }

    [Required(ErrorMessage = "Valor unitário é obrigatório")]
    [Display(Name = "Valor Unitário")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor unitário deve ser maior que zero")]
    public decimal ValorUnitario { get; set; }

    [Display(Name = "Valor Total")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal ValorTotal { get; set; }

    [Display(Name = "Observações")]
    [StringLength(200, ErrorMessage = "Observações devem ter no máximo 200 caracteres")]
    public string? Observacoes { get; set; }

    // Propriedades auxiliares
    public SelectList? Produtos { get; set; }
}

/// <summary>
/// ViewModel para filtros de compra
/// </summary>
public class CompraFiltroViewModel
{
    [Display(Name = "Fornecedor")]
    public int? FornecedorId { get; set; }

    [Display(Name = "Status")]
    public StatusCompra? Status { get; set; }

    [Display(Name = "Data Inicial")]
    [DataType(DataType.Date)]
    public DateTime? DataInicial { get; set; }

    [Display(Name = "Data Final")]
    [DataType(DataType.Date)]
    public DateTime? DataFinal { get; set; }

    [Display(Name = "Número da Compra")]
    public string? NumeroCompra { get; set; }

    [Display(Name = "Número da NF")]
    public string? NumeroNF { get; set; }

    // Propriedades para os selects
    public SelectList? Fornecedores { get; set; }
    public SelectList? StatusList { get; set; }
}