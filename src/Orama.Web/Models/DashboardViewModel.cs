using System.ComponentModel.DataAnnotations;

namespace Orama.Web.Models;

/// <summary>
/// ViewModel para o dashboard principal do sistema
/// </summary>
public class DashboardViewModel
{
    // Informações do usuário
    public string NomeUsuario { get; set; } = string.Empty;
    public string PerfilUsuario { get; set; } = string.Empty;
    public DateTime? UltimoLogin { get; set; }

    // Métricas principais
    [Display(Name = "Total de Clientes")]
    public int TotalClientes { get; set; }
    
    [Display(Name = "Total de Fornecedores")]
    public int TotalFornecedores { get; set; }
    
    [Display(Name = "Total de Produtos")]
    public int TotalProdutos { get; set; }
    
    [Display(Name = "Contas a Receber Vencidas")]
    public int ContasReceberVencidas { get; set; }
    
    [Display(Name = "Contas a Pagar Vencidas")]
    public int ContasPagarVencidas { get; set; }
    
    [Display(Name = "Produtos com Estoque Baixo")]
    public int EstoqueBaixo { get; set; }
    
    [Display(Name = "Vendas do Mês")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal VendasMes { get; set; }
    
    [Display(Name = "Compras do Mês")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal ComprasMes { get; set; }
    
    [Display(Name = "Valor Total do Estoque")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal ValorEstoque { get; set; }
    
    /// <summary>
    /// Calcula o saldo do mês (vendas - compras)
    /// </summary>
    [Display(Name = "Saldo do Mês")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal SaldoMes => VendasMes - ComprasMes;
    
    /// <summary>
    /// Indica se há alertas importantes
    /// </summary>
    public bool TemAlertas => ContasReceberVencidas > 0 || ContasPagarVencidas > 0 || EstoqueBaixo > 0;
    
    /// <summary>
    /// Número total de alertas
    /// </summary>
    public int TotalAlertas => ContasReceberVencidas + ContasPagarVencidas + EstoqueBaixo;
    
    /// <summary>
    /// Percentual de crescimento das vendas (simulado)
    /// </summary>
    public decimal PercentualCrescimentoVendas { get; set; } = 12.5m;
    
    /// <summary>
    /// Margem de lucro bruta (simulada)
    /// </summary>
    public decimal MargemLucroBruta => VendasMes > 0 ? ((VendasMes - ComprasMes) / VendasMes) * 100 : 0;
}