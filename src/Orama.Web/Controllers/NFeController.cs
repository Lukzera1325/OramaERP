using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orama.Application.Services.Fiscal.NFe;
using Orama.Domain.Entities.Fiscal.NFe;

namespace Orama.Web.Controllers
{
    /// <summary>
    /// Controller para emissão de NF-e - Isolado do core
    /// </summary>
    [Authorize]
    public class NFeController : BaseController
    {
        private readonly INFeEmissaoService _emissaoService;
        private readonly INFeConsultaService _consultaService;
        private readonly INFeCancelamentoService _cancelamentoService;
        
        public NFeController(
            INFeEmissaoService emissaoService,
            INFeConsultaService consultaService,
            INFeCancelamentoService cancelamentoService)
        {
            _emissaoService = emissaoService;
            _consultaService = consultaService;
            _cancelamentoService = cancelamentoService;
        }
        
        /// <summary>
        /// Lista NF-es da empresa
        /// </summary>
        public async Task<IActionResult> Index(DateTime? dataInicio, DateTime? dataFim)
        {
            try
            {
                var empresaId = UsuarioLogado?.EmpresaId ?? 0;
                
                // Definir período padrão (últimos 30 dias)
                dataInicio ??= DateTime.Today.AddDays(-30);
                dataFim ??= DateTime.Today;
                
                var nfes = await _emissaoService.ListarNFesAsync(empresaId, dataInicio, dataFim);
                
                ViewBag.DataInicio = dataInicio.Value.ToString("yyyy-MM-dd");
                ViewBag.DataFim = dataFim.Value.ToString("yyyy-MM-dd");
                
                return View(nfes);
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao carregar NF-es: {ex.Message}";
                return View(new List<NFeDocumento>());
            }
        }
        
        /// <summary>
        /// Gera NF-e a partir de uma venda
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GerarNFe(int vendaId)
        {
            try
            {
                // Validar venda
                var erros = await _emissaoService.ValidarVendaParaNFeAsync(vendaId);
                if (erros.Any())
                {
                    TempData["Erro"] = $"Venda inválida: {string.Join(", ", erros)}";
                    return RedirectToAction("Index", "Vendas");
                }
                
                // Gerar NF-e
                var nfe = await _emissaoService.GerarNFeAsync(vendaId, UsuarioId);
                
                TempData["Sucesso"] = $"NF-e {nfe.Numero} gerada com sucesso!";
                return RedirectToAction(nameof(Detalhes), new { id = nfe.Id });
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao gerar NF-e: {ex.Message}";
                return RedirectToAction("Index", "Vendas");
            }
        }
        
        /// <summary>
        /// Assina e envia NF-e para SEFAZ
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarNFe(int id)
        {
            try
            {
                var nfe = await _emissaoService.AssinarEEnviarAsync(id, UsuarioId);
                
                if (nfe.Status == NFeStatus.Autorizada)
                {
                    TempData["Sucesso"] = $"NF-e {nfe.Numero} autorizada com sucesso! Protocolo: {nfe.ProtocoloAutorizacao}";
                }
                else
                {
                    TempData["Erro"] = $"NF-e rejeitada: {nfe.MensagemSefaz}";
                }
                
                return RedirectToAction(nameof(Detalhes), new { id });
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao enviar NF-e: {ex.Message}";
                return RedirectToAction(nameof(Detalhes), new { id });
            }
        }
        
        /// <summary>
        /// Consulta situação da NF-e na SEFAZ
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConsultarNFe(int id)
        {
            try
            {
                var nfe = await _consultaService.ConsultarSituacaoAsync(id, UsuarioId);
                
                TempData["Sucesso"] = $"Consulta realizada. Status: {nfe.Status} - {nfe.MensagemSefaz}";
                return RedirectToAction(nameof(Detalhes), new { id });
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao consultar NF-e: {ex.Message}";
                return RedirectToAction(nameof(Detalhes), new { id });
            }
        }
        
        /// <summary>
        /// Cancela NF-e
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarNFe(int id, string justificativa)
        {
            try
            {
                var nfe = await _cancelamentoService.CancelarNFeAsync(id, justificativa, UsuarioId);
                
                TempData["Sucesso"] = $"NF-e {nfe.Numero} cancelada com sucesso!";
                return RedirectToAction(nameof(Detalhes), new { id });
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao cancelar NF-e: {ex.Message}";
                return RedirectToAction(nameof(Detalhes), new { id });
            }
        }
        
        /// <summary>
        /// Detalhes da NF-e
        /// </summary>
        public async Task<IActionResult> Detalhes(int id)
        {
            try
            {
                var empresaId = UsuarioLogado?.EmpresaId ?? 0;
                var nfe = await _emissaoService.ObterNFePorIdAsync(id, empresaId);
                
                if (nfe == null)
                {
                    TempData["Erro"] = "NF-e não encontrada";
                    return RedirectToAction(nameof(Index));
                }
                
                // Verificar se pode cancelar
                var (podeCancelar, motivoCancelamento) = await _cancelamentoService.PodeCancelarAsync(id);
                ViewBag.PodeCancelar = podeCancelar;
                ViewBag.MotivoCancelamento = motivoCancelamento;
                
                return View(nfe);
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao carregar NF-e: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
        
        /// <summary>
        /// Verifica status do serviço SEFAZ
        /// </summary>
        public async Task<IActionResult> StatusServico()
        {
            try
            {
                var empresaId = UsuarioLogado?.EmpresaId ?? 0;
                var statusOk = await _consultaService.VerificarStatusServicoAsync(empresaId);
                
                return Json(new { 
                    sucesso = true, 
                    statusServico = statusOk ? "Disponível" : "Indisponível",
                    disponivel = statusOk 
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    sucesso = false, 
                    erro = ex.Message 
                });
            }
        }
        
        /// <summary>
        /// Atualiza status de NF-es pendentes
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarStatusPendentes()
        {
            try
            {
                var empresaId = UsuarioLogado?.EmpresaId ?? 0;
                var atualizadas = await _consultaService.AtualizarStatusPendentesAsync(empresaId);
                
                TempData["Sucesso"] = $"{atualizadas} NF-es atualizadas";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao atualizar status: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
        
        /// <summary>
        /// Download do XML da NF-e
        /// </summary>
        public async Task<IActionResult> DownloadXml(int id)
        {
            try
            {
                var empresaId = UsuarioLogado?.EmpresaId ?? 0;
                var nfe = await _emissaoService.ObterNFePorIdAsync(id, empresaId);
                
                if (nfe == null)
                    return NotFound();
                
                var xml = nfe.XmlAssinado ?? nfe.XmlGerado;
                if (string.IsNullOrEmpty(xml))
                {
                    TempData["Erro"] = "XML não disponível";
                    return RedirectToAction(nameof(Detalhes), new { id });
                }
                
                var fileName = $"NFe_{nfe.ChaveAcesso ?? nfe.Numero.ToString("D9")}.xml";
                var bytes = System.Text.Encoding.UTF8.GetBytes(xml);
                
                return File(bytes, "application/xml", fileName);
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao baixar XML: {ex.Message}";
                return RedirectToAction(nameof(Detalhes), new { id });
            }
        }
    }
}