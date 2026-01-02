using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orama.Application.Services.Fiscal;
using Orama.Domain.Entities.Fiscal;
using Orama.Web.Controllers;

namespace Orama.Web.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de configurações fiscais
    /// Isolado do core, focado apenas em configuração fiscal
    /// </summary>
    [Authorize]
    public class FiscalController : BaseController
    {
        private readonly IEmpresaFiscalService _empresaFiscalService;
        private readonly IContextoFiscalService _contextoFiscalService;
        
        public FiscalController(
            IEmpresaFiscalService empresaFiscalService,
            IContextoFiscalService contextoFiscalService)
        {
            _empresaFiscalService = empresaFiscalService;
            _contextoFiscalService = contextoFiscalService;
        }
        
        /// <summary>
        /// Página principal de configurações fiscais
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var empresaId = UsuarioLogado?.EmpresaId ?? 0;
                var configuracoes = await _empresaFiscalService.ListarConfiguracoesAsync(empresaId);
                return View(configuracoes);
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao carregar configurações fiscais: {ex.Message}";
                return View(new List<EmpresaFiscalConfig>());
            }
        }
        
        /// <summary>
        /// Formulário para criar nova configuração fiscal
        /// </summary>
        public IActionResult Criar()
        {
            var empresaId = UsuarioLogado?.EmpresaId ?? 0;
            var configuracao = new EmpresaFiscalConfig
            {
                EmpresaId = empresaId,
                VigenteDe = DateTime.Today,
                VersaoFiscal = "2025.1",
                AmbienteFiscal = AmbienteFiscal.Homologacao,
                RegimeTributario = RegimeTributario.SimplesNacional,
                CRT = CRT.SimplesNacional,
                UF = "SP"
            };
            
            return View(configuracao);
        }
        
        /// <summary>
        /// Salva nova configuração fiscal
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(EmpresaFiscalConfig configuracao)
        {
            try
            {
                var empresaId = UsuarioLogado?.EmpresaId ?? 0;
                configuracao.EmpresaId = empresaId;
                configuracao.CriadoPor = UsuarioId;
                
                await _empresaFiscalService.CriarConfiguracaoAsync(configuracao);
                
                TempData["Sucesso"] = "Configuração fiscal criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao criar configuração fiscal: {ex.Message}";
                return View(configuracao);
            }
        }
        
        /// <summary>
        /// Formulário para editar configuração fiscal
        /// </summary>
        public async Task<IActionResult> Editar(int id)
        {
            try
            {
                var empresaId = UsuarioLogado?.EmpresaId ?? 0;
                var configuracoes = await _empresaFiscalService.ListarConfiguracoesAsync(empresaId);
                var configuracao = configuracoes.FirstOrDefault(c => c.Id == id);
                
                if (configuracao == null)
                {
                    TempData["Erro"] = "Configuração fiscal não encontrada";
                    return RedirectToAction(nameof(Index));
                }
                
                return View(configuracao);
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao carregar configuração fiscal: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
        
        /// <summary>
        /// Salva alterações na configuração fiscal
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EmpresaFiscalConfig configuracao)
        {
            try
            {
                var empresaId = UsuarioLogado?.EmpresaId ?? 0;
                configuracao.EmpresaId = empresaId;
                
                await _empresaFiscalService.AtualizarConfiguracaoAsync(configuracao);
                
                TempData["Sucesso"] = "Configuração fiscal atualizada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao atualizar configuração fiscal: {ex.Message}";
                return View(configuracao);
            }
        }
        
        /// <summary>
        /// Encerra vigência de uma configuração fiscal
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EncerrarVigencia(int id, DateTime dataEncerramento)
        {
            try
            {
                await _empresaFiscalService.EncerrarVigenciaAsync(id, dataEncerramento);
                
                TempData["Sucesso"] = "Vigência da configuração fiscal encerrada com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao encerrar vigência: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        /// <summary>
        /// Testa montagem de contexto fiscal (para debug)
        /// </summary>
        public async Task<IActionResult> TestarContexto(int produtoId, int tipoOperacao, decimal valor)
        {
            try
            {
                var empresaId = UsuarioLogado?.EmpresaId ?? 0;
                var contexto = await _contextoFiscalService.MontarContextoAsync(
                    empresaId,
                    produtoId,
                    (TipoOperacaoFiscal)tipoOperacao,
                    DateTime.Today,
                    valor);
                
                return Json(new
                {
                    sucesso = true,
                    contexto = new
                    {
                        empresa = new
                        {
                            regime = contexto.RegimeTributario.ToString(),
                            crt = contexto.EmpresaFiscalConfig.CRT.ToString(),
                            uf = contexto.UFOrigem,
                            ambiente = contexto.AmbienteFiscal.ToString(),
                            versao = contexto.VersaoFiscal
                        },
                        produto = new
                        {
                            ncm = contexto.ProdutoFiscalConfig.NCM,
                            origem = contexto.ProdutoFiscalConfig.Origem.ToString(),
                            cstOuCsosn = contexto.ObterCSTOuCSOSN(),
                            aliquota = contexto.ProdutoFiscalConfig.AliquotaICMSPadrao
                        },
                        operacao = new
                        {
                            tipo = contexto.OperacaoFiscalConfig.TipoOperacao.ToString(),
                            cfop = contexto.OperacaoFiscalConfig.CFOPPadrao,
                            descricao = contexto.OperacaoFiscalConfig.Descricao
                        },
                        valido = contexto.IsValido()
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    sucesso = false,
                    erro = ex.Message
                });
            }
        }
    }
}