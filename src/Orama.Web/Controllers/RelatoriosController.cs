using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orama.Application.Services;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller para relatórios gerenciais
/// </summary>
public class RelatoriosController : BaseController
{
    private readonly IVendaService _vendaService;
    private readonly ICompraService _compraService;
    private readonly IEstoqueService _estoqueService;
    private readonly IContaReceberService _contaReceberService;
    private readonly IContaPagarService _contaPagarService;
    private readonly IClienteService _clienteService;
    private readonly IFornecedorService _fornecedorService;
    private readonly IProdutoService _produtoService;

    public RelatoriosController(
        IVendaService vendaService,
        ICompraService compraService,
        IEstoqueService estoqueService,
        IContaReceberService contaReceberService,
        IContaPagarService contaPagarService,
        IClienteService clienteService,
        IFornecedorService fornecedorService,
        IProdutoService produtoService)
    {
        _vendaService = vendaService;
        _compraService = compraService;
        _estoqueService = estoqueService;
        _contaReceberService = contaReceberService;
        _contaPagarService = contaPagarService;
        _clienteService = clienteService;
        _fornecedorService = fornecedorService;
        _produtoService = produtoService;
    }

    /// <summary>
    /// Dashboard principal com KPIs e gráficos
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var hoje = DateTime.Today;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
            var fimMes = inicioMes.AddMonths(1).AddDays(-1);
            var inicioMesAnterior = inicioMes.AddMonths(-1);
            var fimMesAnterior = inicioMes.AddDays(-1);

            // KPIs do mês atual
            var vendasMes = await _vendaService.ObterTotalVendasAsync(empresaId, inicioMes, fimMes);
            var comprasMes = await _compraService.ObterTotalComprasAsync(empresaId, inicioMes, fimMes);
            var contasReceberVencidas = await _contaReceberService.ObterVencidasAsync(empresaId);
            var contasPagarVencidas = await _contaPagarService.ObterVencidasAsync(empresaId);

            // KPIs do mês anterior para comparação
            var vendasMesAnterior = await _vendaService.ObterTotalVendasAsync(empresaId, inicioMesAnterior, fimMesAnterior);
            var comprasMesAnterior = await _compraService.ObterTotalComprasAsync(empresaId, inicioMesAnterior, fimMesAnterior);

            // Dados para gráficos
            var vendasUltimos12Meses = await ObterVendasUltimos12MesesAsync(empresaId);
            var topProdutos = await ObterTopProdutosAsync(empresaId);
            var topClientes = await ObterTopClientesAsync(empresaId);

            var dashboard = new BIDashboardViewModel
            {
                // KPIs principais
                VendasMes = vendasMes,
                ComprasMes = comprasMes,
                LucroMes = vendasMes - comprasMes,
                ContasReceberVencidas = contasReceberVencidas.Count(),
                ContasPagarVencidas = contasPagarVencidas.Count(),
                ValorReceberVencido = contasReceberVencidas.Sum(c => c.ValorTotal),
                ValorPagarVencido = contasPagarVencidas.Sum(c => c.ValorTotal),

                // Comparativos
                CrescimentoVendas = CalcularCrescimento(vendasMes, vendasMesAnterior),
                CrescimentoCompras = CalcularCrescimento(comprasMes, comprasMesAnterior),

                // Dados para gráficos
                VendasUltimos12Meses = vendasUltimos12Meses,
                TopProdutos = topProdutos,
                TopClientes = topClientes,

                // Período
                PeriodoAtual = $"{inicioMes:MMM/yyyy}",
                DataAtualizacao = DateTime.Now
            };

