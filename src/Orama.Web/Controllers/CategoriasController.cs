using Microsoft.AspNetCore.Mvc;
using Orama.Application.Services;
using Orama.Domain.Entities;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller para gerenciamento de categorias de produtos
/// </summary>
public class CategoriasController : BaseController
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    /// <summary>
    /// Lista todas as categorias
    /// </summary>
    public async Task<IActionResult> Index()
    {
        if (!TemPermissao("Produtos.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var categorias = await _categoriaService.ObterTodosAsync();
        return View(categorias);
    }

    /// <summary>
    /// Exibe detalhes de uma categoria
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        if (!TemPermissao("Produtos.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var categoria = await _categoriaService.ObterPorIdAsync(id);
        if (categoria == null)
            return NotFound();

        return View(categoria);
    }

    /// <summary>
    /// Exibe formulário para criar nova categoria
    /// </summary>
    public IActionResult Create()
    {
        if (!TemPermissao("Produtos.Incluir"))
            return RedirectToAction("AccessDenied", "Auth");

        return View(new CategoriaViewModel());
    }

    /// <summary>
    /// Processa criação de nova categoria
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoriaViewModel model)
    {
        if (!TemPermissao("Produtos.Incluir"))
            return RedirectToAction("AccessDenied", "Auth");

        if (!ModelState.IsValid)
            return View(model);

        var categoria = new Categoria
        {
            Nome = model.Nome,
            Descricao = model.Descricao,
            Ativo = true
        };

        await _categoriaService.CriarAsync(categoria);

        TempData["Sucesso"] = "Categoria cadastrada com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Exibe formulário para editar categoria
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        if (!TemPermissao("Produtos.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        var categoria = await _categoriaService.ObterPorIdAsync(id);
        if (categoria == null)
            return NotFound();

        var viewModel = new CategoriaViewModel
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Descricao = categoria.Descricao
        };

        return View(viewModel);
    }

    /// <summary>
    /// Processa edição de categoria
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoriaViewModel model)
    {
        if (!TemPermissao("Produtos.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        if (id != model.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        var categoria = new Categoria
        {
            Id = model.Id,
            Nome = model.Nome,
            Descricao = model.Descricao
        };

        await _categoriaService.AtualizarAsync(categoria);

        TempData["Sucesso"] = "Categoria atualizada com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Exibe confirmação para excluir categoria
    /// </summary>
    public async Task<IActionResult> Delete(int id)
    {
        if (!TemPermissao("Produtos.Excluir"))
            return RedirectToAction("AccessDenied", "Auth");

        var categoria = await _categoriaService.ObterPorIdAsync(id);
        if (categoria == null)
            return NotFound();

        return View(categoria);
    }

    /// <summary>
    /// Processa exclusão de categoria
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!TemPermissao("Produtos.Excluir"))
            return RedirectToAction("AccessDenied", "Auth");

        await _categoriaService.ExcluirAsync(id);

        TempData["Sucesso"] = "Categoria excluída com sucesso!";
        return RedirectToAction(nameof(Index));
    }
}