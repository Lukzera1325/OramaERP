using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orama.Application.Services;
using Orama.Domain.Entities;
using Orama.Web.Models;

namespace Orama.Web.Controllers
{
    [Authorize]
    public class ProducaoController : BaseController
    {
        private readonly IOrdemProducaoService _ordemProducaoService;
        private readonly IListaMateriaisService _listaMateriaisService;
        private readonly IProdutoService _produtoService;

        public ProducaoController(
            IOrdemProducaoService ordemProducaoService,
            IListaMateriaisService listaMateriaisService,
            IProdutoService produtoService)
        {
            _ordemProducaoService = ordemProducaoService;
            _listaMateriaisService = listaMateriaisService;
            _produtoService = produtoService;
        }

        public async Task<IActionResult> Index()
        {
            var empresaId = ObterEmpresaId();
            
            ViewBag.OrdensAtivas = await _ordemProducaoService.ObterQuantidadeOrdensAtivasAsync(empresaId);
            ViewBag.OrdensAtrasadas = await _ordemProducaoService.ObterQuantidadeOrdensAtrasadasAsync(empresaId);
            ViewBag.CustoMes = await _ordemProducaoService.ObterCustoProducaoMesAsync(empresaId, DateTime.Now.Month, DateTime.Now.Year);
            
            return View();
        }

        public async Task<IActionResult> OrdensProducao()
        {
            var empresaId = ObterEmpresaId();
            var ordens = await _ordemProducaoService.ObterTodosAsync(empresaId);
            
            var viewModel = ordens.Select(o => new OrdemProducaoViewModel
            {
                Id = o.Id,
                Numero = o.Numero,
                ProdutoDescricao = o.Produto.Descricao,
                QuantidadePlanejada = o.QuantidadePlanejada,
                QuantidadeProduzida = o.QuantidadeProduzida,
                DataPlanejada = o.DataPlanejada,
                DataInicio = o.DataInicio,
                DataFim = o.DataFim,
                Status = o.Status,
                Prioridade = o.Prioridade,
                CustoMaterial = o.CustoMaterial,
                CustoMaoObra = o.CustoMaoObra,
                DataCriacao = o.DataCriacao,
                UsuarioCriacao = o.UsuarioCriacao.Nome
            }).ToList();
            
            return View(viewModel);
        }

        public async Task<IActionResult> CriarOrdemProducao()
        {
            var empresaId = ObterEmpresaId();
            var produtos = await _produtoService.ObterTodosAsync(empresaId);
            
            var viewModel = new OrdemProducaoViewModel
            {
                DataPlanejada = DateTime.Today.AddDays(1),
                Prioridade = PrioridadeOrdemProducao.Normal,
                Produtos = new SelectList(produtos, "Id", "Descricao")
            };
            
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarOrdemProducao(OrdemProducaoViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var empresaId = ObterEmpresaId();
                    var usuarioId = UsuarioLogado?.Id ?? 1;
                    
                    var ordem = new OrdemProducao
                    {
                        EmpresaId = empresaId,
                        ProdutoId = viewModel.ProdutoId,
                        QuantidadePlanejada = viewModel.QuantidadePlanejada,
                        DataPlanejada = viewModel.DataPlanejada,
                        Prioridade = viewModel.Prioridade,
                        Observacoes = viewModel.Observacoes,
                        UsuarioCriacaoId = usuarioId
                    };
                    
                    await _ordemProducaoService.CriarAsync(ordem);
                    
                    TempData["Sucesso"] = "Ordem de produção criada com sucesso!";
                    return RedirectToAction(nameof(OrdensProducao));
                }
                catch (Exception ex)
                {
                    TempData["Erro"] = $"Erro ao criar ordem de produção: {ex.Message}";
                }
            }
            
            // Recarregar dados em caso de erro
            var empresaIdReload = ObterEmpresaId();
            var produtosReload = await _produtoService.ObterTodosAsync(empresaIdReload);
            viewModel.Produtos = new SelectList(produtosReload, "Id", "Descricao");
            
            return View(viewModel);
        }

