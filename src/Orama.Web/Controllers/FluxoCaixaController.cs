using Microsoft.AspNetCore.Mvc;
using Orama.Application.Services;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

public class FluxoCaixaController : BaseController
{
    private readonly IContaBancariaService _contaBancariaService;
    private readonly IContaReceberService _contaReceberService;
    private readonly IContaPagarService _contaPagarService;

    public FluxoCaixaController(
        IContaBancariaService contaBancariaService,
        IContaReceberService contaReceberService,
        IContaPagarService contaPagarService)
    {
        _contaBancariaService = contaBancariaService;
        _contaReceberService = contaReceberService;
        _contaPagarService = contaPagarService;
    }

    public async Task<IActionResult> Index(DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        if (!TemPermissao("Financeiro.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        // Definir período padrão (30 dias)
        dataInicio ??= DateTime.Today.AddDays(-30);
        dataFim ??= DateTime.Today.AddDays(30);

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;

        var model = new FluxoCaixaViewModel
        {
            DataInicio = dataInicio.Value,
            DataFim = dataFim.Value,
            SaldoAtual = await _contaBancariaService.ObterSaldoTotalAsync(empresaId),
            TotalAReceber = await _contaReceberService.ObterTotalAReceberAsync(empresaId),
            TotalAPagar = await _contaPagarService.ObterTotalAPagarAsync(empresaId)
        };

        // Calcular projeção
        model.SaldoProjetado = model.SaldoAtual + model.TotalAReceber - model.TotalAPagar;

        // Obter movimentações do período
        var contasBancarias = await _contaBancariaService.ObterTodosAsync(empresaId);
        model.Movimentacoes = new List<MovimentacaoFluxoViewModel>();

        foreach (var conta in contasBancarias)
        {
            var movimentacoes = await _contaBancariaService.ObterMovimentacoesAsync(
                conta.Id, empresaId, dataInicio, dataFim);

            foreach (var mov in movimentacoes)
            {
                model.Movimentacoes.Add(new MovimentacaoFluxoViewModel
                {
                    Data = mov.DataMovimentacao,
                    Descricao = mov.Descricao,
                    Tipo = mov.Tipo.ToString(),
                    Valor = mov.Valor,
                    ContaBancaria = conta.Descricao,
                    SaldoAnterior = mov.SaldoAnterior,
                    SaldoPosterior = mov.SaldoPosterior
                });
            }
        }

        // Ordenar por data
        model.Movimentacoes = model.Movimentacoes.OrderByDescending(m => m.Data).ToList();

        return View(model);
    }

    /// <summary>
    /// Exporta fluxo de caixa para Excel
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ExportarExcel(DateTime dataInicio, DateTime dataFim)
    {
        if (!TemPermissao("Financeiro.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        // Por enquanto retorna JSON, pode ser implementado Excel depois
        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        
        var dados = new
        {
            periodo = $"{dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}",
            saldoAtual = await _contaBancariaService.ObterSaldoTotalAsync(empresaId),
            totalAReceber = await _contaReceberService.ObterTotalAReceberAsync(empresaId),
            totalAPagar = await _contaPagarService.ObterTotalAPagarAsync(empresaId)
        };

        return Json(new { success = true, dados });
    }
}