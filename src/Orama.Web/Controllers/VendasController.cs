using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orama.Application.Services;
using Orama.Domain.Entities;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

public class VendasController : BaseController
{
    private readonly IVendaService _vendaService;
    private readonly IClienteService _clienteService;
    private readonly IProdutoService _produtoService;
    private readonly IContaBancariaService _contaBancariaService;

    public VendasController(
        IVendaService vendaService,
        IClienteService clienteService,
        IProdutoService produtoService,
        IContaBancariaService contaBancariaService)
    {
        _vendaService = vendaService;
        _clienteService = clienteService;
        _produtoService = produtoService;
        _contaBancariaService = contaBancariaService;
    }

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
        if (venda == null) return NotFound();

        return View(VendaViewModel.FromEntity(venda));
    }

    public async Task<IActionResult> Create()
    {
        if (!TemPermissao("Vendas.Incluir"))
            return RedirectToAction("AccessDenied", "Auth");

        await CarregarViewBags();
        return View(new VendaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VendaViewModel model)
    {
        if (!TemPermissao("Vendas.Incluir"))
            return RedirectToAction("AccessDenied", "Auth");

        // Remover itens vazios (sem produto selecionado)
        model.Itens = model.Itens.Where(i => i.ProdutoId > 0).ToList();

        if (!model.Itens.Any())
        {
            ModelState.AddModelError("", "Adicione pelo menos um item à venda");
        }

        // Remover validações de campos que não são enviados no form
        ModelState.Remove("ClienteNome");
        ModelState.Remove("VendedorNome");
        foreach (var key in ModelState.Keys.Where(k => k.Contains("ProdutoNome") || k.Contains("ProdutoCodigo")).ToList())
        {
            ModelState.Remove(key);
        }

        if (!ModelState.IsValid)
        {
            await CarregarViewBags();
            return View(model);
        }

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            var venda = model.ToEntity(empresaId);
            await _vendaService.IncluirAsync(venda);

            TempData["Sucesso"] = "Venda cadastrada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
            await CarregarViewBags();
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!TemPermissao("Vendas.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        Console.WriteLine($"[DEBUG] Edit GET - Id: {id}, EmpresaId: {empresaId}");
        
        var venda = await _vendaService.ObterPorIdAsync(id, empresaId);
        if (venda == null)
        {
            Console.WriteLine($"[DEBUG] Venda não encontrada");
            return NotFound();
        }

        Console.WriteLine($"[DEBUG] Venda encontrada - Status: {venda.Status}, Itens: {venda.Itens?.Count ?? 0}");

        if (venda.Status == StatusVenda.Faturada || venda.Status == StatusVenda.Cancelado)
        {
            TempData["Erro"] = "Não é possível editar uma venda faturada ou cancelada";
            return RedirectToAction(nameof(Index));
        }

        await CarregarViewBags();
        var viewModel = VendaViewModel.FromEntity(venda);
        Console.WriteLine($"[DEBUG] ViewModel criado - Itens: {viewModel.Itens?.Count ?? 0}");
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VendaViewModel model)
    {
        if (!TemPermissao("Vendas.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        if (id != model.Id) return NotFound();

        // Remover itens vazios (sem produto selecionado)
        model.Itens = model.Itens.Where(i => i.ProdutoId > 0).ToList();

        if (!model.Itens.Any())
        {
            ModelState.AddModelError("", "Adicione pelo menos um item à venda");
        }

        // Remover validações de campos que não são enviados no form
        ModelState.Remove("ClienteNome");
        ModelState.Remove("VendedorNome");
        foreach (var key in ModelState.Keys.Where(k => k.Contains("ProdutoNome") || k.Contains("ProdutoCodigo")).ToList())
        {
            ModelState.Remove(key);
        }

        if (!ModelState.IsValid)
        {
            await CarregarViewBags();
            return View(model);
        }

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            var venda = model.ToEntity(empresaId);
            await _vendaService.AlterarAsync(venda);

            TempData["Sucesso"] = "Venda atualizada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
            await CarregarViewBags();
            return View(model);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (!TemPermissao("Vendas.Excluir"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var venda = await _vendaService.ObterPorIdAsync(id, empresaId);
        if (venda == null) return NotFound();

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

    [HttpPost]
    public async Task<IActionResult> Aprovar(int id)
    {
        if (!TemPermissao("Vendas.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            await _vendaService.AprovarAsync(id, empresaId);
            TempData["Sucesso"] = "Venda aprovada com sucesso!";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Faturar(int id)
    {
        if (!TemPermissao("Vendas.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var venda = await _vendaService.ObterPorIdAsync(id, empresaId);
        if (venda == null) return NotFound();

        var contas = await _contaBancariaService.ObterTodosAsync(empresaId);
        ViewBag.ContasBancarias = new SelectList(contas, "Id", "Descricao");

        return View(VendaViewModel.FromEntity(venda));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Faturar(int id, int? contaBancariaId)
    {
        if (!TemPermissao("Vendas.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            await _vendaService.FaturarAsync(id, empresaId, contaBancariaId);
            TempData["Sucesso"] = "Venda faturada com sucesso! Conta a receber gerada.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Cancelar(int id)
    {
        if (!TemPermissao("Vendas.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            await _vendaService.CancelarAsync(id, empresaId);
            TempData["Sucesso"] = "Venda cancelada com sucesso!";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> BuscarProduto(string termo)
    {
        var produtos = await _produtoService.BuscarAsync(termo);

        var resultado = produtos
            .Take(10)
            .Select(p => new
            {
                p.Id,
                p.Codigo,
                p.Descricao,
                p.PrecoVenda,
                p.EstoqueAtual,
                p.Unidade
            });

        return Json(resultado);
    }

    /// <summary>
    /// Adiciona item à venda via AJAX
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AdicionarItem(int vendaId, int produtoId, decimal quantidade)
    {
        if (!TemPermissao("Vendas.Alterar"))
            return Json(new { success = false, message = "Sem permissão" });

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            var item = new VendaItem
            {
                ProdutoId = produtoId,
                Quantidade = quantidade
            };

            await _vendaService.AdicionarItemAsync(vendaId, item, empresaId);
            return Json(new { success = true, message = "Item adicionado com sucesso!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza item da venda via AJAX
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AtualizarItem(int itemId, decimal quantidade, decimal valorUnitario)
    {
        if (!TemPermissao("Vendas.Alterar"))
            return Json(new { success = false, message = "Sem permissão" });

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            var item = new VendaItem
            {
                Id = itemId,
                Quantidade = quantidade,
                PrecoUnitario = valorUnitario
            };

            await _vendaService.AtualizarItemAsync(item, empresaId);
            return Json(new { success = true, message = "Item atualizado com sucesso!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Remove item da venda via AJAX
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RemoverItem(int itemId)
    {
        if (!TemPermissao("Vendas.Alterar"))
            return Json(new { success = false, message = "Sem permissão" });

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            await _vendaService.RemoverItemAsync(itemId, empresaId);
            return Json(new { success = true, message = "Item removido com sucesso!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Valida estoque antes de faturar
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ValidarEstoque(int vendaId)
    {
        if (!TemPermissao("Vendas.Visualizar"))
            return Json(new { success = false, message = "Sem permissão" });

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            var estoqueOk = await _vendaService.ValidarEstoqueAsync(vendaId, empresaId);
            return Json(new { success = true, estoqueOk });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Recalcula totais da venda via AJAX
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RecalcularTotais(int vendaId)
    {
        if (!TemPermissao("Vendas.Alterar"))
            return Json(new { success = false, message = "Sem permissão" });

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            var venda = await _vendaService.RecalcularTotaisAsync(vendaId, empresaId);
            
            return Json(new { 
                success = true, 
                subTotal = venda.SubTotal,
                valorDesconto = venda.ValorDesconto,
                valorFrete = venda.ValorFrete,
                valorTotal = venda.ValorTotal
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    private async Task CarregarViewBags()
    {
        var clientes = await _clienteService.ObterTodosAsync();
        var produtos = await _produtoService.ObterTodosAsync();

        ViewBag.Clientes = new SelectList(clientes, "Id", "Nome");
        ViewBag.Produtos = new SelectList(produtos, "Id", "CodigoFormatado");
    }
}
