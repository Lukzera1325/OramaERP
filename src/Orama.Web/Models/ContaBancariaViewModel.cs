using Orama.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Orama.Web.Models;

public class ContaBancariaViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Descrição é obrigatória")]
    [StringLength(100)]
    [Display(Name = "Descrição")]
    public string Descricao { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Banco")]
    public string? Banco { get; set; }

    [StringLength(10)]
    [Display(Name = "Agência")]
    public string? Agencia { get; set; }

    [StringLength(20)]
    [Display(Name = "Número da Conta")]
    public string? NumeroConta { get; set; }

    [Display(Name = "Tipo de Conta")]
    public TipoConta TipoConta { get; set; } = TipoConta.ContaCorrente;

    [Display(Name = "Saldo Inicial")]
    [Range(0, double.MaxValue)]
    public decimal SaldoInicial { get; set; }

    [Display(Name = "Saldo Atual")]
    public decimal SaldoAtual { get; set; }

    [Display(Name = "Conta Padrão")]
    public bool ContaPadrao { get; set; }

    public DateTime DataCriacao { get; set; }
    public bool Ativo { get; set; } = true;

    public string TipoContaDescricao => TipoConta switch
    {
        TipoConta.ContaCorrente => "Conta Corrente",
        TipoConta.Poupanca => "Poupança",
        TipoConta.Caixa => "Caixa",
        TipoConta.Investimento => "Investimento",
        _ => "Outro"
    };

    public ContaBancaria ToEntity(int empresaId)
    {
        return new ContaBancaria
        {
            Id = Id,
            EmpresaId = empresaId,
            Descricao = Descricao,
            Banco = Banco,
            Agencia = Agencia,
            NumeroConta = NumeroConta,
            TipoConta = TipoConta,
            SaldoInicial = SaldoInicial,
            SaldoAtual = SaldoAtual,
            ContaPadrao = ContaPadrao,
            Ativo = Ativo
        };
    }

    public static ContaBancariaViewModel FromEntity(ContaBancaria conta)
    {
        return new ContaBancariaViewModel
        {
            Id = conta.Id,
            Descricao = conta.Descricao,
            Banco = conta.Banco,
            Agencia = conta.Agencia,
            NumeroConta = conta.NumeroConta,
            TipoConta = conta.TipoConta,
            SaldoInicial = conta.SaldoInicial,
            SaldoAtual = conta.SaldoAtual,
            ContaPadrao = conta.ContaPadrao,
            DataCriacao = conta.DataCriacao,
            Ativo = conta.Ativo
        };
    }
}
