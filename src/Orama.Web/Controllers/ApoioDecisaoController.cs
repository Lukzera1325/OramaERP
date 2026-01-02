using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orama.Application.Services;
using Orama.Domain.Entities;
using Orama.Web.Controllers;
using Orama.Web.Models;

namespace Orama.Web.Controllers
{
    [Authorize]
    public class ApoioDecisaoController : BaseController
    {
        private readonly ISugestaoAcaoService _sugestaoService;
        private readonly ISimulacaoService _simulacaoService;
        private readonly IDecisaoGerencialService _decisaoService;
        private readonly IChecklistFechamentoService _checklistService;
        
        public ApoioDecisaoController(
            ISugestaoAcaoService sugestaoService,
            ISimulacaoService simulacaoService,
            IDecisaoGerencialService decisaoService,
            IChecklistFechamentoService checklistService)
        {
            _sugestaoService = sugestaoService;
            _simulacaoService = simulacaoService;
            _decisaoService = decisaoService;
            _checklistService = checklistService;
        }
        
        // Dashboard principal
        public async Task<IActionResult> Index()
        {
            var model = new ApoioDecisaoViewModel
            {
                // Sugestões pendentes
                SugestoesPendentes = await _sugestaoService.ObterSugestoesPendentesAsync(),
                EstatisticasSugestoes = await _sugestaoService.ObterEstatisticasSugestoesAsync(),
                
                // Decisões recentes
                DecisoesRecentes = await _decisaoService.ObterDecisoesRecentesAsync(10),
                EstatisticasDecisoes = await _decisaoService.ObterEstatisticasDecisoesAsync(),
                
                // Checklist atual
                ChecklistAtual = await _checklistService.ObterChecklistAtualAsync(),
                ResumoMensal = await _checklistService.ObterResumoMensalAsync(DateTime.Now.Year, DateTime.Now.Month)
            };
            
            return View(model);
        }
        
        // === SUGESTÕES DE AÇÃO ===
        
        public async Task<IActionResult> Sugestoes()
        {
            var sugestoes = await _sugestaoService.ObterSugestoesPendentesAsync();
            var estatisticas = await _sugestaoService.ObterEstatisticasSugestoesAsync();
            
            ViewBag.Estatisticas = estatisticas;
            return View(sugestoes);
        }
        
        public async Task<IActionResult> DetalhesSugestao(int id)
        {
            var sugestao = await _sugestaoService.ObterPorIdAsync(id);
            if (sugestao == null)
                return NotFound();
                
            return View(sugestao);
        }
        
        [HttpPost]
        public async Task<IActionResult> ResolverSugestao(int id, string observacao)
        {
            var sucesso = await _sugestaoService.ResolverSugestaoAsync(id, UsuarioId, observacao);
            
            if (sucesso)
            {
                // Registrar decisão
                var sugestao = await _sugestaoService.ObterPorIdAsync(id);
                if (sugestao != null)
                {
                    await _decisaoService.RegistrarDecisaoSugestaoAsync(id, $"Sugestão resolvida: {observacao}", observacao);
                }
                
                TempData["Sucesso"] = "Sugestão resolvida com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Erro ao resolver sugestão.";
            }
            
            return RedirectToAction(nameof(Sugestoes));
        }
        
        [HttpPost]
        public async Task<IActionResult> DescartarSugestao(int id, string motivo)
        {
            var sucesso = await _sugestaoService.DescartarSugestaoAsync(id, UsuarioId, motivo);
            
            if (sucesso)
            {
                TempData["Sucesso"] = "Sugestão descartada com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Erro ao descartar sugestão.";
            }
            
            return RedirectToAction(nameof(Sugestoes));
        }
        
        [HttpPost]
        public async Task<IActionResult> GerarSugestoes()
        {
            await _sugestaoService.GerarSugestoesAutomaticasAsync();
            TempData["Sucesso"] = "Sugestões geradas com sucesso!";
            return RedirectToAction(nameof(Sugestoes));
        }
        
        // === SIMULAÇÕES ===
        