        public async Task<IActionResult> DetalhesOrdemProducao(int id)
        {
            var empresaId = ObterEmpresaId();
            var ordem = await _ordemProducaoService.ObterPorIdAsync(id, empresaId);
            
            if (ordem == null)
            {
                TempData["Erro"] = "Ordem de produção não encontrada.";
                return RedirectToAction(nameof(OrdensProducao));
            }
            
            var viewModel = new OrdemProducaoViewModel
            {
                Id = ordem.Id,
                Numero = ordem.Numero,
                ProdutoId = ordem.ProdutoId,
                ProdutoDescricao = ordem.Produto.Descricao,
                QuantidadePlanejada = ordem.QuantidadePlanejada,
                QuantidadeProduzida = ordem.QuantidadeProduzida,
                DataPlanejada = ordem.DataPlanejada,
                DataInicio = ordem.DataInicio,
                DataFim = ordem.DataFim,
                Status = ordem.Status,
                Prioridade = ordem.Prioridade,
                Observacoes = ordem.Observacoes,
                CustoMaterial = ordem.CustoMaterial,
                CustoMaoObra = ordem.CustoMaoObra,
                DataCriacao = ordem.DataCriacao,
                UsuarioCriacao = ordem.UsuarioCriacao.Nome,
                
                Itens = ordem.Itens.Select(i => new OrdemProducaoItemViewModel
                {
                    Id = i.Id,
                    ProdutoId = i.ProdutoId,
                    ProdutoDescricao = i.Produto.Descricao,
                    QuantidadeNecessaria = i.QuantidadeNecessaria,
                    QuantidadeConsumida = i.QuantidadeConsumida,
                    CustoUnitario = i.CustoUnitario,
                    Tipo = i.Tipo,
                    Observacoes = i.Observacoes
                }).ToList(),
                
                Etapas = ordem.Etapas.Select(e => new OrdemProducaoEtapaViewModel
                {
                    Id = e.Id,
                    Nome = e.Nome,
                    Descricao = e.Descricao,
                    Sequencia = e.Sequencia,
                    TempoEstimado = e.TempoEstimado,
                    TempoRealizado = e.TempoRealizado,
                    DataInicio = e.DataInicio,
                    DataFim = e.DataFim,
                    Status = e.Status,
                    ResponsavelNome = e.Responsavel?.Nome,
                    Observacoes = e.Observacoes
                }).OrderBy(e => e.Sequencia).ToList(),
                
                ApontamentosHoras = ordem.ApontamentosHoras.Select(ah => new ApontamentoHorasViewModel
                {
                    Id = ah.Id,
                    EtapaNome = ah.Etapa?.Nome,
                    FuncionarioNome = ah.Funcionario.Nome,
                    DataInicio = ah.DataInicio,
                    DataFim = ah.DataFim,
                    HorasTrabalhadas = ah.HorasTrabalhadas,
                    ValorHora = ah.ValorHora,
                    Tipo = ah.Tipo,
                    Observacoes = ah.Observacoes
                }).OrderByDescending(ah => ah.DataInicio).ToList(),
                
                InspecoesQualidade = ordem.InspecoesQualidade.Select(iq => new InspecaoQualidadeViewModel
                {
                    Id = iq.Id,
                    EtapaNome = iq.Etapa?.Nome,
                    Titulo = iq.Titulo,
                    Tipo = iq.Tipo,
                    QuantidadeInspecionada = iq.QuantidadeInspecionada,
                    QuantidadeAprovada = iq.QuantidadeAprovada,
                    QuantidadeRejeitada = iq.QuantidadeRejeitada,
                    Resultado = iq.Resultado,
                    DataInspecao = iq.DataInspecao,
                    InspetorNome = iq.Inspetor.Nome,
                    Observacoes = iq.Observacoes
                }).OrderByDescending(iq => iq.DataInspecao).ToList()
            };
            
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LiberarOrdem(int id)
        {
            try
            {
                var empresaId = ObterEmpresaId();
                var usuarioId = UsuarioLogado?.Id ?? 1;
                
                var sucesso = await _ordemProducaoService.LiberarOrdemAsync(id, empresaId, usuarioId);
                
                if (sucesso)
                {
                    TempData["Sucesso"] = "Ordem de produção liberada com sucesso!";
                }
                else
                {
                    TempData["Erro"] = "Não foi possível liberar a ordem. Verifique a disponibilidade de materiais.";
                }
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao liberar ordem: {ex.Message}";
            }
            
            return RedirectToAction(nameof(DetalhesOrdemProducao), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> IniciarProducao(int id)
        {
            try
            {
                var empresaId = ObterEmpresaId();
                var usuarioId = UsuarioLogado?.Id ?? 1;
                
                var sucesso = await _ordemProducaoService.IniciarProducaoAsync(id, empresaId, usuarioId);
                
                if (sucesso)
                {
                    TempData["Sucesso"] = "Produção iniciada com sucesso!";
                }
                else
                {
                    TempData["Erro"] = "Não foi possível iniciar a produção.";
                }
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao iniciar produção: {ex.Message}";
            }
            
            return RedirectToAction(nameof(DetalhesOrdemProducao), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> FinalizarProducao(int id, decimal quantidadeProduzida)
        {
            try
            {
                var empresaId = ObterEmpresaId();
                var usuarioId = UsuarioLogado?.Id ?? 1;
                
                // Atualizar quantidade produzida
                var ordem = await _ordemProducaoService.ObterPorIdAsync(id, empresaId);
                if (ordem != null)
                {
                    ordem.QuantidadeProduzida = quantidadeProduzida;
                    await _ordemProducaoService.AtualizarAsync(ordem);
                }
                
                var sucesso = await _ordemProducaoService.FinalizarProducaoAsync(id, empresaId, usuarioId);
                
                if (sucesso)
                {
                    TempData["Sucesso"] = "Produção finalizada com sucesso!";
                }
                else
                {
                    TempData["Erro"] = "Não foi possível finalizar a produção.";
                }
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao finalizar produção: {ex.Message}";
            }
            
            return RedirectToAction(nameof(DetalhesOrdemProducao), new { id });
        }
    }
}