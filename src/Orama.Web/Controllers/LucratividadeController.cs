using Microsoft.AspNetCore.Mvc;
using Orama.Application.Services;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller SIMPLIFICADO para Relatórios de Lucratividade
/// Recebe request -> Chama service -> Retorna response
/// </summary>
public class LucratividadeController : BaseController
{
    private readonly IVendaService _vendaService;

    public LucratividadeController(IVendaService vendaService)
    {
        _vendaService = vendaService;
    }

    /// <summary>
    /// Relatório principal de lucratividade
    /// Mostra vendas com margem calculada por período
    /// </summary>
    public async Task<IActionResult> Index(DateTime? dataInicio, DateTime? dataFim)
    {
        var empresaId = ObterEmpresaId();
        
        // Padrão: últimos 30 dias
        dataInicio ??= DateTime.Now.AddDays(-30);
        dataFim ??= DateTime.Now;

        // Buscar dados
        var vendas = await _vendaService.ObterRelatorioLucratividadeAsync(
            dataInicio.Value, 
            dataFim.Value, 
            empresaId);

        var resumo = await _vendaService.ObterResumoLucratividadeAsync(
            dataInicio.Value, 
            dataFim.Value, 
            empresaId);

        // Preparar ViewBag
        ViewBag.DataInicio = dataInicio.Value.ToString("yyyy-MM-dd");
        ViewBag.DataFim = dataFim.Value.ToString("yyyy-MM-dd");
        ViewBag.Resumo = resumo;

        return View(vendas);
    }

    /// <summary>
    /// Relatório de vendas mais lucrativas
    /// Top 20 vendas com maior lucro
    /// </summary>
    public async Task<IActionResult> MaisLucrativas()
    {
        var empresaId = ObterEmpresaId();
        var vendas = await _vendaService.ObterVendasMaisLucrativasAsync(empresaId, 20);
        
        return View(vendas);
    }

    /// <summary>
    /// Relatório de vendas com prejuízo
    /// Vendas abaixo do custo
    /// </summary>
    public async Task<IActionResult> ComPrejuizo()
    {
        var empresaId = ObterEmpresaId();
        var vendas = await _vendaService.ObterVendasComPrejuizoAsync(empresaId);
        
        return View(vendas);
    }

    /// <summary>
    /// API para obter dados de lucratividade (AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObterDadosLucratividade(DateTime dataInicio, DateTime dataFim)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var resumo = await _vendaService.ObterResumoLucratividadeAsync(dataInicio, dataFim, empresaId);

            return Json(new { success = true, data = resumo });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}