        public IActionResult Simulacao()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> SimularPreco(int produtoId, decimal novoPreco)
        {
            try
            {
                var simulacao = await _simulacaoService.SimularNovoPrecoAsync(produtoId, novoPreco);
                return Json(new { sucesso = true, dados = simulacao });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, erro = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> SimularCusto(int produtoId, decimal novoCusto)
        {
            try
            {
                var simulacao = await _simulacaoService.SimularNovoCustoAsync(produtoId, novoCusto);
                return Json(new { sucesso = true, dados = simulacao });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, erro = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> SimularVenda(int produtoId, decimal quantidade, decimal precoUnitario)
        {
            try
            {
                var simulacao = await _simulacaoService.SimularVendaAsync(produtoId, quantidade, precoUnitario);
                return Json(new { sucesso = true, dados = simulacao });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, erro = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> SimularCenarios(int produtoId)
        {
            try
            {
                var cenarios = await _simulacaoService.SimularCenariosProdutoAsync(produtoId);
                return Json(new { sucesso = true, dados = cenarios });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, erro = ex.Message });
            }
        }
        
        // === HISTÓRICO DE DECISÕES ===
        
        public async Task<IActionResult> Decisoes()
        {
            var decisoes = await _decisaoService.ObterDecisoesRecentesAsync(50);
            var estatisticas = await _decisaoService.ObterEstatisticasDecisoesAsync();
            
            ViewBag.Estatisticas = estatisticas;
            return View(decisoes);
        }
        
        public async Task<IActionResult> DetalhesDecisao(int id)
        {
            var decisao = await _decisaoService.ObterPorIdAsync(id);
            if (decisao == null)
                return NotFound();
                
            return View(decisao);
        }
        
        [HttpPost]
        public async Task<IActionResult> RegistrarDecisao(int referenciaId, string tipoReferencia, string problema, string acao, string observacoes = "")
        {
            bool sucesso = false;
            
            switch (tipoReferencia.ToLower())
            {
                case "produto":
                    sucesso = await _decisaoService.RegistrarDecisaoProdutoAsync(referenciaId, problema, acao, observacoes);
                    break;
                case "venda":
                    sucesso = await _decisaoService.RegistrarDecisaoVendaAsync(referenciaId, problema, acao, observacoes);
                    break;
                case "sugestao":
                    sucesso = await _decisaoService.RegistrarDecisaoSugestaoAsync(referenciaId, acao, observacoes);
                    break;
            }
            
            if (sucesso)
            {
                TempData["Sucesso"] = "Decisão registrada com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Erro ao registrar decisão.";
            }
            
            return RedirectToAction(nameof(Decisoes));
        }
        
        [HttpPost]
        public async Task<IActionResult> AvaliarDecisao(int id, string resultado, bool foiEfetiva)
        {
            var sucesso = await _decisaoService.AvaliarResultadoDecisaoAsync(id, resultado, foiEfetiva);
            
            if (sucesso)
            {
                TempData["Sucesso"] = "Decisão avaliada com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Erro ao avaliar decisão.";
            }
            
            return RedirectToAction(nameof(Decisoes));
        }
        
        // === CHECKLIST DE FECHAMENTO ===
        
        public async Task<IActionResult> Checklist()
        {
            var checklist = await _checklistService.ObterChecklistAtualAsync();
            
            if (checklist == null)
            {
                // Criar checklist para o mês atual
                checklist = await _checklistService.CriarChecklistMensalAsync(DateTime.Now.Year, DateTime.Now.Month);
            }
            
            return View(checklist);
        }
        
        public async Task<IActionResult> DetalhesChecklist(int id)
        {
            var checklist = await _checklistService.ObterPorIdAsync(id);
            if (checklist == null)
                return NotFound();
                
            return View(checklist);
        }
        
        [HttpPost]
        public async Task<IActionResult> MarcarItem(int checklistId, int itemId, string observacoes = "")
        {
            var sucesso = await _checklistService.MarcarItemConcluidoAsync(checklistId, itemId, observacoes);
            
            if (sucesso)
            {
                TempData["Sucesso"] = "Item marcado como concluído!";
            }
            else
            {
                TempData["Erro"] = "Erro ao marcar item.";
            }
            
            return RedirectToAction(nameof(DetalhesChecklist), new { id = checklistId });
        }
        
        [HttpPost]
        public async Task<IActionResult> DesmarcarItem(int checklistId, int itemId)
        {
            var sucesso = await _checklistService.DesmarcarItemAsync(checklistId, itemId);
            
            if (sucesso)
            {
                TempData["Sucesso"] = "Item desmarcado!";
            }
            else
            {
                TempData["Erro"] = "Erro ao desmarcar item.";
            }
            
            return RedirectToAction(nameof(DetalhesChecklist), new { id = checklistId });
        }
        
        [HttpPost]
        public async Task<IActionResult> ConcluirChecklist(int id)
        {
            var sucesso = await _checklistService.ConcluirChecklistAsync(id);
            
            if (sucesso)
            {
                TempData["Sucesso"] = "Checklist concluído com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Não é possível concluir o checklist. Verifique se todos os itens obrigatórios foram concluídos.";
            }
            
            return RedirectToAction(nameof(DetalhesChecklist), new { id });
        }
        
        [HttpPost]
        public async Task<IActionResult> ReabrirChecklist(int id)
        {
            var sucesso = await _checklistService.ReabrirChecklistAsync(id);
            
            if (sucesso)
            {
                TempData["Sucesso"] = "Checklist reaberto!";
            }
            else
            {
                TempData["Erro"] = "Erro ao reabrir checklist.";
            }
            
            return RedirectToAction(nameof(DetalhesChecklist), new { id });
        }
        
        public async Task<IActionResult> HistoricoChecklists()
        {
            var checklists = await _checklistService.ObterHistoricoChecklistsAsync();
            return View(checklists);
        }
        
        [HttpPost]
        public async Task<IActionResult> CriarChecklistMensal(int ano, int mes)
        {
            var checklist = await _checklistService.CriarChecklistMensalAsync(ano, mes);
            TempData["Sucesso"] = $"Checklist criado para {mes:00}/{ano}!";
            return RedirectToAction(nameof(DetalhesChecklist), new { id = checklist.Id });
        }
    }
}