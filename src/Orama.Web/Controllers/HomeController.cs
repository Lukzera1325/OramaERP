using Microsoft.AspNetCore.Mvc;
using Orama.Web.Models;
using System.Diagnostics;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller principal do sistema - Dashboard e páginas iniciais
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Página inicial - Dashboard do sistema
    /// </summary>
    public IActionResult Index()
    {
        // TODO: Verificar se usuário está autenticado
        // Por enquanto, vamos simular um usuário logado
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
        {
            // Simular login para desenvolvimento
            HttpContext.Session.SetString("UsuarioId", "1");
            HttpContext.Session.SetString("UsuarioNome", "Administrador");
            HttpContext.Session.SetString("UsuarioEmail", "admin@orama.com.br");
        }

        // Dados do dashboard (por enquanto fictícios)
        var dashboardData = new DashboardViewModel
        {
            TotalClientes = 150,
            TotalFornecedores = 45,
            TotalProdutos = 320,
            ContasReceberVencidas = 12,
            ContasPagarVencidas = 8,
            EstoqueBaixo = 25,
            VendasMes = 85000.00m,
            ComprasMes = 45000.00m
        };

        ViewData["Title"] = "Dashboard";
        return View(dashboardData);
    }

    /// <summary>
    /// Página de privacidade
    /// </summary>
    public IActionResult Privacy()
    {
        ViewData["Title"] = "Política de Privacidade";
        ViewData["Breadcrumb"] = "<li class=\"breadcrumb-item active\">Privacidade</li>";
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