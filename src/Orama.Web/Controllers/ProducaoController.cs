using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orama.Application.Services;
using Orama.Domain.Entities;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller SIMPLIFICADO para Produção Industrial
/// Recebe request -> Chama service -> Retorna response
/// </summary>
public class ProducaoController : BaseController
{
    private readonly IOrdemProducaoService _ordemProducaoService;
    private readonly IEstruturaProdutoService _estruturaService;
    private readonly IProdutoService _produtoService;

    public ProducaoController(
        IOrdemProducaoService ordemProducaoService,
        IEstruturaProdutoService estruturaService,
        IProdutoService produtoService)
    {
        _ordemProducaoService = ordemProducaoService;
        _estruturaService = estruturaService;
        _produtoService = produtoService;
    }

    // GET: Producao (Lista de ordens)
    public async Task<IActionResult> Index()
    {
        var empresaId = ObterEmpresaId();
        var ordens = await _ordemProducaoService.ObterTodosAsync(empresaId);
        return View(ordens);
    }

    // GET: Producao/Criar (Formulário para criar ordem)
    public async Task<IActionResult> Criar()
    {
        var empresaId = ObterEmpresaId();
        var produtos = await _produtoService.ObterTodosAsync(empresaId);
        var produtosProduzíveis = produtos.Where(p => p.EhProduzivel());
        
        ViewBag.Produtos = new SelectList(produtosProduzíveis, "Id", "CodigoFormatado");
        return View(new OrdemProducaoViewModel());
    }

    // POST: Producao/Criar (Processar criação)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(OrdemProducaoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var empresaId = ObterEmpresaId();
            var produtos = await _produtoService.ObterTodosAsync(empresaId);
            var produtosProduzíveis = produtos.Where(p => p.EhProduzivel());
            ViewBag.Produtos = new SelectList(produtosProduzíveis, "Id", "CodigoFormatado");
            return View(model);
        }

        try
        {
            var empresaId = ObterEmpresaId();
            var usuarioId = 1; // TODO: Obter do contexto

            var ordem = await _ordemProducaoService.CriarAsync(
                model.ProdutoId, 
                model.QuantidadePlanejada, 
                model.Observacoes, 
                empresaId, 
                usuarioId);

            TempData["Sucesso"] = $"Ordem de Produção {ordem.Numero} criada com sucesso!";
            return RedirectToAction(nameof(Detalhes), new { id = ordem.Id });
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao criar ordem: {ex.Message}";
            
            var empresaId = ObterEmpresaId();
            var produtos = await _produtoService.ObterTodosAsync(empresaId);
            var produtosProduzíveis = produtos.Where(p => p.EhProduzivel());
            ViewBag.Produtos = new SelectList(produtosProduzíveis, "Id", "CodigoFormatado");
            return View(model);
        }
    }

    // GET: Producao/Detalhes/5 (Detalhes da ordem)
    public async Task<IActionResult> Detalhes(int id)
    {
        var empresaId = ObterEmpresaId();
        var ordem = await _ordemProducaoService.ObterPorIdAsync(id, empresaId);
        
        if (ordem == null)
            return NotFound();

        return View(ordem);
    }

    /// <summary>
    /// Libera ordem para produção
    /// Ação: Planejada → Liberada
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LiberarParaProducao(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var usuarioId = 1; // TODO: Obter do contexto

            var sucesso = await _ordemProducaoService.LiberarAsync(id, empresaId, usuarioId);

            if (sucesso)
                TempData["Sucesso"] = "Ordem liberada para produção com sucesso!";
            else
                TempData["Erro"] = "Ordem não encontrada ou não pode ser liberada.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao liberar ordem para produção: {ex.Message}";
        }

        return RedirectToAction(nameof(Detalhes), new { id });
    }

    /// <summary>
    /// Inicia a produção
    /// Ação: Liberada → Em Andamento
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarProducao(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var usuarioId = 1; // TODO: Obter do contexto

            var sucesso = await _ordemProducaoService.IniciarAsync(id, empresaId, usuarioId);

            if (sucesso)
                TempData["Sucesso"] = "Produção iniciada com sucesso!";
            else
                TempData["Erro"] = "Ordem não encontrada ou não pode ser iniciada.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao iniciar produção: {ex.Message}";
        }

        return RedirectToAction(nameof(Detalhes), new { id });
    }

    /// <summary>
    /// Finaliza a produção e atualiza estoque automaticamente
    /// Ação: Em Andamento → Finalizada
    /// Efeitos: Baixa componentes + Entrada produto acabado
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinalizarProducao(int id, decimal quantidadeProduzida)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var usuarioId = 1; // TODO: Obter do contexto

            var sucesso = await _ordemProducaoService.FinalizarAsync(id, quantidadeProduzida, empresaId, usuarioId);

            if (sucesso)
                TempData["Sucesso"] = "Produção finalizada com sucesso! Estoque atualizado automaticamente.";
            else
                TempData["Erro"] = "Ordem não encontrada ou não pode ser finalizada.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao finalizar produção: {ex.Message}";
        }

        return RedirectToAction(nameof(Detalhes), new { id });
    }

    /// <summary>
    /// Cancela a ordem de produção
    /// Ação: Qualquer Status (exceto Finalizada) → Cancelada
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelarOrdemProducao(int id, string motivo)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var usuarioId = 1; // TODO: Obter do contexto

            var sucesso = await _ordemProducaoService.CancelarAsync(id, motivo, empresaId, usuarioId);

            if (sucesso)
                TempData["Sucesso"] = "Ordem de produção cancelada com sucesso!";
            else
                TempData["Erro"] = "Ordem não encontrada ou não pode ser cancelada.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao cancelar ordem de produção: {ex.Message}";
        }

        return RedirectToAction(nameof(Detalhes), new { id });
    }

    // AJAX: Obter estrutura de um produto
    [HttpGet]
    public async Task<IActionResult> ObterEstruturaProduto(int produtoId)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var estrutura = await _estruturaService.ObterEstruturaPorProdutoAsync(produtoId, empresaId);

            var resultado = estrutura.Select(e => new
            {
                componenteNome = e.ProdutoComponente.Descricao,
                componenteCodigo = e.ProdutoComponente.Codigo,
                quantidade = e.QuantidadeNecessaria,
                estoqueDisponivel = e.ProdutoComponente.EstoqueAtual
            });

            return Json(new { success = true, data = resultado });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // GET: Producao/RelatorioCustom (Relatório de custos)
    public async Task<IActionResult> RelatorioCustom(DateTime? dataInicio, DateTime? dataFim)
    {
        var empresaId = ObterEmpresaId();
        
        // Padrão: últimos 30 dias
        dataInicio ??= DateTime.Now.AddDays(-30);
        dataFim ??= DateTime.Now;

        var ordens = await _ordemProducaoService.ObterRelatorioCustomPorPeriodoAsync(
            dataInicio.Value, 
            dataFim.Value, 
            empresaId);

        ViewBag.DataInicio = dataInicio.Value.ToString("yyyy-MM-dd");
        ViewBag.DataFim = dataFim.Value.ToString("yyyy-MM-dd");
        ViewBag.TotalOrdens = ordens.Count();
        ViewBag.CustoTotalPeriodo = ordens.Sum(o => o.CustoTotalProducao);
        ViewBag.QuantidadeTotalProduzida = ordens.Sum(o => o.QuantidadeProduzida);

        return View(ordens);
    }
}