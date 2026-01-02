using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities;

/// <summary>
/// Entidade que representa uma conta bancária da empresa
/// </summary>
public class ContaBancaria : BaseEntity
{
    public int EmpresaId { get; set; }
    public virtual Empresa? Empresa { get; set; }

    [Required(ErrorMessage = "Descrição é obrigatória")]
    [StringLength(100, ErrorMessage = "Descrição deve ter no máximo 100 caracteres")]
    public string Descricao { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Banco { get; set; }

    [StringLength(10)]
    public string? Agencia { get; set; }

    [StringLength(20)]
    public string? NumeroConta { get; set; }

    public TipoConta TipoConta { get; set; } = TipoConta.ContaCorrente;

    public decimal SaldoInicial { get; set; }
    public decimal SaldoAtual { get; set; }

    public bool ContaPadrao { get; set; }

    // Relacionamentos
    public virtual ICollection<MovimentacaoFinanceira> Movimentacoes { get; set; } = new List<MovimentacaoFinanceira>();
}

public enum TipoConta
{
    ContaCorrente = 1,
    Poupanca = 2,
    Caixa = 3,
    Investimento = 4
}

/// <summary>
/// Entidade que representa uma conta a receber
/// </summary>
public class ContaReceber : BaseEntity
{
    public int EmpresaId { get; set; }
    public virtual Empresa? Empresa { get; set; }

    public int? ClienteId { get; set; }
    public virtual Cliente? Cliente { get; set; }

    public int? VendaId { get; set; }

    [Required(ErrorMessage = "Descrição é obrigatória")]
    [StringLength(200)]
    public string Descricao { get; set; } = string.Empty;

    [StringLength(50)]
    public string? NumeroDocumento { get; set; }

    public DateTime DataEmissao { get; set; } = DateTime.Today;
    public DateTime DataVencimento { get; set; }
    public DateTime? DataRecebimento { get; set; }

    public decimal ValorOriginal { get; set; }
    public decimal ValorJuros { get; set; }
    public decimal ValorMulta { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorRecebido { get; set; }

    public StatusConta Status { get; set; } = StatusConta.Aberta;

    public int? ContaBancariaId { get; set; }
    public virtual ContaBancaria? ContaBancaria { get; set; }

    public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.Dinheiro;

    [StringLength(500)]
    public string? Observacoes { get; set; }

    // Propriedades calculadas
    public decimal ValorTotal => ValorOriginal + ValorJuros + ValorMulta - ValorDesconto;
    public decimal SaldoDevedor => ValorTotal - ValorRecebido;
    public bool Vencida => Status == StatusConta.Aberta && DataVencimento < DateTime.Today;
    public int DiasAtraso => Vencida ? (DateTime.Today - DataVencimento).Days : 0;
}

/// <summary>
/// Entidade que representa uma conta a pagar
/// </summary>
public class ContaPagar : BaseEntity
{
    public int EmpresaId { get; set; }
    public virtual Empresa? Empresa { get; set; }

    public int? FornecedorId { get; set; }
    public virtual Fornecedor? Fornecedor { get; set; }

    public int? CompraId { get; set; }

    [Required(ErrorMessage = "Descrição é obrigatória")]
    [StringLength(200)]
    public string Descricao { get; set; } = string.Empty;

    [StringLength(50)]
    public string? NumeroDocumento { get; set; }

    public DateTime DataEmissao { get; set; } = DateTime.Today;
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }

    public decimal ValorOriginal { get; set; }
    public decimal ValorJuros { get; set; }
    public decimal ValorMulta { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorPago { get; set; }

    public StatusConta Status { get; set; } = StatusConta.Aberta;

    public int? ContaBancariaId { get; set; }
    public virtual ContaBancaria? ContaBancaria { get; set; }

    public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.Dinheiro;

    // Campos para aprovação
    public StatusAprovacao StatusAprovacao { get; set; } = StatusAprovacao.Pendente;
    public int? UsuarioAprovadorId { get; set; }
    public DateTime? DataAprovacao { get; set; }
    public string? MotivoReprovacao { get; set; }

    // Campos para agendamento
    public DateTime? DataAgendamento { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }

    // Propriedades calculadas
    public decimal ValorTotal => ValorOriginal + ValorJuros + ValorMulta - ValorDesconto;
    public decimal SaldoDevedor => ValorTotal - ValorPago;
    public bool Vencida => Status == StatusConta.Aberta && DataVencimento < DateTime.Today;
    public int DiasAtraso => Vencida ? (DateTime.Today - DataVencimento).Days : 0;
}

public enum StatusConta
{
    Aberta = 1,
    Parcial = 2,
    Paga = 3,
    Cancelada = 4,
    Agendada = 5
}

public enum StatusAprovacao
{
    Pendente = 1,
    Aprovada = 2,
    Reprovada = 3
}

/// <summary>
/// Movimentação financeira (entrada/saída de caixa)
/// </summary>
public class MovimentacaoFinanceira : BaseEntity
{
    public int EmpresaId { get; set; }
    public virtual Empresa? Empresa { get; set; }

    public int ContaBancariaId { get; set; }
    public virtual ContaBancaria ContaBancaria { get; set; } = null!;

    public TipoMovimentacao Tipo { get; set; }

    [Required]
    [StringLength(200)]
    public string Descricao { get; set; } = string.Empty;

    public DateTime DataMovimentacao { get; set; } = DateTime.Now;

    public decimal Valor { get; set; }
    public decimal SaldoAnterior { get; set; }
    public decimal SaldoPosterior { get; set; }

    // Referências opcionais
    public int? ContaReceberId { get; set; }
    public int? ContaPagarId { get; set; }
    public int? VendaId { get; set; }
    public int? CompraId { get; set; }

    // Campos para conciliação
    public bool Conciliada { get; set; } = false;
    public DateTime? DataConciliacao { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }
}

public enum TipoMovimentacao
{
    Entrada = 1,
    Saida = 2,
    Transferencia = 3
}
