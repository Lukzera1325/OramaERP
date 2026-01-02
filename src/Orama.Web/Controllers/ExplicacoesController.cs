using Microsoft.AspNetCore.Mvc;
using Orama.Application.Services;
using Orama.Domain.Entities;
using Orama.Web.Extensions;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller para Explicações de Resultado - Extremamente Simples
/// 
/// Responsabilidades:
/// - Receber requisições HTTP
/// - Chamar ExplicacaoResultadoService
/// - Retornar Views ou JSON
/// 
/// Padrão: Receber → Chamar Service → Retornar
/// </summary>
public class ExplicacoesController : Controller
{
    private readonly IExplicacaoResultadoService _explicacaoResultadoService;

    public ExplicacoesController(IExplicacaoResultadoService explicacaoResultadoService)
    {
        _explicacaoResultadoService = explicacaoResultadoService;
    }

    /// <summary>
    /// Lista explicações por tipo de resultado
    /// </summary>
    public async Task<IActionResult> Index(TipoResultado? tipo, DateTime? dataInicio, DateTime? dataFim)
    {
        var empresaId = HttpContext.ObterEmpresaId();
        
        // Definir período padrão (últimos 30 dias)
        dataInicio ??= DateTime.Now.AddDays(-30);
        dataFim ??= DateTime.Now;

        IEnumerable<ExplicacaoResultado> explicacoes;

        if (tipo.HasValue)
        {
            explicacoes = await _explicacaoResultadoService.ObterExplicacoesPorTipoAsync(
                tipo.Value, empresaId, dataInicio, dataFim);
        }
        else
        {
            // Buscar todas as explicações de prejuízo por padrão
            explicacoes = await _explicacaoResultadoService.ObterExplicacoesPorTipoAsync(
                TipoResultado.Prejuizo, empresaId, dataInicio, dataFim);
        }

        ViewBag.TipoSelecionado = tipo ?? TipoResultado.Prejuizo;
        ViewBag.DataInicio = dataInicio.Value.ToString("yyyy-MM-dd");
        ViewBag.DataFim = dataFim.Value.ToString("yyyy-MM-dd");

        return View(explicacoes);
    }

    /// <summary>
    /// Exibe detalhes de uma explicação específica
    /// </summary>
    public async Task<IActionResult> Detalhes(int id)
    {
        var empresaId = HttpContext.ObterEmpresaId();
        var explicacao = await _explicacaoResultadoService.ObterExplicacaoPorIdAsync(id, empresaId);
        
        if (explicacao == null)
            return NotFound();

        return View(explicacao);
    }

    /// <summary>
    /// Exibe explicações de uma venda específica
    /// </summary>
    public async Task<IActionResult> Venda(int vendaId)
    {
        var empresaId = HttpContext.ObterEmpresaId();
        var explicacoes = await _explicacaoResultadoService.ObterExplicacoesVendaAsync(vendaId, empresaId);
        
        if (!explicacoes.Any())
        {
            TempData["Info"] = "Nenhuma explicação encontrada para esta venda.";
            return RedirectToAction("Index");
        }

        ViewBag.VendaId = vendaId;
        return View(explicacoes);
    }

    /// <summary>
    /// Relatório de principais motivos de prejuízo
    /// </summary>
    public async Task<IActionResult> MotivosPrejuizo(DateTime? dataInicio, DateTime? dataFim)
    {
        var empresaId = HttpContext.ObterEmpresaId();
        
        // Definir período padrão (últimos 30 dias)
        dataInicio ??= DateTime.Now.AddDays(-30);
        dataFim ??= DateTime.Now;

        var motivos = await _explicacaoResultadoService.ObterPrincipaisMotivosPrejuizoAsync(
            dataInicio.Value, dataFim.Value, empresaId);

        ViewBag.DataInicio = dataInicio.Value.ToString("yyyy-MM-dd");
        ViewBag.DataFim = dataFim.Value.ToString("yyyy-MM-dd");

        return View(motivos);
    }

    /// <summary>
    /// Relatório de produtos que mais geram prejuízo
    /// </summary>
    public async Task<IActionResult> ProdutosPrejuizo(DateTime? dataInicio, DateTime? dataFim, int limite = 10)
    {
        var empresaId = HttpContext.ObterEmpresaId();
        
        // Definir período padrão (últimos 30 dias)
        dataInicio ??= DateTime.Now.AddDays(-30);
        dataFim ??= DateTime.Now;

        var produtos = await _explicacaoResultadoService.ObterProdutosMaisPrejuizoAsync(
            dataInicio.Value, dataFim.Value, empresaId, limite);

        ViewBag.DataInicio = dataInicio.Value.ToString("yyyy-MM-dd");
        ViewBag.DataFim = dataFim.Value.ToString("yyyy-MM-dd");
        ViewBag.Limite = limite;

        return View(produtos);
    }

    /// <summary>
    /// Dashboard com resumo de explicações
    /// </summary>
    public async Task<IActionResult> Dashboard(DateTime? dataInicio, DateTime? dataFim)
    {
        var empresaId = HttpContext.ObterEmpresaId();
        
        // Definir período padrão (últimos 30 dias)
        dataInicio ??= DateTime.Now.AddDays(-30);
        dataFim ??= DateTime.Now;

        var resumo = await _explicacaoResultadoService.ObterResumoExplicacoesAsync(
            dataInicio.Value, dataFim.Value, empresaId);

        ViewBag.DataInicio = dataInicio.Value.ToString("yyyy-MM-dd");
        ViewBag.DataFim = dataFim.Value.ToString("yyyy-MM-dd");

        return View(resumo);
    }

    /// <summary>
    /// API para obter explicações de uma venda (AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObterExplicacoesVenda(int vendaId)
    {
        var empresaId = HttpContext.ObterEmpresaId();
        var explicacoes = await _explicacaoResultadoService.ObterExplicacoesVendaAsync(vendaId, empresaId);
        
        var resultado = explicacoes.Select(e => new
        {
            id = e.Id,
            tipo = e.Tipo.ToString(),
            tipoDescricao = e.TipoDescricao,
            categoria = e.Categoria.ToString(),
            categoriaDescricao = e.CategoriaDescricao,
            resumo = e.ResumoExplicacao,
            textoCompleto = e.TextoExplicacao,
            icone = e.Icone,
            cssClass = e.CssClass,
            produtoId = e.ProdutoId,
            produtoDescricao = e.Produto?.Descricao,
            valorVenda = e.ValorVenda,
            custoTotal = e.CustoTotal,
            lucroCalculado = e.LucroCalculado,
            margemPercentual = e.MargemPercentual
        });

        return Json(resultado);
    }

    /// <summary>
    /// Força regeneração de explicações para uma venda (para testes)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RegenerarExplicacoes(int vendaId)
    {
        var empresaId = HttpContext.ObterEmpresaId();
        
        try
        {
            await _explicacaoResultadoService.GerarExplicacoesVendaAsync(vendaId, empresaId);
            TempData["Sucesso"] = "Explicações regeneradas com sucesso!";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao regenerar explicações: {ex.Message}";
        }

        return RedirectToAction("Venda", new { vendaId });
    }
}