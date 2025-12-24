namespace Orama.Web.Models;

/// <summary>
/// ViewModel para exibir dados do dashboard principal
/// </summary>
public class DashboardViewModel
{
    public int TotalClientes { get; set; }
    public int TotalFornecedores { get; set; }
    public int TotalProdutos { get; set; }
    public int ContasReceberVencidas { get; set; }
    public int ContasPagarVencidas { get; set; }
    public int EstoqueBaixo { get; set; }
    public decimal VendasMes { get; set; }
    public decimal ComprasMes { get; set; }
    
    /// <summary>
    /// Calcula o saldo do mês (vendas - compras)
    /// </summary>
    public decimal SaldoMes => VendasMes - ComprasMes;
    
    /// <summary>
    /// Indica se há alertas importantes
    /// </summary>
    public bool TemAlertas => ContasReceberVencidas > 0 || ContasPagarVencidas > 0 || EstoqueBaixo > 0;
}