using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orama.Application.Services;
using Orama.Domain.Entities;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller extremamente simples para Produção Industrial
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

    // GET: Producao (Dashboard de produção)
    public async Task<IActionResult> Index()
    {
        var empresaId = ObterEmpresaId();
        
        var ordensEmProducao = await _ordemProducaoService.ObterPorStatusAsync(StatusOrdemProducao.EmAndamento, empresaId);
        var ordensAtrasadas = await _ordemProducaoService.ObterOrdensAtrasadasAsync(empresaId);
        
        ViewBag.OrdensEmProducao = ordensEmProducao.Count();
        ViewBag.OrdensAtrasadas = ordensAtrasadas.Count();
        
        return View(ordensEmProducao);
    }

    // GET: Producao/OrdensProducao (Lista de ordens)
    public async Task<IActionResult> OrdensProducao()
    {
        var empresaId = ObterEmpresaId();
        var ordens = await _ordemProducaoService.ObterTodosAsync(empresaId);
        return View(ordens);
    }

    // GET: Producao/CriarOrdemProducao (Formulário para criar ordem)
    public async Task<IActionResult> CriarOrdemProducao()
    {
        var empresaId = ObterEmpresaId();
        var produtos = await _produtoService.ObterTodosAsync(empresaId);
        var produtosProduzíveis = produtos.Where(p => p.EhProduzivel());
        
        ViewBag.Produtos = new SelectList(produtosProduzíveis, "Id", "CodigoFormatado");
        return View(new OrdemProducaoViewModel());
    }

    // POST: Producao/CriarOrdemProducao (Processar criação)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CriarOrdemProducao(OrdemProducaoViewModel model)
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
            return RedirectToAction(nameof(DetalhesOrdemProducao), new { id = ordem.Id });
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

    // GET: Producao/DetalhesOrdemProducao/5 (Detalhes da ordem)
    public async Task<IActionResult> DetalhesOrdemProducao(int id)
    {
        var empresaId = ObterEmpresaId();
        var ordem = await _ordemProducaoService.ObterPorIdAsync(id, empresaId);
        
        if (ordem == null)
            return NotFound();

        return View(ordem);
    }

    // POST: Producao/LiberarProducao/5 (Liberar ordem)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LiberarProducao(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var usuarioId = 1; // TODO: Obter do contexto

            var sucesso = await _ordemProducaoService.LiberarAsync(id, empresaId, usuarioId);

            if (sucesso)
                TempData["Sucesso"] = "Ordem liberada para produção!";
            else
                TempData["Erro"] = "Ordem não encontrada.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao liberar ordem: {ex.Message}";
        }

        return RedirectToAction(nameof(DetalhesOrdemProducao), new { id });
    }

    // POST: Producao/IniciarProducao/5 (Iniciar ordem)
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
                TempData["Erro"] = "Ordem não encontrada.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao iniciar produção: {ex.Message}";
        }

        return RedirectToAction(nameof(DetalhesOrdemProducao), new { id });
    }

    // POST: Producao/FinalizarProducao/5 (Finalizar ordem)
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
                TempData["Erro"] = "Ordem não encontrada.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao finalizar produção: {ex.Message}";
        }

        return RedirectToAction(nameof(DetalhesOrdemProducao), new { id });
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
                id = e.Id,
                componenteId = e.ProdutoComponenteId,
                componenteNome = e.ProdutoComponente.Descricao,
                componenteCodigo = e.ProdutoComponente.Codigo,
                quantidade = e.QuantidadeNecessaria,
                unidade = e.Unidade,
                estoqueDisponivel = e.ProdutoComponente.EstoqueAtual
            });

            return Json(new { success = true, data = resultado });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}