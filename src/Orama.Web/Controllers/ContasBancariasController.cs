using Microsoft.AspNetCore.Mvc;
using Orama.Application.Services;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

public class ContasBancariasController : BaseController
{
    private readonly IContaBancariaService _contaBancariaService;

    public ContasBancariasController(IContaBancariaService contaBancariaService)
    {
        _contaBancariaService = contaBancariaService;
    }

    public async Task<IActionResult> Index()
    {
        if (!TemPermissao("Financeiro.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var contas = await _contaBancariaService.ObterTodosAsync(empresaId);
        var viewModels = contas.Select(ContaBancariaViewModel.FromEntity).ToList();
        
        ViewBag.SaldoTotal = await _contaBancariaService.ObterSaldoTotalAsync(empresaId);
        return View(viewModels);
    }

    public async Task<IActionResult> Details(int id)
    {
        if (!TemPermissao("Financeiro.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var conta = await _contaBancariaService.ObterPorIdAsync(id, empresaId);
        if (conta == null) return NotFound();

        return View(ContaBancariaViewModel.FromEntity(conta));
    }

    public IActionResult Create()
    {
        if (!TemPermissao("Financeiro.Incluir"))
            return RedirectToAction("AccessDenied", "Auth");

        return View(new ContaBancariaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContaBancariaViewModel model)
    {
        if (!TemPermissao("Financeiro.Incluir"))
            return RedirectToAction("AccessDenied", "Auth");

        if (!ModelState.IsValid) return View(model);

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var conta = model.ToEntity(empresaId);
        await _contaBancariaService.IncluirAsync(conta);

        TempData["Sucesso"] = "Conta bancária cadastrada com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!TemPermissao("Financeiro.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var conta = await _contaBancariaService.ObterPorIdAsync(id, empresaId);
        if (conta == null) return NotFound();

        return View(ContaBancariaViewModel.FromEntity(conta));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ContaBancariaViewModel model)
    {
        if (!TemPermissao("Financeiro.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var conta = model.ToEntity(empresaId);
        await _contaBancariaService.AlterarAsync(conta);

        TempData["Sucesso"] = "Conta bancária atualizada com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (!TemPermissao("Financeiro.Excluir"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var conta = await _contaBancariaService.ObterPorIdAsync(id, empresaId);
        if (conta == null) return NotFound();

        return View(ContaBancariaViewModel.FromEntity(conta));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!TemPermissao("Financeiro.Excluir"))
            return RedirectToAction("AccessDenied", "Auth");

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            await _contaBancariaService.ExcluirAsync(id, empresaId);
            TempData["Sucesso"] = "Conta bancária excluída com sucesso!";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
