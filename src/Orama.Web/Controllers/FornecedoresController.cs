using Microsoft.AspNetCore.Mvc;
using Orama.Application.Services;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller para gerenciamento de fornecedores
/// </summary>
public class FornecedoresController : BaseController
{
    private readonly IFornecedorService _fornecedorService;

    public FornecedoresController(IFornecedorService fornecedorService)
    {
        _fornecedorService = fornecedorService;
    }

    /// <summary>
    /// Lista todos os fornecedores
    /// </summary>
    public async Task<IActionResult> Index()
    {
        if (!TemPermissao("Fornecedores.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var fornecedores = await _fornecedorService.ObterTodosAsync(empresaId);
        var viewModels = fornecedores.Select(FornecedorViewModel.FromEntity).ToList();

        return View(viewModels);
    }

    /// <summary>
    /// Exibe detalhes de um fornecedor
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        if (!TemPermissao("Fornecedores.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var fornecedor = await _fornecedorService.ObterPorIdAsync(id, empresaId);
        if (fornecedor == null)
            return NotFound();

        var viewModel = FornecedorViewModel.FromEntity(fornecedor);
        return View(viewModel);
    }

    /// <summary>
    /// Exibe formulário para criar novo fornecedor
    /// </summary>
    public IActionResult Create()
    {
        if (!TemPermissao("Fornecedores.Incluir"))
            return RedirectToAction("AccessDenied", "Auth");

        return View(new FornecedorViewModel());
    }

    /// <summary>
    /// Processa criação de novo fornecedor
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FornecedorViewModel model)
    {
        if (!TemPermissao("Fornecedores.Incluir"))
            return RedirectToAction("AccessDenied", "Auth");

        if (!ModelState.IsValid)
            return View(model);

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;

        // Verificar se CPF/CNPJ já existe
        if (await _fornecedorService.CnpjJaCadastradoAsync(model.CpfCnpj, empresaId))
        {
            ModelState.AddModelError("CpfCnpj", "Este CPF/CNPJ já está cadastrado.");
            return View(model);
        }

        var fornecedor = model.ToEntity(empresaId);
        await _fornecedorService.IncluirAsync(fornecedor);

        TempData["Sucesso"] = "Fornecedor cadastrado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Exibe formulário para editar fornecedor
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        if (!TemPermissao("Fornecedores.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var fornecedor = await _fornecedorService.ObterPorIdAsync(id, empresaId);
        if (fornecedor == null)
            return NotFound();

        var viewModel = FornecedorViewModel.FromEntity(fornecedor);
        return View(viewModel);
    }

    /// <summary>
    /// Processa edição de fornecedor
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FornecedorViewModel model)
    {
        if (!TemPermissao("Fornecedores.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        if (id != model.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;

        // Verificar se CPF/CNPJ já existe (exceto para o próprio fornecedor)
        if (await _fornecedorService.CnpjJaCadastradoAsync(model.CpfCnpj, empresaId, model.Id))
        {
            ModelState.AddModelError("CpfCnpj", "Este CPF/CNPJ já está cadastrado.");
            return View(model);
        }

        var fornecedor = model.ToEntity(empresaId);
        await _fornecedorService.AlterarAsync(fornecedor);

        TempData["Sucesso"] = "Fornecedor atualizado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Exibe confirmação para excluir fornecedor
    /// </summary>
    public async Task<IActionResult> Delete(int id)
    {
        if (!TemPermissao("Fornecedores.Excluir"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var fornecedor = await _fornecedorService.ObterPorIdAsync(id, empresaId);
        if (fornecedor == null)
            return NotFound();

        var viewModel = FornecedorViewModel.FromEntity(fornecedor);
        return View(viewModel);
    }

    /// <summary>
    /// Processa exclusão de fornecedor
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!TemPermissao("Fornecedores.Excluir"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var sucesso = await _fornecedorService.ExcluirAsync(id, empresaId);
        if (!sucesso)
            return NotFound();

        TempData["Sucesso"] = "Fornecedor excluído com sucesso!";
        return RedirectToAction(nameof(Index));
    }
}