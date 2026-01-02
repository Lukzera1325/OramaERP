using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orama.Application.Services;
using Orama.Domain.Entities;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller extremamente simples para Vendas
/// Apenas recebe requests, valida input básico, chama service e retorna resposta
/// ZERO lógica de negócio aqui
/// </summary>
public class VendasController : BaseController
{
    private readonly IVendaService _vendaService;
    private readonly IClienteService _clienteService;
    private readonly IProdutoService _produtoService;

    public VendasController(
        IVendaService vendaService,
        IClienteService clienteService,
        IProdutoService produtoService)
    {
        _vendaService = vendaService;
        _clienteService = clienteService;
        _produtoService = produtoService;
    }

    // CRUD Básico - Simples e Direto

    public async Task<IActionResult> Index(string filtro = "todas")
    {
        if (!TemPermissao("Vendas.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;

        IEnumerable<Venda> vendas = filtro switch
        {
            "orcamentos" => await _vendaService.ObterPorStatusAsync(empresaId, StatusVenda.Orcamento),
            "aprovadas" => await _vendaService.ObterPorStatusAsync(empresaId, StatusVenda.Aprovado),
            "faturadas" => await _vendaService.ObterPorStatusAsync(empresaId, StatusVenda.Faturada),
            "canceladas" => await _vendaService.ObterPorStatusAsync(empresaId, StatusVenda.Cancelado),
            _ => await _vendaService.ObterTodosAsync(empresaId)
        };

        var viewModels = vendas.Select(VendaViewModel.FromEntity).ToList();

        ViewBag.TotalVendas = await _vendaService.ObterTotalVendasAsync(empresaId, DateTime.Today.AddDays(-30), DateTime.Today);
        ViewBag.FiltroAtual = filtro;

        return View(viewModels);
    }

    public async Task<IActionResult> Details(int id)
    {
        if (!TemPermissao("Vendas.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var venda = await _vendaService.ObterPorIdAsync(id, empresaId);
        
        if (venda == null) 
            return NotFound();

        return View(VendaViewModel.FromEntity(venda));
    }

    public async Task<IActionResult> Create()
    {
        if (!TemPermissao("Vendas.Incluir"))
            return RedirectToAction("AccessDenied", "Auth");

        await PrepararDadosFormulario();
        return View(new VendaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VendaViewModel model)
    {
        if (!TemPermissao("Vendas.Incluir"))
            return RedirectToAction("AccessDenied", "Auth");

        if (!ModelState.IsValid)
        {
            await PrepararDadosFormulario();
            return View(model);
        }

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            var venda = model.ToEntity(empresaId);
            
            await _vendaService.CriarAsync(venda);
            
            TempData["Sucesso"] = "Venda criada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
            await PrepararDadosFormulario();
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!TemPermissao("Vendas.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var venda = await _vendaService.ObterPorIdAsync(id, empresaId);
        
        if (venda == null) 
            return NotFound();

        await PrepararDadosFormulario();
        return View(VendaViewModel.FromEntity(venda));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(VendaViewModel model)
    {
        if (!TemPermissao("Vendas.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        if (!ModelState.IsValid)
        {
            await PrepararDadosFormulario();
            return View(model);
        }

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            var venda = model.ToEntity(empresaId);
            
            await _vendaService.AtualizarAsync(venda);
            
            TempData["Sucesso"] = "Venda atualizada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
            await PrepararDadosFormulario();
            return View(model);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (!TemPermissao("Vendas.Excluir"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var venda = await _vendaService.ObterPorIdAsync(id, empresaId);
        
        if (venda == null) 
            return NotFound();

        return View(VendaViewModel.FromEntity(venda));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!TemPermissao("Vendas.Excluir"))
            return RedirectToAction("AccessDenied", "Auth");

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            await _vendaService.ExcluirAsync(id, empresaId);
            
            TempData["Sucesso"] = "Venda excluída com sucesso!";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // Operações de Negócio - Uma linha cada

    [HttpPost]
    public async Task<IActionResult> Aprovar(int id)
    {
        if (!TemPermissao("Vendas.Aprovar"))
            return Json(new { sucesso = false, mensagem = "Sem permissão" });

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            await _vendaService.AprovarAsync(id, empresaId);
            
            return Json(new { sucesso = true, mensagem = "Venda aprovada com sucesso!" });
        }
        catch (Exception ex)
        {
            return Json(new { sucesso = false, mensagem = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Faturar(int id)
    {
        if (!TemPermissao("Vendas.Faturar"))
            return Json(new { sucesso = false, mensagem = "Sem permissão" });

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            await _vendaService.FaturarAsync(id, empresaId);
            
            return Json(new { sucesso = true, mensagem = "Venda faturada com sucesso!" });
        }
        catch (Exception ex)
        {
            return Json(new { sucesso = false, mensagem = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Cancelar(int id, string motivo = "")
    {
        if (!TemPermissao("Vendas.Cancelar"))
            return Json(new { sucesso = false, mensagem = "Sem permissão" });

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            await _vendaService.CancelarAsync(id, empresaId, motivo);
            
            return Json(new { sucesso = true, mensagem = "Venda cancelada com sucesso!" });
        }
        catch (Exception ex)
        {
            return Json(new { sucesso = false, mensagem = ex.Message });
        }
    }

    // Método auxiliar privado - Simples

    private async Task PrepararDadosFormulario()
    {
        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        
        ViewBag.Clientes = new SelectList(
            await _clienteService.ObterTodosAsync(empresaId), 
            "Id", "Nome"
        );
        
        ViewBag.Produtos = new SelectList(
            await _produtoService.ObterTodosAsync(empresaId), 
            "Id", "Descricao"
        );
    }
}