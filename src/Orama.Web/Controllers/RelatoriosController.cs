using Microsoft.AspNetCore.Mvc;
using Orama.Application.Services;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller SIMPLIFICADO para Relatórios Gerenciais
/// Recebe request -> Chama service -> Retorna response
/// </summary>
public class RelatoriosController : BaseController
{
    private readonly IRelatorioLucratividadeService _relatorioService;

    public RelatoriosController(IRelatorioLucratividadeService relatorioService)
    {
        _relatorioService = relatorioService;
    }

    /// <summary>
    /// Página principal de relatórios
    /// Menu de navegação para os relatórios disponíveis
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Relatório 1: Produtos Mais Lucrativos
    /// Mostra quais produtos geram mais lucro
    /// </summary>
    public async Task<IActionResult> ProdutosMaisLucrativos(DateTime? dataInicio, DateTime? dataFim, int limite = 20)
    {
        var empresaId = ObterEmpresaId();
        
        // Padrão: últimos 90 dias
        dataInicio ??= DateTime.Now.AddDays(-90);
        dataFim ??= DateTime.Now;

        var produtos = await _relatorioService.ObterProdutosMaisLucrativosAsync(
            dataInicio.Value, 
            dataFim.Value, 
            empresaId, 
            limite);

        var resumo = await _relatorioService.ObterResumoLucratividadeAsync(
            dataInicio.Value, 
            dataFim.Value, 
            empresaId);

        ViewBag.DataInicio = dataInicio.Value.ToString("yyyy-MM-dd");
        ViewBag.DataFim = dataFim.Value.ToString("yyyy-MM-dd");
        ViewBag.Limite = limite;
        ViewBag.Resumo = resumo;

        return View(produtos);
    }

    /// <summary>
    /// Relatório 2: Vendas com Margem Negativa
    /// Identifica vendas com prejuízo
    /// </summary>
    public async Task<IActionResult> VendasMargemNegativa(DateTime? dataInicio, DateTime? dataFim)
    {
        var empresaId = ObterEmpresaId();
        
        // Padrão: últimos 30 dias
        dataInicio ??= DateTime.Now.AddDays(-30);
        dataFim ??= DateTime.Now;

        var vendas = await _relatorioService.ObterVendasMargemNegativaAsync(
            dataInicio.Value, 
            dataFim.Value, 
            empresaId);

        ViewBag.DataInicio = dataInicio.Value.ToString("yyyy-MM-dd");
        ViewBag.DataFim = dataFim.Value.ToString("yyyy-MM-dd");
        ViewBag.TotalPrejuizo = vendas.Sum(v => v.LucroNegativo);
        ViewBag.QuantidadeVendas = vendas.Count();

        return View(vendas);
    }

    /// <summary>
    /// Relatório 3: Evolução de Margem no Tempo
    /// Mostra tendência de melhoria ou piora da margem
    /// </summary>
    public async Task<IActionResult> EvolucaoMargem(DateTime? dataInicio, DateTime? dataFim, string periodo = "mensal")
    {
        var empresaId = ObterEmpresaId();
        
        // Padrão: últimos 12 meses para mensal, últimos 30 dias para diário
        if (periodo == "diario")
        {
            dataInicio ??= DateTime.Now.AddDays(-30);
            dataFim ??= DateTime.Now;
        }
        else
        {
            dataInicio ??= DateTime.Now.AddMonths(-12);
            dataFim ??= DateTime.Now;
        }

        IEnumerable<EvolucaoMargem> evolucao;
        
        if (periodo == "diario")
        {
            evolucao = await _relatorioService.ObterEvolucaoMargemDiariaAsync(
                dataInicio.Value, 
                dataFim.Value, 
                empresaId);
        }
        else
        {
            evolucao = await _relatorioService.ObterEvolucaoMargemMensalAsync(
                dataInicio.Value, 
                dataFim.Value, 
                empresaId);
        }

        var resumo = await _relatorioService.ObterResumoLucratividadeAsync(
            dataInicio.Value, 
            dataFim.Value, 
            empresaId);

        ViewBag.DataInicio = dataInicio.Value.ToString("yyyy-MM-dd");
        ViewBag.DataFim = dataFim.Value.ToString("yyyy-MM-dd");
        ViewBag.Periodo = periodo;
        ViewBag.Resumo = resumo;

        return View(evolucao);
    }

    /// <summary>
    /// API para obter dados de evolução (AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObterDadosEvolucao(DateTime dataInicio, DateTime dataFim, string periodo = "mensal")
    {
        try
        {
            var empresaId = ObterEmpresaId();
            
            IEnumerable<EvolucaoMargem> dados;
            
            if (periodo == "diario")
            {
                dados = await _relatorioService.ObterEvolucaoMargemDiariaAsync(dataInicio, dataFim, empresaId);
            }
            else
            {
                dados = await _relatorioService.ObterEvolucaoMargemMensalAsync(dataInicio, dataFim, empresaId);
            }

            return Json(new { success = true, data = dados });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}