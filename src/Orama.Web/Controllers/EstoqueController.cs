using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orama.Application.Services;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller extremamente simples para estoque
/// Recebe request -> Chama service -> Retorna response
/// </summary>
public class EstoqueController : BaseController
{
    private readonly IEstoqueService _estoqueService;
    private readonly IProdutoService _produtoService;

    public EstoqueController(IEstoqueService estoqueService, IProdutoService produtoService)
    {
        _estoqueService = estoqueService;
        _produtoService = produtoService;
    }

    // GET: Estoque (Histórico de movimentações)
    public async Task<IActionResult> Index()
    {
        var empresaId = ObterEmpresaId();
        var movimentacoes = await _estoqueService.ObterHistoricoAsync(empresaId);
        return View(movimentacoes);
    }

    // GET: Estoque/Posicao (Posição atual do estoque)
    public async Task<IActionResult> Posicao()
    {
        var empresaId = ObterEmpresaId();
        var produtos = await _estoqueService.ObterPosicaoEstoqueAsync(empresaId);
        return View(produtos);
    }

    // GET: Estoque/EstoqueBaixo (Produtos com estoque baixo)
    public async Task<IActionResult> EstoqueBaixo()
    {
        var empresaId = ObterEmpresaId();
        var produtos = await _estoqueService.ObterProdutosEstoqueBaixoAsync(empresaId);
        return View(produtos);
    }

    // GET: Estoque/Ajustar (Formulário para ajustar estoque)
    public async Task<IActionResult> Ajustar()
    {
        var empresaId = ObterEmpresaId();
        var produtos = await _produtoService.ObterTodosAsync(empresaId);
        
        ViewBag.Produtos = new SelectList(produtos, "Id", "CodigoFormatado");
        return View(new EstoqueViewModel());
    }

    // POST: Estoque/Ajustar (Processar ajuste de estoque)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ajustar(EstoqueViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var empresaId = ObterEmpresaId();
            var produtos = await _produtoService.ObterTodosAsync(empresaId);
            ViewBag.Produtos = new SelectList(produtos, "Id", "CodigoFormatado");
            return View(model);
        }

        try
        {
            var empresaId = ObterEmpresaId();
            var usuarioId = 1; // TODO: Obter do contexto

            var sucesso = await _estoqueService.AjustarEstoqueAsync(
                model.ProdutoId, 
                model.NovoEstoque, 
                model.Motivo, 
                empresaId, 
                usuarioId);

            if (sucesso)
            {
                TempData["Sucesso"] = "Estoque ajustado com sucesso!";
                return RedirectToAction(nameof(Posicao));
            }
            else
            {
                TempData["Erro"] = "Produto não encontrado.";
            }
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao ajustar estoque: {ex.Message}";
        }

        var empresaIdError = ObterEmpresaId();
        var produtosError = await _produtoService.ObterTodosAsync(empresaIdError);
        ViewBag.Produtos = new SelectList(produtosError, "Id", "CodigoFormatado");
        return View(model);
    }

    // GET: Estoque/Inventario (Inventário de estoque)
    public async Task<IActionResult> Inventario()
    {
        var empresaId = ObterEmpresaId();
        var produtos = await _estoqueService.ObterProdutosParaInventarioAsync(empresaId);
        return View(produtos);
    }

    // POST: Estoque/ProcessarInventario (Processar inventário)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessarInventario(Dictionary<int, decimal> contagem)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var usuarioId = 1; // TODO: Obter do contexto

            var sucesso = await _estoqueService.ProcessarInventarioAsync(contagem, empresaId, usuarioId);

            if (sucesso)
            {
                TempData["Sucesso"] = "Inventário processado com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Erro ao processar inventário.";
            }
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao processar inventário: {ex.Message}";
        }

        return RedirectToAction(nameof(Posicao));
    }

    // GET: Estoque/Historico (Histórico de um produto específico)
    public async Task<IActionResult> Historico(int produtoId)
    {
        var empresaId = ObterEmpresaId();
        var movimentacoes = await _estoqueService.ObterHistoricoAsync(empresaId, produtoId);
        return View(movimentacoes);
    }

    // AJAX: Entrada de estoque
    [HttpPost]
    public async Task<IActionResult> EntradaEstoque(int produtoId, decimal quantidade, string motivo)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var usuarioId = 1; // TODO: Obter do contexto

            var sucesso = await _estoqueService.EntradaEstoqueAsync(produtoId, quantidade, motivo, empresaId, usuarioId);

            return Json(new { success = sucesso, message = sucesso ? "Entrada registrada com sucesso!" : "Produto não encontrado." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // AJAX: Saída de estoque
    [HttpPost]
    public async Task<IActionResult> SaidaEstoque(int produtoId, decimal quantidade, string motivo)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var usuarioId = 1; // TODO: Obter do contexto

            var sucesso = await _estoqueService.SaidaEstoqueAsync(produtoId, quantidade, motivo, empresaId, usuarioId);

            return Json(new { success = sucesso, message = sucesso ? "Saída registrada com sucesso!" : "Produto não encontrado." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}