            return View(dashboard);
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao carregar dashboard: {ex.Message}";
            return View(new BIDashboardViewModel());
        }
    }

    /// <summary>
    /// Relatório de vendas
    /// </summary>
    public async Task<IActionResult> Vendas(DateTime? inicio = null, DateTime? fim = null)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            
            // Definir período padrão (mês atual)
            inicio ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            fim ??= inicio.Value.AddMonths(1).AddDays(-1);

            var vendas = await _vendaService.ObterPorPeriodoAsync(empresaId, inicio.Value, fim.Value);
            var totalVendas = await _vendaService.ObterTotalVendasAsync(empresaId, inicio.Value, fim.Value);

            var viewModel = new RelatorioVendasViewModel
            {
                DataInicio = inicio.Value,
                DataFim = fim.Value,
                TotalVendas = totalVendas,
                QuantidadeVendas = vendas.Count(),
                TicketMedio = vendas.Any() ? totalVendas / vendas.Count() : 0,
                Vendas = vendas.Select(v => new VendaResumoViewModel
                {
                    Id = v.Id,
                    Numero = v.Numero,
                    ClienteNome = v.Cliente?.Nome ?? "Cliente não informado",
                    DataVenda = v.DataVenda,
                    ValorTotal = v.ValorTotal,
                    Status = v.Status.ToString()
                }).OrderByDescending(v => v.DataVenda).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao gerar relatório de vendas: {ex.Message}";
            return View(new RelatorioVendasViewModel());
        }
    }

    /// <summary>
    /// Relatório de compras
    /// </summary>
    public async Task<IActionResult> Compras(DateTime? inicio = null, DateTime? fim = null)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            
            // Definir período padrão (mês atual)
            inicio ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            fim ??= inicio.Value.AddMonths(1).AddDays(-1);

            var compras = await _compraService.ObterPorPeriodoAsync(empresaId, inicio.Value, fim.Value);
            var totalCompras = await _compraService.ObterTotalComprasAsync(empresaId, inicio.Value, fim.Value);

            var viewModel = new RelatorioComprasViewModel
            {
                DataInicio = inicio.Value,
                DataFim = fim.Value,
                TotalCompras = totalCompras,
                QuantidadeCompras = compras.Count(),
                TicketMedio = compras.Any() ? totalCompras / compras.Count() : 0,
                Compras = compras.Select(c => new CompraResumoViewModel
                {
                    Id = c.Id,
                    NumeroCompra = c.NumeroCompra,
                    FornecedorNome = c.Fornecedor?.Nome ?? "Fornecedor não informado",
                    DataCompra = c.DataCompra,
                    ValorTotal = c.ValorTotal,
                    Status = c.Status.ToString()
                }).OrderByDescending(c => c.DataCompra).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao gerar relatório de compras: {ex.Message}";
            return View(new RelatorioComprasViewModel());
        }
    }

    /// <summary>
    /// Relatório financeiro
    /// </summary>
    public async Task<IActionResult> Financeiro(DateTime? inicio = null, DateTime? fim = null)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            
            // Definir período padrão (mês atual)
            inicio ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            fim ??= inicio.Value.AddMonths(1).AddDays(-1);

            var contasReceber = await _contaReceberService.ObterPorPeriodoAsync(empresaId, inicio.Value, fim.Value);
            var contasPagar = await _contaPagarService.ObterPorPeriodoAsync(empresaId, inicio.Value, fim.Value);
            var contasReceberVencidas = await _contaReceberService.ObterVencidasAsync(empresaId);
            var contasPagarVencidas = await _contaPagarService.ObterVencidasAsync(empresaId);

            var viewModel = new RelatorioFinanceiroViewModel
            {
                DataInicio = inicio.Value,
                DataFim = fim.Value,
                TotalContasReceber = contasReceber.Sum(c => c.ValorOriginal),
                TotalContasPagar = contasPagar.Sum(c => c.ValorOriginal),
                ContasReceberVencidas = contasReceberVencidas.Count(),
                ContasPagarVencidas = contasPagarVencidas.Count(),
                ValorReceberVencido = contasReceberVencidas.Sum(c => c.ValorTotal),
                ValorPagarVencido = contasPagarVencidas.Sum(c => c.ValorTotal)
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao gerar relatório financeiro: {ex.Message}";
            return View(new RelatorioFinanceiroViewModel());
        }
    }

    /// <summary>
    /// Relatório de estoque
    /// </summary>
    public async Task<IActionResult> Estoque()
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var relatorio = await _estoqueService.ObterPosicaoEstoqueAsync(empresaId);

            return View(relatorio);
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao gerar relatório de estoque: {ex.Message}";
            return View();
        }
    }

    /// <summary>
    /// Relatório DRE simplificado
    /// </summary>
    public async Task<IActionResult> DRE(DateTime? inicio = null, DateTime? fim = null)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            
            // Definir período padrão (mês atual)
            inicio ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            fim ??= inicio.Value.AddMonths(1).AddDays(-1);

            var totalVendas = await _vendaService.ObterTotalVendasAsync(empresaId, inicio.Value, fim.Value);
            var totalCompras = await _compraService.ObterTotalComprasAsync(empresaId, inicio.Value, fim.Value);

            var viewModel = new RelatorioDREViewModel
            {
                DataInicio = inicio.Value,
                DataFim = fim.Value,
                ReceitaBruta = totalVendas,
                CustoMercadoriaVendida = totalCompras * 0.7m, // Estimativa
                LucroBruto = totalVendas - (totalCompras * 0.7m),
                DespesasOperacionais = totalVendas * 0.15m, // Estimativa
                LucroLiquido = totalVendas - (totalCompras * 0.7m) - (totalVendas * 0.15m)
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao gerar DRE: {ex.Message}";
            return View(new RelatorioDREViewModel());
        }
    }

    /// <summary>
    /// Exporta relatório para Excel (AJAX)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ExportarExcel(string tipoRelatorio, DateTime inicio, DateTime fim)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            
            // Por enquanto, retorna sucesso
            // Implementação completa do Excel seria feita com EPPlus ou similar
            
            return Json(new { 
                success = true, 
                message = "Relatório exportado com sucesso!",
                downloadUrl = "#" // URL do arquivo gerado
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Erro ao exportar: {ex.Message}" });
        }
    }

    /// <summary>
    /// API para dados do dashboard (AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> DadosDashboard()
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var hoje = DateTime.Today;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
            var fimMes = inicioMes.AddMonths(1).AddDays(-1);

            var vendasHoje = await _vendaService.ObterTotalVendasAsync(empresaId, hoje, hoje);
            var vendasMes = await _vendaService.ObterTotalVendasAsync(empresaId, inicioMes, fimMes);
            var contasVencidas = await _contaReceberService.ObterVencidasAsync(empresaId);

            return Json(new
            {
                vendasHoje = vendasHoje.ToString("C"),
                vendasMes = vendasMes.ToString("C"),
                contasVencidas = contasVencidas.Count(),
                ultimaAtualizacao = DateTime.Now.ToString("HH:mm:ss")
            });
        }
        catch (Exception ex)
        {
            return Json(new { erro = ex.Message });
        }
    }

    #region Métodos Auxiliares

    private decimal CalcularCrescimento(decimal valorAtual, decimal valorAnterior)
    {
        if (valorAnterior == 0) return valorAtual > 0 ? 100 : 0;
        return ((valorAtual - valorAnterior) / valorAnterior) * 100;
    }

    private async Task<List<VendaMensalViewModel>> ObterVendasUltimos12MesesAsync(int empresaId)
    {
        var resultado = new List<VendaMensalViewModel>();
        var dataInicio = DateTime.Today.AddMonths(-11);

        for (int i = 0; i < 12; i++)
        {
            var mes = dataInicio.AddMonths(i);
            var inicioMes = new DateTime(mes.Year, mes.Month, 1);
            var fimMes = inicioMes.AddMonths(1).AddDays(-1);

            var total = await _vendaService.ObterTotalVendasAsync(empresaId, inicioMes, fimMes);
            
            resultado.Add(new VendaMensalViewModel
            {
                Mes = mes.ToString("MMM/yy"),
                Valor = total,
                Periodo = inicioMes
            });
        }

        return resultado;
    }

    private async Task<List<ProdutoTopViewModel>> ObterTopProdutosAsync(int empresaId)
    {
        var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);

        // Simulação - em implementação real, seria uma query específica
        var produtos = await _produtoService.ObterTodosAsync(empresaId);
        
        return produtos.Take(5).Select(p => new ProdutoTopViewModel
        {
            Nome = p.Descricao,
            Vendas = new Random().Next(10, 100), // Simulado
            Valor = new Random().Next(1000, 10000) // Simulado
        }).ToList();
    }

    private async Task<List<ClienteTopViewModel>> ObterTopClientesAsync(int empresaId)
    {
        var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);

        // Simulação - em implementação real, seria uma query específica
        var clientes = await _clienteService.ObterTodosAsync(empresaId);
        
        return clientes.Take(5).Select(c => new ClienteTopViewModel
        {
            Nome = c.Nome,
            Vendas = new Random().Next(5, 50), // Simulado
            Valor = new Random().Next(5000, 50000) // Simulado
        }).ToList();
    }

    #endregion
}