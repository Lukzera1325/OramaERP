using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orama.Application.Services;
using Orama.Domain.Entities;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

public class ContasPagarController : BaseController
{
    private readonly IContaPagarService _contaPagarService;
    private readonly IFornecedorService _fornecedorService;
    private readonly IContaBancariaService _contaBancariaService;

    public ContasPagarController(
        IContaPagarService contaPagarService,
        IFornecedorService fornecedorService,
        IContaBancariaService contaBancariaService)
    {
        _contaPagarService = contaPagarService;
        _fornecedorService = fornecedorService;
        _contaBancariaService = contaBancariaService;
    }

    public async Task<IActionResult> Index(string filtro = "abertas")
    {
        if (!TemPermissao("Financeiro.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        
        IEnumerable<ContaPagar> contas = filtro switch
        {
            "abertas" => await _contaPagarService.ObterPorStatusAsync(empresaId, StatusConta.Aberta),
            "vencidas" => await _contaPagarService.ObterVencidasAsync(empresaId),
            "avencer" => await _contaPagarService.ObterAVencerAsync(empresaId, 7),
            _ => await _contaPagarService.ObterPorStatusAsync(empresaId, StatusConta.Aberta)
        };

        var viewModels = contas.Select(ContaPagarViewModel.FromEntity).ToList();
        
        ViewBag.TotalAPagar = await _contaPagarService.ObterTotalAPagarAsync(empresaId);
        ViewBag.TotalVencido = await _contaPagarService.ObterTotalVencidoAsync(empresaId);
        ViewBag.FiltroAtual = filtro;
        
        return View(viewModels);
    }

    /// <summary>
    /// Lista contas pagas (histórico)
    /// </summary>
    public async Task<IActionResult> Pagas()
    {
        if (!TemPermissao("Financeiro.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var contas = await _contaPagarService.ObterPorStatusAsync(empresaId, StatusConta.Paga);

        var viewModels = contas.Select(ContaPagarViewModel.FromEntity).ToList();
        ViewBag.TotalPago = contas.Sum(c => c.ValorTotal);
        
        return View(viewModels);
    }

    public async Task<IActionResult> Details(int id)
    {
        if (!TemPermissao("Financeiro.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var conta = await _contaPagarService.ObterPorIdAsync(id, empresaId);
        if (conta == null) return NotFound();

        return View(ContaPagarViewModel.FromEntity(conta));
    }

    public async Task<IActionResult> Create()
    {
        if (!TemPermissao("Financeiro.Incluir"))
            return RedirectToAction("AccessDenied", "Auth");

        await CarregarViewBags();
        return View(new ContaPagarViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContaPagarViewModel model)
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
        await _contaPagarService.IncluirAsync(conta);

        TempData["Sucesso"] = "Conta a pagar cadastrada com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!TemPermissao("Financeiro.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var conta = await _contaPagarService.ObterPorIdAsync(id, empresaId);
        if (conta == null) return NotFound();

        // Impedir edição de contas já pagas
        if (conta.Status == StatusConta.Paga)
        {
            TempData["Erro"] = "Não é possível editar uma conta que já foi paga.";
            return RedirectToAction(nameof(Index));
        }

        await CarregarViewBags();
        return View(ContaPagarViewModel.FromEntity(conta));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ContaPagarViewModel model)
    {
        if (!TemPermissao("Financeiro.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        if (id != model.Id) return NotFound();

        // Verificar se a conta não foi paga
        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var contaExistente = await _contaPagarService.ObterPorIdAsync(id, empresaId);
        if (contaExistente?.Status == StatusConta.Paga)
        {
            TempData["Erro"] = "Não é possível editar uma conta que já foi paga.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            await CarregarViewBags();
            return View(model);
        }
        var conta = model.ToEntity(empresaId);
        await _contaPagarService.AlterarAsync(conta);

        TempData["Sucesso"] = "Conta a pagar atualizada com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (!TemPermissao("Financeiro.Excluir"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var conta = await _contaPagarService.ObterPorIdAsync(id, empresaId);
        if (conta == null) return NotFound();

        return View(ContaPagarViewModel.FromEntity(conta));
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
            await _contaPagarService.ExcluirAsync(id, empresaId);
            TempData["Sucesso"] = "Conta a pagar excluída com sucesso!";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Pagar(int id)
    {
        if (!TemPermissao("Financeiro.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var conta = await _contaPagarService.ObterPorIdAsync(id, empresaId);
        if (conta == null) return NotFound();

        var model = new PagarContaViewModel
        {
            ContaId = conta.Id,
            Descricao = conta.Descricao,
            SaldoDevedor = conta.SaldoDevedor,
            ValorPago = conta.SaldoDevedor,
            DataPagamento = DateTime.Today
        };

        await CarregarContasBancarias();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pagar(PagarContaViewModel model)
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
            await _contaPagarService.PagarAsync(
                model.ContaId, 
                empresaId, 
                model.ValorPago, 
                model.ContaBancariaId, 
                model.DataPagamento);

            TempData["Sucesso"] = "Pagamento registrado com sucesso!";
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
    /// Lista contas pendentes de aprovação
    /// </summary>
    public async Task<IActionResult> Aprovacao()
    {
        if (!TemPermissao("Financeiro.Aprovar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var contas = await _contaPagarService.ObterContasParaAprovacaoAsync(empresaId);
        var viewModels = contas.Select(ContaPagarViewModel.FromEntity).ToList();

        return View(viewModels);
    }

    /// <summary>
    /// Aprova uma conta a pagar
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Aprovar(int id)
    {
        if (!TemPermissao("Financeiro.Aprovar"))
            return Json(new { success = false, message = "Sem permissão" });

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            var usuarioId = UsuarioLogado?.Id ?? 0;
            await _contaPagarService.AprovarContaAsync(id, empresaId, usuarioId);

            return Json(new { success = true, message = "Conta aprovada com sucesso!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Reprova uma conta a pagar
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Reprovar(int id, string motivo)
    {
        if (!TemPermissao("Financeiro.Aprovar"))
            return Json(new { success = false, message = "Sem permissão" });

        if (string.IsNullOrWhiteSpace(motivo))
            return Json(new { success = false, message = "Motivo da reprovação é obrigatório" });

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            var usuarioId = UsuarioLogado?.Id ?? 0;
            await _contaPagarService.ReprovarContaAsync(id, empresaId, usuarioId, motivo);

            return Json(new { success = true, message = "Conta reprovada com sucesso!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Agenda um pagamento
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Agendar(int id, DateTime dataAgendamento, int contaBancariaId)
    {
        if (!TemPermissao("Financeiro.Alterar"))
            return Json(new { success = false, message = "Sem permissão" });

        try
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            await _contaPagarService.AgendarPagamentoAsync(id, empresaId, dataAgendamento, contaBancariaId);

            return Json(new { success = true, message = "Pagamento agendado com sucesso!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Lista pagamentos agendados para hoje
    /// </summary>
    public async Task<IActionResult> Agendados()
    {
        if (!TemPermissao("Financeiro.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var contas = await _contaPagarService.ObterAgendadasAsync(empresaId, DateTime.Today);
        var viewModels = contas.Select(ContaPagarViewModel.FromEntity).ToList();

        return View(viewModels);
    }

    private async Task CarregarViewBags()
    {
        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var fornecedores = await _fornecedorService.ObterTodosAsync(empresaId);
        ViewBag.Fornecedores = new SelectList(fornecedores, "Id", "Nome");
        await CarregarContasBancarias();
    }

    private async Task CarregarContasBancarias()
    {
        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var contas = await _contaBancariaService.ObterTodosAsync(empresaId);
        ViewBag.ContasBancarias = new SelectList(contas, "Id", "Descricao");
    }
}
