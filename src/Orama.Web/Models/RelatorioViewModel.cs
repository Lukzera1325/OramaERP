using System.ComponentModel.DataAnnotations;

namespace Orama.Web.Models;

/// <summary>
/// ViewModel base para relatórios
/// </summary>
public abstract class RelatorioBaseViewModel
{
    [Display(Name = "Data Início")]
    [DataType(DataType.Date)]
    public DateTime DataInicio { get; set; }
    
    [Display(Name = "Data Fim")]
    [DataType(DataType.Date)]
    public DateTime DataFim { get; set; }
    
    public string PeriodoFormatado => $"{DataInicio:dd/MM/yyyy} a {DataFim:dd/MM/yyyy}";
}

/// <summary>
/// ViewModel para dashboard de Business Intelligence
/// </summary>
public class BIDashboardViewModel
{
    // KPIs principais
    [Display(Name = "Vendas do Mês")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal VendasMes { get; set; }

    [Display(Name = "Compras do Mês")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal ComprasMes { get; set; }

    [Display(Name = "Lucro do Mês")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal LucroMes { get; set; }

    [Display(Name = "Contas a Receber Vencidas")]
    public int ContasReceberVencidas { get; set; }

    [Display(Name = "Contas a Pagar Vencidas")]
    public int ContasPagarVencidas { get; set; }

    [Display(Name = "Valor a Receber Vencido")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal ValorReceberVencido { get; set; }

    [Display(Name = "Valor a Pagar Vencido")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal ValorPagarVencido { get; set; }

    // Comparativos
    [Display(Name = "Crescimento Vendas (%)")]
    [DisplayFormat(DataFormatString = "{0:N2}%", ApplyFormatInEditMode = false)]
    public decimal CrescimentoVendas { get; set; }

    [Display(Name = "Crescimento Compras (%)")]
    [DisplayFormat(DataFormatString = "{0:N2}%", ApplyFormatInEditMode = false)]
    public decimal CrescimentoCompras { get; set; }

    // Dados para gráficos
    public List<VendaMensalViewModel> VendasUltimos12Meses { get; set; } = new();
    public List<ProdutoTopViewModel> TopProdutos { get; set; } = new();
    public List<ClienteTopViewModel> TopClientes { get; set; } = new();

    // Informações gerais
    public string PeriodoAtual { get; set; } = string.Empty;
    public DateTime DataAtualizacao { get; set; }

    // Propriedades calculadas
    public string StatusVendas => CrescimentoVendas >= 0 ? "success" : "danger";
    public string IconeVendas => CrescimentoVendas >= 0 ? "fa-arrow-up" : "fa-arrow-down";
    public decimal MargemLucro => VendasMes > 0 ? (LucroMes / VendasMes) * 100 : 0;
}

/// <summary>
/// ViewModel para vendas mensais (gráfico)
/// </summary>
public class VendaMensalViewModel
{
    public string Mes { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime Periodo { get; set; }
}

/// <summary>
/// ViewModel para top produtos
/// </summary>
public class ProdutoTopViewModel
{
    public string Nome { get; set; } = string.Empty;
    public int Vendas { get; set; }
    public decimal Valor { get; set; }
}

/// <summary>
/// ViewModel para top clientes
/// </summary>
public class ClienteTopViewModel
{
    public string Nome { get; set; } = string.Empty;
    public int Vendas { get; set; }
    public decimal Valor { get; set; }
}

/// <summary>
/// ViewModel para relatório de vendas
/// </summary>
public class RelatorioVendasViewModel : RelatorioBaseViewModel
{
    [Display(Name = "Total de Vendas")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal TotalVendas { get; set; }
    
    [Display(Name = "Quantidade de Vendas")]
    public int QuantidadeVendas { get; set; }
    
    [Display(Name = "Ticket Médio")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal TicketMedio { get; set; }
    
    public List<VendaResumoViewModel> Vendas { get; set; } = new();
}

/// <summary>
/// ViewModel para resumo de venda no relatório
/// </summary>
public class VendaResumoViewModel
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;
    public DateTime DataVenda { get; set; }
    
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal ValorTotal { get; set; }
    
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel para relatório de compras
/// </summary>
public class RelatorioComprasViewModel : RelatorioBaseViewModel
{
    [Display(Name = "Total de Compras")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal TotalCompras { get; set; }
    
    [Display(Name = "Quantidade de Compras")]
    public int QuantidadeCompras { get; set; }
    
    [Display(Name = "Ticket Médio")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal TicketMedio { get; set; }
    
    public List<CompraResumoViewModel> Compras { get; set; } = new();
}

/// <summary>
/// ViewModel para resumo de compra no relatório
/// </summary>
public class CompraResumoViewModel
{
    public int Id { get; set; }
    public string NumeroCompra { get; set; } = string.Empty;
    public string FornecedorNome { get; set; } = string.Empty;
    public DateTime DataCompra { get; set; }
    
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal ValorTotal { get; set; }
    
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel para relatório financeiro
/// </summary>
public class RelatorioFinanceiroViewModel : RelatorioBaseViewModel
{
    [Display(Name = "Total Contas a Receber")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal TotalContasReceber { get; set; }
    
    [Display(Name = "Total Contas a Pagar")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal TotalContasPagar { get; set; }
    
    [Display(Name = "Contas a Receber Vencidas")]
    public int ContasReceberVencidas { get; set; }
    
    [Display(Name = "Contas a Pagar Vencidas")]
    public int ContasPagarVencidas { get; set; }
    
    [Display(Name = "Valor a Receber Vencido")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal ValorReceberVencido { get; set; }
    
    [Display(Name = "Valor a Pagar Vencido")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal ValorPagarVencido { get; set; }
    
    [Display(Name = "Saldo do Período")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal SaldoPeriodo => TotalContasReceber - TotalContasPagar;
}

/// <summary>
/// ViewModel para DRE simplificado
/// </summary>
public class RelatorioDREViewModel : RelatorioBaseViewModel
{
    [Display(Name = "Receita Bruta")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal ReceitaBruta { get; set; }
    
    [Display(Name = "(-) Custo da Mercadoria Vendida")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal CustoMercadoriaVendida { get; set; }
    
    [Display(Name = "Lucro Bruto")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal LucroBruto { get; set; }
    
    [Display(Name = "(-) Despesas Operacionais")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal DespesasOperacionais { get; set; }
    
    [Display(Name = "Lucro Líquido")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal LucroLiquido { get; set; }
    
    [Display(Name = "Margem Bruta (%)")]
    [DisplayFormat(DataFormatString = "{0:N2}%", ApplyFormatInEditMode = false)]
    public decimal MargemBruta => ReceitaBruta > 0 ? (LucroBruto / ReceitaBruta) * 100 : 0;
    
    [Display(Name = "Margem Líquida (%)")]
    [DisplayFormat(DataFormatString = "{0:N2}%", ApplyFormatInEditMode = false)]
    public decimal MargemLiquida => ReceitaBruta > 0 ? (LucroLiquido / ReceitaBruta) * 100 : 0;
}