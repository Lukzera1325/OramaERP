namespace Orama.Web.Models;

/// <summary>
/// ViewModel para fluxo de caixa
/// </summary>
public class FluxoCaixaViewModel
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public decimal SaldoAtual { get; set; }
    public decimal TotalAReceber { get; set; }
    public decimal TotalAPagar { get; set; }
    public decimal SaldoProjetado { get; set; }
    public List<MovimentacaoFluxoViewModel> Movimentacoes { get; set; } = new();
}

/// <summary>
/// ViewModel para movimentação do fluxo
/// </summary>
public class MovimentacaoFluxoViewModel
{
    public DateTime Data { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string ContaBancaria { get; set; } = string.Empty;
    public decimal SaldoAnterior { get; set; }
    public decimal SaldoPosterior { get; set; }
}