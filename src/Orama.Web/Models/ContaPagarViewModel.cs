using Orama.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Orama.Web.Models;

public class ContaPagarViewModel
{
    public int Id { get; set; }

    [Display(Name = "Fornecedor")]
    public int? FornecedorId { get; set; }
    public string? FornecedorNome { get; set; }

    [Required(ErrorMessage = "Descrição é obrigatória")]
    [StringLength(200)]
    [Display(Name = "Descrição")]
    public string Descricao { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "Nº Documento")]
    public string? NumeroDocumento { get; set; }

    [Display(Name = "Data Emissão")]
    [DataType(DataType.Date)]
    public DateTime DataEmissao { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Data de vencimento é obrigatória")]
    [Display(Name = "Data Vencimento")]
    [DataType(DataType.Date)]
    public DateTime DataVencimento { get; set; } = DateTime.Today.AddDays(30);

    [Display(Name = "Data Pagamento")]
    [DataType(DataType.Date)]
    public DateTime? DataPagamento { get; set; }

    [Required(ErrorMessage = "Valor é obrigatório")]
    [Display(Name = "Valor Original")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal ValorOriginal { get; set; }

    [Display(Name = "Juros")]
    public decimal ValorJuros { get; set; }

    [Display(Name = "Multa")]
    public decimal ValorMulta { get; set; }

    [Display(Name = "Desconto")]
    public decimal ValorDesconto { get; set; }

    [Display(Name = "Valor Pago")]
    public decimal ValorPago { get; set; }

    [Display(Name = "Status")]
    public StatusConta Status { get; set; } = StatusConta.Aberta;

    [Display(Name = "Conta Bancária")]
    public int? ContaBancariaId { get; set; }

    [Display(Name = "Forma de Pagamento")]
    public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.Dinheiro;

    [StringLength(500)]
    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }

    public DateTime DataCriacao { get; set; }
    public bool Ativo { get; set; } = true;

    public decimal ValorTotal => ValorOriginal + ValorJuros + ValorMulta - ValorDesconto;
    public decimal SaldoDevedor => ValorTotal - ValorPago;
    public bool Vencida => Status == StatusConta.Aberta && DataVencimento < DateTime.Today;
    public int DiasAtraso => Vencida ? (DateTime.Today - DataVencimento).Days : 0;

    public string StatusDescricao => Status switch
    {
        StatusConta.Aberta => "Aberta",
        StatusConta.Parcial => "Parcial",
        StatusConta.Paga => "Paga",
        StatusConta.Cancelada => "Cancelada",
        _ => "Desconhecido"
    };

    public string StatusCor => Status switch
    {
        StatusConta.Aberta => Vencida ? "danger" : "warning",
        StatusConta.Parcial => "info",
        StatusConta.Paga => "success",
        StatusConta.Cancelada => "secondary",
        _ => "secondary"
    };

    public ContaPagar ToEntity(int empresaId)
    {
        return new ContaPagar
        {
            Id = Id,
            EmpresaId = empresaId,
            FornecedorId = FornecedorId,
            Descricao = Descricao,
            NumeroDocumento = NumeroDocumento,
            DataEmissao = DataEmissao,
            DataVencimento = DataVencimento,
            DataPagamento = DataPagamento,
            ValorOriginal = ValorOriginal,
            ValorJuros = ValorJuros,
            ValorMulta = ValorMulta,
            ValorDesconto = ValorDesconto,
            ValorPago = ValorPago,
            Status = Status,
            ContaBancariaId = ContaBancariaId,
            FormaPagamento = FormaPagamento,
            Observacoes = Observacoes,
            Ativo = Ativo
        };
    }

    public static ContaPagarViewModel FromEntity(ContaPagar conta)
    {
        return new ContaPagarViewModel
        {
            Id = conta.Id,
            FornecedorId = conta.FornecedorId,
            FornecedorNome = conta.Fornecedor?.Nome,
            Descricao = conta.Descricao,
            NumeroDocumento = conta.NumeroDocumento,
            DataEmissao = conta.DataEmissao,
            DataVencimento = conta.DataVencimento,
            DataPagamento = conta.DataPagamento,
            ValorOriginal = conta.ValorOriginal,
            ValorJuros = conta.ValorJuros,
            ValorMulta = conta.ValorMulta,
            ValorDesconto = conta.ValorDesconto,
            ValorPago = conta.ValorPago,
            Status = conta.Status,
            ContaBancariaId = conta.ContaBancariaId,
            FormaPagamento = conta.FormaPagamento,
            Observacoes = conta.Observacoes,
            DataCriacao = conta.DataCriacao,
            Ativo = conta.Ativo
        };
    }
}

public class PagarContaViewModel
{
    public int ContaId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal SaldoDevedor { get; set; }

    [Required(ErrorMessage = "Valor é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    [Display(Name = "Valor a Pagar")]
    public decimal ValorPago { get; set; }

    [Required(ErrorMessage = "Conta bancária é obrigatória")]
    [Display(Name = "Conta Bancária")]
    public int ContaBancariaId { get; set; }

    [Required(ErrorMessage = "Data é obrigatória")]
    [Display(Name = "Data do Pagamento")]
    [DataType(DataType.Date)]
    public DateTime DataPagamento { get; set; } = DateTime.Today;
}
