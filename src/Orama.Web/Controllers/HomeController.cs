using Microsoft.AspNetCore.Mvc;
using Orama.Application.Services;
using Orama.Web.Models;
using System.Diagnostics;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller principal do sistema - Dashboard
/// </summary>
public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;
    private readonly IClienteService _clienteService;
    private readonly IFornecedorService _fornecedorService;
    private readonly IProdutoService _produtoService;
    private readonly IContaReceberService _contaReceberService;
    private readonly IContaPagarService _contaPagarService;
    private readonly IEstoqueService _estoqueService;
    private readonly IVendaService _vendaService;
    private readonly ICompraService _compraService;

    public HomeController(
        ILogger<HomeController> logger,
        IClienteService clienteService,
        IFornecedorService fornecedorService,
        IProdutoService produtoService,
        IContaReceberService contaReceberService,
        IContaPagarService contaPagarService,
        IEstoqueService estoqueService,
        IVendaService vendaService,
        ICompraService compraService)
    {
        _logger = logger;
        _clienteService = clienteService;
        _fornecedorService = fornecedorService;
        _produtoService = produtoService;
        _contaReceberService = contaReceberService;
        _contaPagarService = contaPagarService;
        _estoqueService = estoqueService;
        _vendaService = vendaService;
        _compraService = compraService;
    }

    /// <summary>
    /// Dashboard principal do sistema
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var fimMes = inicioMes.AddMonths(1).AddDays(-1);

            // Buscar dados reais
            var clientes = await _clienteService.ObterTodosAsync(empresaId);
            var fornecedores = await _fornecedorService.ObterTodosAsync(empresaId);
            var produtos = await _produtoService.ObterTodosAsync(empresaId);
            var contasReceberVencidas = await _contaReceberService.ObterVencidasAsync(empresaId);
            var contasPagarVencidas = await _contaPagarService.ObterVencidasAsync(empresaId);
            var produtosEstoqueBaixo = await _estoqueService.ObterProdutosEstoqueBaixoAsync(empresaId);
            var vendasMes = await _vendaService.ObterTotalVendasAsync(empresaId, inicioMes, fimMes);
            var comprasMes = await _compraService.ObterTotalComprasAsync(empresaId, inicioMes, fimMes);

            var model = new DashboardViewModel
            {
                NomeUsuario = UsuarioLogado?.Nome ?? "Usuário",
                PerfilUsuario = UsuarioLogado?.Perfil ?? "Sem perfil",
                UltimoLogin = UsuarioLogado?.UltimoLogin,
                TotalClientes = clientes.Count(),
                TotalFornecedores = fornecedores.Count(),
                TotalProdutos = produtos.Count(),
                ContasReceberVencidas = contasReceberVencidas.Count(),
                ContasPagarVencidas = contasPagarVencidas.Count(),
                EstoqueBaixo = produtosEstoqueBaixo.Count(),
                VendasMes = vendasMes,
                ComprasMes = comprasMes,
                ValorEstoque = await _estoqueService.CalcularValorTotalEstoqueAsync(empresaId)
            };

            ViewData["Title"] = "Dashboard";
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar dashboard");
            
            // Fallback com dados básicos
            var model = new DashboardViewModel
            {
                NomeUsuario = UsuarioLogado?.Nome ?? "Usuário",
                PerfilUsuario = UsuarioLogado?.Perfil ?? "Sem perfil",
                UltimoLogin = UsuarioLogado?.UltimoLogin
            };

            ViewData["Title"] = "Dashboard";
            return View(model);
        }
    }

    /// <summary>
    /// Obtém dados para gráficos do dashboard (AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObterDadosGraficos()
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var hoje = DateTime.Now;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
            
            // Dados dos últimos 12 meses
            var dadosVendas = new List<object>();
            var dadosCompras = new List<object>();
            
            for (int i = 11; i >= 0; i--)
            {
                var mes = inicioMes.AddMonths(-i);
                var fimMes = mes.AddMonths(1).AddDays(-1);
                
                var vendasMes = await _vendaService.ObterTotalVendasAsync(empresaId, mes, fimMes);
                var comprasMes = await _compraService.ObterTotalComprasAsync(empresaId, mes, fimMes);
                
                dadosVendas.Add(new { 
                    mes = mes.ToString("MMM/yyyy"), 
                    valor = vendasMes 
                });
                
                dadosCompras.Add(new { 
                    mes = mes.ToString("MMM/yyyy"), 
                    valor = comprasMes 
                });
            }

            // Top 5 produtos mais vendidos (últimos 30 dias)
            var inicio30Dias = hoje.AddDays(-30);
            var topProdutos = await _vendaService.ObterProdutosMaisVendidosAsync(empresaId, inicio30Dias, hoje, 5);

            return Json(new
            {
                success = true,
                data = new
                {
                    vendas = dadosVendas,
                    compras = dadosCompras,
                    topProdutos = topProdutos.Select(p => new
                    {
                        produto = p.Nome,
                        quantidade = p.QuantidadeVendida,
                        valor = p.ValorTotal
                    })
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter dados para gráficos");
            return Json(new { success = false, message = "Erro ao carregar dados dos gráficos" });
        }
    }

    /// <summary>
    /// Página de privacidade
    /// </summary>
    public IActionResult Privacy()
    {
        ViewData["Title"] = "Política de Privacidade";
        return View();
    }

    /// <summary>
    /// Página de erro
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}