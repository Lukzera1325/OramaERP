using Microsoft.AspNetCore.Mvc;
using Orama.Application.Services;
using Orama.Domain.Entities;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller para Alertas de Margem - Extremamente Simples
/// 
/// Responsabilidades:
/// - Receber requisições HTTP
/// - Chamar AlertaMargemService
/// - Retornar Views ou JSON
/// 
/// Padrão: Receber → Chamar Service → Retornar
/// </summary>
public class AlertasController : BaseController
{
    private readonly IAlertaMargemService _alertaMargemService;

    public AlertasController(IAlertaMargemService alertaMargemService)
    {
        _alertaMargemService = alertaMargemService;
    }

    /// <summary>
    /// Lista todos os alertas ativos
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var empresaId = ObterEmpresaId();
        var alertas = await _alertaMargemService.ObterAlertasAtivosAsync(empresaId);
        return View(alertas);
    }

    /// <summary>
    /// Exibe detalhes de um alerta específico
    /// </summary>
    public async Task<IActionResult> Detalhes(int id)
    {
        var empresaId = ObterEmpresaId();
        var alerta = await _alertaMargemService.ObterAlertaPorIdAsync(id, empresaId);
        
        if (alerta == null)
            return NotFound();

        return View(alerta);
    }

    /// <summary>
    /// Resolve um alerta manualmente
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Resolver(int id, string? observacoes)
    {
        var empresaId = ObterEmpresaId();
        var usuarioId = UsuarioId;
        
        var sucesso = await _alertaMargemService.ResolverAlertaAsync(id, usuarioId, empresaId, observacoes);
        
        if (sucesso)
        {
            TempData["Sucesso"] = "Alerta resolvido com sucesso!";
        }
        else
        {
            TempData["Erro"] = "Erro ao resolver alerta.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Reativa um alerta
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Reativar(int id)
    {
        var empresaId = ObterEmpresaId();
        
        var sucesso = await _alertaMargemService.ReativarAlertaAsync(id, empresaId);
        
        if (sucesso)
        {
            TempData["Sucesso"] = "Alerta reativado com sucesso!";
        }
        else
        {
            TempData["Erro"] = "Erro ao reativar alerta.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Resolve múltiplos alertas em lote
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ResolverLote(int[] alertaIds, string? observacoes)
    {
        if (alertaIds == null || !alertaIds.Any())
        {
            TempData["Erro"] = "Nenhum alerta selecionado.";
            return RedirectToAction(nameof(Index));
        }

        var empresaId = ObterEmpresaId();
        var usuarioId = UsuarioId;
        
        var quantidade = await _alertaMargemService.ResolverAlertasEmLoteAsync(alertaIds, usuarioId, empresaId, observacoes);
        
        TempData["Sucesso"] = $"{quantidade} alerta(s) resolvido(s) com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Relatório de alertas por período
    /// </summary>
    public async Task<IActionResult> Relatorio(DateTime? dataInicio, DateTime? dataFim, StatusAlerta? status)
    {
        var empresaId = ObterEmpresaId();
        
        // Definir período padrão (últimos 30 dias)
        dataInicio ??= DateTime.Now.AddDays(-30);
        dataFim ??= DateTime.Now;

        var alertas = await _alertaMargemService.ObterAlertasPorPeriodoAsync(
            dataInicio.Value, dataFim.Value, empresaId, status);

        ViewBag.DataInicio = dataInicio.Value.ToString("yyyy-MM-dd");
        ViewBag.DataFim = dataFim.Value.ToString("yyyy-MM-dd");
        ViewBag.Status = status;

        return View(alertas);
    }

    /// <summary>
    /// Estatísticas de alertas (JSON para dashboard)
    /// </summary>
    public async Task<IActionResult> Estatisticas(DateTime? dataInicio, DateTime? dataFim)
    {
        var empresaId = ObterEmpresaId();
        
        // Definir período padrão (últimos 30 dias)
        dataInicio ??= DateTime.Now.AddDays(-30);
        dataFim ??= DateTime.Now;

        var estatisticas = await _alertaMargemService.ObterEstatisticasAlertasAsync(
            dataInicio.Value, dataFim.Value, empresaId);

        return Json(estatisticas);
    }

    /// <summary>
    /// Conta alertas ativos (para badge no menu)
    /// </summary>
    public async Task<IActionResult> ContarAtivos()
    {
        var empresaId = ObterEmpresaId();
        var quantidade = await _alertaMargemService.ContarAlertasAtivosAsync(empresaId);
        return Json(new { quantidade });
    }
}