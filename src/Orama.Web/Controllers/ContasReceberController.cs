using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orama.Application.Services;
using Orama.Domain.Entities;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

public class ContasReceberController : BaseController
{
    private readonly IContaReceberService _contaReceberService;
    private readonly IClienteService _clienteService;
    private readonly IContaBancariaService _contaBancariaService;

    public ContasReceberController(
        IContaReceberService contaReceberService,
        IClienteService clienteService,
        IContaBancariaService contaBancariaService)
    {
        _contaReceberService = contaReceberService;
        _clienteService = clienteService;
        _contaBancariaService = contaBancariaService;
    }

    public async Task<IActionResult> Index(string filtro = "abertas")
    {
        if (!TemPermissao("Financeiro.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        
        IEnumerable<ContaReceber> contas = filtro switch
        {
            "abertas" => await _contaReceberService.ObterPorStatusAsync(empresaId, StatusConta.Aberta),
            "vencidas" => await _contaReceberService.ObterVencidasAsync(empresaId),
            "avencer" => await _contaReceberService.ObterAVencerAsync(empresaId, 7),
            _ => await _contaReceberService.ObterPorStatusAsync(empresaId, StatusConta.Aberta)
        };

        var viewModels = contas.Select(ContaReceberViewModel.FromEntity).ToList();
        
        ViewBag.TotalAReceber = await _contaReceberService.ObterTotalAReceberAsync(empresaId);
        ViewBag.TotalVencido = await _contaReceberService.ObterTotalVencidoAsync(empresaId);
        ViewBag.FiltroAtual = filtro;
        
        return View(viewModels);
    }

    /// <summary>
    /// Lista contas recebidas (histórico)
    /// </summary>
    public async Task<IActionResult> Recebidas()
    {
        if (!TemPermissao("Financeiro.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var contas = await _contaReceberService.ObterPorStatusAsync(empresaId, StatusConta.Paga);

        var viewModels = contas.Select(ContaReceberViewModel.FromEntity).ToList();
        ViewBag.TotalRecebido = contas.Sum(c => c.ValorTotal);
        
        return View(viewModels);
    }

    public async Task<IActionResult> Details(int id)
    {
        if (!TemPermissao("Financeiro.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var conta = await _contaReceberService.ObterPorIdAsync(id, empresaId);
        if (conta == null) return NotFound();

        return View(ContaReceberViewModel.FromEntity(conta));
    }

    public async Task<IActionResult> Create()
    {
        if (!TemPermissao("Financeiro.Incluir"))
            return RedirectToAction("AccessDenied", "Auth");

        await CarregarViewBags();
        return View(new ContaReceberViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContaReceberViewModel model)
    {
        if (!TemPermissao("Financeiro.Incluir"))
            return RedirectToAction("AccessDenied", "Auth");

        if (!ModelState.IsValid)
        {
            await CarregarViewBags();
            return View(model);
        }

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var conta = model.ToEntity(empresaId);
        await _contaReceberService.IncluirAsync(conta);

        TempData["Sucesso"] = "Conta a receber cadastrada com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!TemPermissao("Financeiro.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var conta = await _contaReceberService.ObterPorIdAsync(id, empresaId);
        if (conta == null) return NotFound();

        // Impedir edição de contas já recebidas
        if (conta.Status == StatusConta.Paga)
        {
            TempData["Erro"] = "Não é possível editar uma conta que já foi recebida.";
            return RedirectToAction(nameof(Index));
        }

        await CarregarViewBags();
        return View(ContaReceberViewModel.FromEntity(conta));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ContaReceberViewModel model)
    {
        if (!TemPermissao("Financeiro.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        if (id != model.Id) return NotFound();

        // Verificar se a conta não foi recebida
        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var contaExistente = await _contaReceberService.ObterPorIdAsync(id, empresaId);
        if (contaExistente?.Status == StatusConta.Paga)
        {
            TempData["Erro"] = "Não é possível editar uma conta que já foi recebida.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            await CarregarViewBags();
            return View(model);
        }
        var conta = model.ToEntity(empresaId);
        await _contaReceberService.AlterarAsync(conta);

        TempData["Sucesso"] = "Conta a receber atualizada com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (!TemPermissao("Financeiro.Excluir"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var conta = await _contaReceberService.ObterPorIdAsync(id, empresaId);
        if (conta == null) return NotFound();

        return View(ContaReceberViewModel.FromEntity(conta));
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
            await _contaReceberService.ExcluirAsync(id, empresaId);
            TempData["Sucesso"] = "Conta a receber excluída com sucesso!";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Receber(int id)
    {
        if (!TemPermissao("Financeiro.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var conta = await _contaReceberService.ObterPorIdAsync(id, empresaId);
        if (conta == null) return NotFound();

        var model = new ReceberContaViewModel
        {
            ContaId = conta.Id,
            Descricao = conta.Descricao,
            SaldoDevedor = conta.SaldoDevedor,
            ValorRecebido = conta.SaldoDevedor,
            DataRecebimento = DateTime.Today
        };

        await CarregarContasBancarias();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Receber(ReceberContaViewModel model)
    {
        if (!TemPermissao("Financeiro.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        if (!ModelState.IsValid)
        {
            await CarregarContasBancarias();
            return View(model);
        }

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            await _contaReceberService.ReceberAsync(
                model.ContaId, 
                empresaId, 
                model.ValorRecebido, 
                model.ContaBancariaId, 
                model.DataRecebimento);

            TempData["Sucesso"] = "Recebimento registrado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
            await CarregarContasBancarias();
            return View(model);
        }
    }

    /// <summary>
    /// Calcula juros e multa para conta vencida
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CalcularJurosMulta(int id)
    {
        if (!TemPermissao("Financeiro.Alterar"))
            return Json(new { success = false, message = "Sem permissão" });

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            var conta = await _contaReceberService.CalcularJurosMultaAsync(id, empresaId, DateTime.Today);
            
            return Json(new { 
                success = true, 
                valorJuros = conta.ValorJuros,
                valorMulta = conta.ValorMulta,
                valorTotal = conta.ValorTotal,
                saldoDevedor = conta.SaldoDevedor
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Obtém contas de um cliente via AJAX
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObterContasCliente(int clienteId)
    {
        if (!TemPermissao("Financeiro.Visualizar"))
            return Json(new { success = false, message = "Sem permissão" });

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            var contas = await _contaReceberService.ObterPorClienteAsync(clienteId, empresaId);
            
            var result = contas.Select(c => new {
                id = c.Id,
                descricao = c.Descricao,
                dataVencimento = c.DataVencimento.ToString("dd/MM/yyyy"),
                valorTotal = c.ValorTotal,
                saldoDevedor = c.SaldoDevedor,
                status = c.Status.ToString(),
                vencida = c.Vencida
            });

            return Json(new { success = true, contas = result });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    private async Task CarregarViewBags()
    {
        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var clientes = await _clienteService.ObterTodosAsync();
        ViewBag.Clientes = new SelectList(clientes.Where(c => c.EmpresaId == empresaId), "Id", "Nome");
        await CarregarContasBancarias();
    }

    private async Task CarregarContasBancarias()
    {
        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var contas = await _contaBancariaService.ObterTodosAsync(empresaId);
        ViewBag.ContasBancarias = new SelectList(contas, "Id", "Descricao");
    }
}
