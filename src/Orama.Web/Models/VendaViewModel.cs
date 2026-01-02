using Orama.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Orama.Web.Models;

public class VendaViewModel
{
    public int Id { get; set; }

    [Display(Name = "Número")]
    public string Numero { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cliente é obrigatório")]
    [Display(Name = "Cliente")]
    public int ClienteId { get; set; }
    public string? ClienteNome { get; set; }

    [Display(Name = "Vendedor")]
    public int? VendedorId { get; set; }
    public string? VendedorNome { get; set; }

    [Required(ErrorMessage = "Data da venda é obrigatória")]
    [Display(Name = "Data da Venda")]
    [DataType(DataType.Date)]
    public DateTime DataVenda { get; set; } = DateTime.Today;

    [Display(Name = "Data de Entrega")]
    [DataType(DataType.Date)]
    public DateTime? DataEntrega { get; set; }

    [Display(Name = "Status")]
    public StatusVenda Status { get; set; } = StatusVenda.Orcamento;

    [Display(Name = "Subtotal")]
    public decimal SubTotal { get; set; }

    [Display(Name = "Desconto (R$)")]
    public decimal ValorDesconto { get; set; }

    [Display(Name = "Desconto (%)")]
    [Range(0, 100)]
    public decimal PercentualDesconto { get; set; }

    [Display(Name = "Frete")]
    public decimal ValorFrete { get; set; }

    [Display(Name = "Total")]
    public decimal ValorTotal { get; set; }

    [Required(ErrorMessage = "Forma de pagamento é obrigatória")]
    [Display(Name = "Forma de Pagamento")]
    public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.Dinheiro;

    [Display(Name = "Parcelas")]
    [Range(1, 48)]
    public int Parcelas { get; set; } = 1;

    [StringLength(500)]
    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }

    public List<VendaItemViewModel> Itens { get; set; } = new();

    public string StatusDescricao => Status switch
    {
        StatusVenda.Orcamento => "Orçamento",
        StatusVenda.Aprovado => "Aprovado",
        StatusVenda.Faturada => "Faturada",
        StatusVenda.Entregue => "Entregue",
        StatusVenda.Cancelado => "Cancelado",
        _ => "Desconhecido"
    };

    public string StatusCor => Status switch
    {
        StatusVenda.Orcamento => "secondary",
        StatusVenda.Aprovado => "info",
        StatusVenda.Faturada => "success",
        StatusVenda.Entregue => "primary",
        StatusVenda.Cancelado => "danger",
        _ => "secondary"
    };

    public string FormaPagamentoDescricao => FormaPagamento switch
    {
        FormaPagamento.Dinheiro => "Dinheiro",
        FormaPagamento.CartaoCredito => "Cartão de Crédito",
        FormaPagamento.CartaoDebito => "Cartão de Débito",
        FormaPagamento.Pix => "PIX",
        FormaPagamento.Boleto => "Boleto",
        FormaPagamento.Transferencia => "Transferência",
        FormaPagamento.Cheque => "Cheque",
        _ => "Outro"
    };

    public Venda ToEntity(int empresaId)
    {
        var venda = new Venda
        {
            Id = Id,
            EmpresaId = empresaId,
            Numero = Numero,
            ClienteId = ClienteId,
            VendedorId = VendedorId,
            DataVenda = DataVenda,
            DataEntrega = DataEntrega,
            Status = Status,
            SubTotal = SubTotal,
            ValorDesconto = ValorDesconto,
            PercentualDesconto = PercentualDesconto,
            ValorFrete = ValorFrete,
            ValorTotal = ValorTotal,
            FormaPagamento = FormaPagamento,
            Parcelas = Parcelas,
            Observacoes = Observacoes
        };

        foreach (var item in Itens)
        {
            venda.Itens.Add(new VendaItem
            {
                ProdutoId = item.ProdutoId,
                Quantidade = item.Quantidade,
                PrecoUnitario = item.PrecoUnitario,
                PercentualDesconto = item.PercentualDesconto,
                ValorDesconto = item.ValorDesconto,
                ValorTotal = item.ValorTotal,
                Observacoes = item.Observacoes,
                Ativo = true,
                DataCriacao = DateTime.Now
            });
        }

        return venda;
    }

    public static VendaViewModel FromEntity(Venda venda)
    {
        var vm = new VendaViewModel
        {
            Id = venda.Id,
            Numero = venda.Numero,
            ClienteId = venda.ClienteId,
            ClienteNome = venda.Cliente?.Nome,
            VendedorId = venda.VendedorId,
            VendedorNome = venda.Vendedor?.Nome,
            DataVenda = venda.DataVenda,
            DataEntrega = venda.DataEntrega,
            Status = venda.Status,
            SubTotal = venda.SubTotal,
            ValorDesconto = venda.ValorDesconto,
            PercentualDesconto = venda.PercentualDesconto,
            ValorFrete = venda.ValorFrete,
            ValorTotal = venda.ValorTotal,
            FormaPagamento = venda.FormaPagamento,
            Parcelas = venda.Parcelas,
            Observacoes = venda.Observacoes
        };

        foreach (var item in venda.Itens)
        {
            vm.Itens.Add(VendaItemViewModel.FromEntity(item));
        }

        return vm;
    }
}

public class VendaItemViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Produto é obrigatório")]
    [Display(Name = "Produto")]
    public int ProdutoId { get; set; }
    public string? ProdutoNome { get; set; }
    public string? ProdutoCodigo { get; set; }

    [Required(ErrorMessage = "Quantidade é obrigatória")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
    [Display(Name = "Quantidade")]
    public decimal Quantidade { get; set; } = 1;

    [Required(ErrorMessage = "Preço é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Preço deve ser maior que zero")]
    [Display(Name = "Preço Unitário")]
    public decimal PrecoUnitario { get; set; }

    [Display(Name = "Desconto (%)")]
    [Range(0, 100)]
    public decimal PercentualDesconto { get; set; }

    [Display(Name = "Desconto (R$)")]
    public decimal ValorDesconto { get; set; }

    [Display(Name = "Total")]
    public decimal ValorTotal { get; set; }

    [StringLength(200)]
    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }

    public static VendaItemViewModel FromEntity(VendaItem item)
    {
        return new VendaItemViewModel
        {
            Id = item.Id,
            ProdutoId = item.ProdutoId,
            ProdutoNome = item.Produto?.Descricao,
            ProdutoCodigo = item.Produto?.Codigo,
            Quantidade = item.Quantidade,
            PrecoUnitario = item.PrecoUnitario,
            PercentualDesconto = item.PercentualDesconto,
            ValorDesconto = item.ValorDesconto,
            ValorTotal = item.ValorTotal,
            Observacoes = item.Observacoes
        };
    }
}
