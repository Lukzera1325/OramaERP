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

    // POST: Producao/Liberar/5 (Liberar ordem)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Liberar(int id)
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

        return RedirectToAction(nameof(Detalhes), new { id });
    }

    // POST: Producao/Iniciar/5 (Iniciar ordem)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Iniciar(int id)
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

        return RedirectToAction(nameof(Detalhes), new { id });
    }

    // POST: Producao/Finalizar/5 (Finalizar ordem)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalizar(int id, decimal quantidadeProduzida)
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

        return RedirectToAction(nameof(Detalhes), new { id });
    }

    // POST: Producao/Cancelar/5 (Cancelar ordem)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id, string motivo)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var usuarioId = 1; // TODO: Obter do contexto

            var sucesso = await _ordemProducaoService.CancelarAsync(id, motivo, empresaId, usuarioId);

            if (sucesso)
                TempData["Sucesso"] = "Ordem cancelada com sucesso!";
            else
                TempData["Erro"] = "Ordem não encontrada.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao cancelar ordem: {ex.Message}";
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
}