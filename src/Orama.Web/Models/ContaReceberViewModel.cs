using Orama.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Orama.Web.Models;

public class ContaReceberViewModel
{
    public int Id { get; set; }

    [Display(Name = "Cliente")]
    public int? ClienteId { get; set; }
    public string? ClienteNome { get; set; }

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

    [Display(Name = "Data Recebimento")]
    [DataType(DataType.Date)]
    public DateTime? DataRecebimento { get; set; }

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

    [Display(Name = "Valor Recebido")]
    public decimal ValorRecebido { get; set; }

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

    // Propriedades calculadas
    public decimal ValorTotal => ValorOriginal + ValorJuros + ValorMulta - ValorDesconto;
    public decimal SaldoDevedor => ValorTotal - ValorRecebido;
    public bool Vencida => Status == StatusConta.Aberta && DataVencimento < DateTime.Today;
    public int DiasAtraso => Vencida ? (DateTime.Today - DataVencimento).Days : 0;

    public string StatusDescricao => Status switch
    {
        StatusConta.Aberta => "Aberta",
        StatusConta.Parcial => "Parcial",
        StatusConta.Paga => "Recebida",
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

    public ContaReceber ToEntity(int empresaId)
    {
        return new ContaReceber
        {
            Id = Id,
            EmpresaId = empresaId,
            ClienteId = ClienteId,
            Descricao = Descricao,
            NumeroDocumento = NumeroDocumento,
            DataEmissao = DataEmissao,
            DataVencimento = DataVencimento,
            DataRecebimento = DataRecebimento,
            ValorOriginal = ValorOriginal,
            ValorJuros = ValorJuros,
            ValorMulta = ValorMulta,
            ValorDesconto = ValorDesconto,
            ValorRecebido = ValorRecebido,
            Status = Status,
            ContaBancariaId = ContaBancariaId,
            FormaPagamento = FormaPagamento,
            Observacoes = Observacoes,
            Ativo = Ativo
        };
    }

    public static ContaReceberViewModel FromEntity(ContaReceber conta)
    {
        return new ContaReceberViewModel
        {
            Id = conta.Id,
            ClienteId = conta.ClienteId,
            ClienteNome = conta.Cliente?.Nome,
            Descricao = conta.Descricao,
            NumeroDocumento = conta.NumeroDocumento,
            DataEmissao = conta.DataEmissao,
            DataVencimento = conta.DataVencimento,
            DataRecebimento = conta.DataRecebimento,
            ValorOriginal = conta.ValorOriginal,
            ValorJuros = conta.ValorJuros,
            ValorMulta = conta.ValorMulta,
            ValorDesconto = conta.ValorDesconto,
            ValorRecebido = conta.ValorRecebido,
            Status = conta.Status,
            ContaBancariaId = conta.ContaBancariaId,
            FormaPagamento = conta.FormaPagamento,
            Observacoes = conta.Observacoes,
            DataCriacao = conta.DataCriacao,
            Ativo = conta.Ativo
        };
    }
}

public class ReceberContaViewModel
{
    public int ContaId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal SaldoDevedor { get; set; }

    [Required(ErrorMessage = "Valor é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    [Display(Name = "Valor a Receber")]
    public decimal ValorRecebido { get; set; }

    [Required(ErrorMessage = "Conta bancária é obrigatória")]
    [Display(Name = "Conta Bancária")]
    public int ContaBancariaId { get; set; }

    [Required(ErrorMessage = "Data é obrigatória")]
    [Display(Name = "Data do Recebimento")]
    [DataType(DataType.Date)]
    public DateTime DataRecebimento { get; set; } = DateTime.Today;
}
