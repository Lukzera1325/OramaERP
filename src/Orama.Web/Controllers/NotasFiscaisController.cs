using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orama.Application.Services;
using Orama.Domain.Entities;
using Orama.Web.Models;

namespace Orama.Web.Controllers
{
    [Authorize]
    public class NotasFiscaisController : BaseController
    {
        private readonly INotaFiscalService _notaFiscalService;
        private readonly IClienteService _clienteService;
        private readonly IFornecedorService _fornecedorService;
        private readonly IProdutoService _produtoService;
        private readonly IVendaService _vendaService;
        private readonly ICompraService _compraService;

        public NotasFiscaisController(
            INotaFiscalService notaFiscalService,
            IClienteService clienteService,
            IFornecedorService fornecedorService,
            IProdutoService produtoService,
            IVendaService vendaService,
            ICompraService compraService)
        {
            _notaFiscalService = notaFiscalService;
            _clienteService = clienteService;
            _fornecedorService = fornecedorService;
            _produtoService = produtoService;
            _vendaService = vendaService;
            _compraService = compraService;
        }

        public async Task<IActionResult> Index(NotaFiscalFiltroViewModel? filtro = null)
        {
            var empresaId = ObterEmpresaId();
            IEnumerable<NotaFiscal> notasFiscais;

            if (filtro != null && (filtro.DataInicial.HasValue || filtro.DataFinal.HasValue || 
                                  !string.IsNullOrEmpty(filtro.Numero) || !string.IsNullOrEmpty(filtro.Status)))
            {
                // Aplicar filtros
                notasFiscais = await _notaFiscalService.ObterTodosAsync(empresaId);
                
                if (filtro.DataInicial.HasValue)
                    notasFiscais = notasFiscais.Where(nf => nf.DataEmissao >= filtro.DataInicial.Value);
                
                if (filtro.DataFinal.HasValue)
                    notasFiscais = notasFiscais.Where(nf => nf.DataEmissao <= filtro.DataFinal.Value);
                
                if (!string.IsNullOrEmpty(filtro.Numero))
                    notasFiscais = notasFiscais.Where(nf => nf.Numero.Contains(filtro.Numero));
                
                if (!string.IsNullOrEmpty(filtro.Status))
                    notasFiscais = notasFiscais.Where(nf => nf.Status == filtro.Status);
                
                if (!string.IsNullOrEmpty(filtro.Tipo))
                    notasFiscais = notasFiscais.Where(nf => nf.Tipo == filtro.Tipo);
            }
            else
            {
                notasFiscais = await _notaFiscalService.ObterTodosAsync(empresaId);
            }

            var viewModel = notasFiscais.Select(nf => new NotaFiscalViewModel
            {
                Id = nf.Id,
                Numero = nf.Numero,
                Serie = nf.Serie,
                Tipo = nf.Tipo,
                Status = nf.Status,
                DataEmissao = nf.DataEmissao,
                DataSaida = nf.DataSaida,
                ClienteNome = nf.Cliente?.Nome,
                FornecedorNome = nf.Fornecedor?.Nome,
                VendaNumero = nf.Venda?.Numero,
                CompraNumero = nf.Compra?.NumeroCompra,
                ValorTotal = nf.ValorTotal,
                NaturezaOperacao = nf.NaturezaOperacao,
                ChaveAcesso = nf.ChaveAcesso,
                DataAutorizacao = nf.DataAutorizacao,
                DataCriacao = nf.DataCriacao
            }).ToList();

            ViewBag.Filtro = filtro ?? new NotaFiscalFiltroViewModel();
            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var empresaId = ObterEmpresaId();
            var notaFiscal = await _notaFiscalService.ObterPorIdAsync(id, empresaId);

            if (notaFiscal == null)
                return NotFound();

            var viewModel = new NotaFiscalViewModel
            {
                Id = notaFiscal.Id,
                Numero = notaFiscal.Numero,
                Serie = notaFiscal.Serie,
                Tipo = notaFiscal.Tipo,
                Status = notaFiscal.Status,
                DataEmissao = notaFiscal.DataEmissao,
                DataSaida = notaFiscal.DataSaida,
                ClienteId = notaFiscal.ClienteId,
                ClienteNome = notaFiscal.Cliente?.Nome,
                FornecedorId = notaFiscal.FornecedorId,
                FornecedorNome = notaFiscal.Fornecedor?.Nome,
                VendaId = notaFiscal.VendaId,
                VendaNumero = notaFiscal.Venda?.Numero,
                CompraId = notaFiscal.CompraId,
                CompraNumero = notaFiscal.Compra?.NumeroCompra,
                ValorProdutos = notaFiscal.ValorProdutos,
                ValorFrete = notaFiscal.ValorFrete,
                ValorSeguro = notaFiscal.ValorSeguro,
                ValorDesconto = notaFiscal.ValorDesconto,
                ValorOutrasDespesas = notaFiscal.ValorOutrasDespesas,
                ValorIPI = notaFiscal.ValorIPI,
                ValorICMS = notaFiscal.ValorICMS,
                ValorPIS = notaFiscal.ValorPIS,
                ValorCOFINS = notaFiscal.ValorCOFINS,
                ValorTotal = notaFiscal.ValorTotal,
                NaturezaOperacao = notaFiscal.NaturezaOperacao,
                CFOP = notaFiscal.CFOP,
                ChaveAcesso = notaFiscal.ChaveAcesso,
                Protocolo = notaFiscal.Protocolo,
                DataAutorizacao = notaFiscal.DataAutorizacao,
                InformacaoComplementar = notaFiscal.InformacaoComplementar,
                ObservacaoFisco = notaFiscal.ObservacaoFisco,
                DataCriacao = notaFiscal.DataCriacao,
                DataAlteracao = notaFiscal.DataAlteracao,
                Itens = notaFiscal.Itens.Select(item => new NotaFiscalItemViewModel
                {
                    Id = item.Id,
                    ProdutoId = item.ProdutoId,
                    ProdutoNome = item.Produto.Descricao,
                    ProdutoCodigo = item.Produto.Codigo,
                    Descricao = item.Descricao,
                    Unidade = item.Unidade,
                    Quantidade = item.Quantidade,
                    ValorUnitario = item.ValorUnitario,
                    ValorTotal = item.ValorTotal,
                    ValorDesconto = item.ValorDesconto,
                    CFOP = item.CFOP,
                    NCM = item.NCM,
                    CST = item.CST,
                    AliquotaICMS = item.AliquotaICMS,
                    ValorICMS = item.ValorICMS,
                    AliquotaIPI = item.AliquotaIPI,
                    ValorIPI = item.ValorIPI,
                    AliquotaPIS = item.AliquotaPIS,
                    ValorPIS = item.ValorPIS,
                    AliquotaCOFINS = item.AliquotaCOFINS,
                    ValorCOFINS = item.ValorCOFINS
                }).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Create(int? vendaId = null, int? compraId = null)
        {
            var empresaId = ObterEmpresaId();
            
            var viewModel = new NotaFiscalViewModel
            {
                EmpresaId = empresaId,
                DataEmissao = DateTime.Now,
                DataSaida = DateTime.Now,
                Serie = "1"
            };

            // Se for criação a partir de venda
            if (vendaId.HasValue)
            {
                var venda = await _vendaService.ObterPorIdAsync(vendaId.Value, empresaId);
                if (venda != null)
                {
                    viewModel.VendaId = venda.Id;
                    viewModel.VendaNumero = venda.Numero;
                    viewModel.Tipo = "Saida";
                    viewModel.ClienteId = venda.ClienteId;
                    viewModel.NaturezaOperacao = "Venda";
                    viewModel.CFOP = "5102";
                }
            }

            // Se for criação a partir de compra
            if (compraId.HasValue)
            {
                var compra = await _compraService.ObterPorIdAsync(compraId.Value, empresaId);
                if (compra != null)
                {
                    viewModel.CompraId = compra.Id;
                    viewModel.CompraNumero = compra.NumeroCompra;
                    viewModel.Tipo = "Entrada";
                    viewModel.FornecedorId = compra.FornecedorId;
                    viewModel.NaturezaOperacao = "Compra";
                    viewModel.CFOP = "1102";
                }
            }

            await CarregarViewBags(empresaId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NotaFiscalViewModel viewModel)
        {
            var empresaId = ObterEmpresaId();

            if (ModelState.IsValid)
            {
                try
                {
                    var notaFiscal = new NotaFiscal
                    {
                        EmpresaId = empresaId,
                        Numero = viewModel.Numero,
                        Serie = viewModel.Serie,
                        Tipo = viewModel.Tipo,
                        Status = "Rascunho",
                        DataEmissao = viewModel.DataEmissao,
                        DataSaida = viewModel.DataSaida,
                        ClienteId = viewModel.ClienteId,
                        FornecedorId = viewModel.FornecedorId,
                        VendaId = viewModel.VendaId,
                        CompraId = viewModel.CompraId,
                        ValorFrete = viewModel.ValorFrete,
                        ValorSeguro = viewModel.ValorSeguro,
                        ValorDesconto = viewModel.ValorDesconto,
                        ValorOutrasDespesas = viewModel.ValorOutrasDespesas,
                        NaturezaOperacao = viewModel.NaturezaOperacao,
                        CFOP = viewModel.CFOP,
                        InformacaoComplementar = viewModel.InformacaoComplementar,
                        ObservacaoFisco = viewModel.ObservacaoFisco,
                        UsuarioCriacaoId = 1 // TODO: Obter do contexto
                    };

                    // Adicionar itens
                    foreach (var itemViewModel in viewModel.Itens)
                    {
                        var item = new NotaFiscalItem
                        {
                            ProdutoId = itemViewModel.ProdutoId,
                            Descricao = itemViewModel.Descricao,
                            Unidade = itemViewModel.Unidade,
                            Quantidade = itemViewModel.Quantidade,
                            ValorUnitario = itemViewModel.ValorUnitario,
                            ValorDesconto = itemViewModel.ValorDesconto,
                            CFOP = itemViewModel.CFOP,
                            NCM = itemViewModel.NCM,
                            CST = itemViewModel.CST,
                            AliquotaICMS = itemViewModel.AliquotaICMS,
                            AliquotaIPI = itemViewModel.AliquotaIPI,
                            AliquotaPIS = itemViewModel.AliquotaPIS,
                            AliquotaCOFINS = itemViewModel.AliquotaCOFINS
                        };

                        item.CalcularValores();
                        notaFiscal.Itens.Add(item);
                    }

                    await _notaFiscalService.CriarAsync(notaFiscal);

                    TempData["Sucesso"] = "Nota fiscal criada com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = notaFiscal.Id });
                }
                catch (Exception ex)
                {
                    TempData["Erro"] = $"Erro ao criar nota fiscal: {ex.Message}";
                }
            }

            await CarregarViewBags(empresaId);
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var empresaId = ObterEmpresaId();
            var notaFiscal = await _notaFiscalService.ObterPorIdAsync(id, empresaId);

            if (notaFiscal == null)
                return NotFound();

            if (!notaFiscal.PodeEditar())
            {
                TempData["Erro"] = "Esta nota fiscal não pode ser editada.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var viewModel = new NotaFiscalViewModel
            {
                Id = notaFiscal.Id,
                EmpresaId = notaFiscal.EmpresaId,
                Numero = notaFiscal.Numero,
                Serie = notaFiscal.Serie,
                Tipo = notaFiscal.Tipo,
                Status = notaFiscal.Status,
                DataEmissao = notaFiscal.DataEmissao,
                DataSaida = notaFiscal.DataSaida,
                ClienteId = notaFiscal.ClienteId,
                FornecedorId = notaFiscal.FornecedorId,
                VendaId = notaFiscal.VendaId,
                CompraId = notaFiscal.CompraId,
                ValorFrete = notaFiscal.ValorFrete,
                ValorSeguro = notaFiscal.ValorSeguro,
                ValorDesconto = notaFiscal.ValorDesconto,
                ValorOutrasDespesas = notaFiscal.ValorOutrasDespesas,
                NaturezaOperacao = notaFiscal.NaturezaOperacao,
                CFOP = notaFiscal.CFOP,
                InformacaoComplementar = notaFiscal.InformacaoComplementar,
                ObservacaoFisco = notaFiscal.ObservacaoFisco,
                Itens = notaFiscal.Itens.Select(item => new NotaFiscalItemViewModel
                {
                    Id = item.Id,
                    ProdutoId = item.ProdutoId,
                    Descricao = item.Descricao,
                    Unidade = item.Unidade,
                    Quantidade = item.Quantidade,
                    ValorUnitario = item.ValorUnitario,
                    ValorDesconto = item.ValorDesconto,
                    CFOP = item.CFOP,
                    NCM = item.NCM,
                    CST = item.CST,
                    AliquotaICMS = item.AliquotaICMS,
                    AliquotaIPI = item.AliquotaIPI,
                    AliquotaPIS = item.AliquotaPIS,
                    AliquotaCOFINS = item.AliquotaCOFINS
                }).ToList()
            };

            await CarregarViewBags(empresaId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NotaFiscalViewModel viewModel)
        {
            if (id != viewModel.Id)
                return NotFound();

            var empresaId = ObterEmpresaId();

            if (ModelState.IsValid)
            {
                try
                {
                    var notaFiscal = await _notaFiscalService.ObterPorIdAsync(id, empresaId);
                    if (notaFiscal == null)
                        return NotFound();

                    // Atualizar propriedades
                    notaFiscal.DataEmissao = viewModel.DataEmissao;
                    notaFiscal.DataSaida = viewModel.DataSaida;
                    notaFiscal.ClienteId = viewModel.ClienteId;
                    notaFiscal.FornecedorId = viewModel.FornecedorId;
                    notaFiscal.ValorFrete = viewModel.ValorFrete;
                    notaFiscal.ValorSeguro = viewModel.ValorSeguro;
                    notaFiscal.ValorDesconto = viewModel.ValorDesconto;
                    notaFiscal.ValorOutrasDespesas = viewModel.ValorOutrasDespesas;
                    notaFiscal.NaturezaOperacao = viewModel.NaturezaOperacao;
                    notaFiscal.CFOP = viewModel.CFOP;
                    notaFiscal.InformacaoComplementar = viewModel.InformacaoComplementar;
                    notaFiscal.ObservacaoFisco = viewModel.ObservacaoFisco;
                    notaFiscal.UsuarioAlteracaoId = 1; // TODO: Obter do contexto

                    // Atualizar itens
                    notaFiscal.Itens.Clear();
                    foreach (var itemViewModel in viewModel.Itens)
                    {
                        var item = new NotaFiscalItem
                        {
                            ProdutoId = itemViewModel.ProdutoId,
                            Descricao = itemViewModel.Descricao,
                            Unidade = itemViewModel.Unidade,
                            Quantidade = itemViewModel.Quantidade,
                            ValorUnitario = itemViewModel.ValorUnitario,
                            ValorDesconto = itemViewModel.ValorDesconto,
                            CFOP = itemViewModel.CFOP,
                            NCM = itemViewModel.NCM,
                            CST = itemViewModel.CST,
                            AliquotaICMS = itemViewModel.AliquotaICMS,
                            AliquotaIPI = itemViewModel.AliquotaIPI,
                            AliquotaPIS = itemViewModel.AliquotaPIS,
                            AliquotaCOFINS = itemViewModel.AliquotaCOFINS
                        };

                        item.CalcularValores();
                        notaFiscal.Itens.Add(item);
                    }

                    await _notaFiscalService.AtualizarAsync(notaFiscal);

                    TempData["Sucesso"] = "Nota fiscal atualizada com sucesso!";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (Exception ex)
                {
                    TempData["Erro"] = $"Erro ao atualizar nota fiscal: {ex.Message}";
                }
            }

            await CarregarViewBags(empresaId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Autorizar(int id)
        {
            var empresaId = ObterEmpresaId();
            
            try
            {
                var sucesso = await _notaFiscalService.AutorizarAsync(id, empresaId);
                
                if (sucesso)
                    TempData["Sucesso"] = "Nota fiscal autorizada com sucesso!";
                else
                    TempData["Erro"] = "Não foi possível autorizar a nota fiscal. Verifique os dados.";
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao autorizar nota fiscal: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id, string justificativa)
        {
            var empresaId = ObterEmpresaId();
            
            if (string.IsNullOrEmpty(justificativa))
            {
                TempData["Erro"] = "Justificativa é obrigatória para cancelamento.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                var sucesso = await _notaFiscalService.CancelarAsync(id, justificativa, empresaId);
                
                if (sucesso)
                    TempData["Sucesso"] = "Nota fiscal cancelada com sucesso!";
                else
                    TempData["Erro"] = "Não foi possível cancelar a nota fiscal.";
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao cancelar nota fiscal: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var empresaId = ObterEmpresaId();
            
            try
            {
                var sucesso = await _notaFiscalService.ExcluirAsync(id, empresaId);
                
                if (sucesso)
                    TempData["Sucesso"] = "Nota fiscal excluída com sucesso!";
                else
                    TempData["Erro"] = "Não foi possível excluir a nota fiscal.";
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao excluir nota fiscal: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Ações AJAX
        [HttpPost]
        public async Task<IActionResult> CriarDeVenda(int vendaId)
        {
            var empresaId = ObterEmpresaId();
            
            try
            {
                var notaFiscal = await _notaFiscalService.CriarDeVendaAsync(vendaId, empresaId);
                return Json(new { success = true, notaFiscalId = notaFiscal.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CriarDeCompra(int compraId)
        {
            var empresaId = ObterEmpresaId();
            
            try
            {
                var notaFiscal = await _notaFiscalService.CriarDeCompraAsync(compraId, empresaId);
                return Json(new { success = true, notaFiscalId = notaFiscal.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GerarProximoNumero(string serie, string tipo)
        {
            var empresaId = ObterEmpresaId();
            var numero = await _notaFiscalService.GerarProximoNumeroAsync(serie, tipo, empresaId);
            return Json(new { numero });
        }

        private async Task CarregarViewBags(int empresaId)
        {
            var clientes = await _clienteService.ObterTodosAsync(empresaId);
            ViewBag.Clientes = new SelectList(clientes, "Id", "Nome");

            var fornecedores = await _fornecedorService.ObterTodosAsync(empresaId);
            ViewBag.Fornecedores = new SelectList(fornecedores, "Id", "Nome");

            var produtos = await _produtoService.ObterTodosAsync(empresaId);
            ViewBag.Produtos = new SelectList(produtos, "Id", "Descricao");

            ViewBag.TiposNota = new SelectList(new[]
            {
                new { Value = "Entrada", Text = "Entrada" },
                new { Value = "Saida", Text = "Saída" }
            }, "Value", "Text");

            ViewBag.StatusNota = new SelectList(new[]
            {
                new { Value = "Rascunho", Text = "Rascunho" },
                new { Value = "Autorizada", Text = "Autorizada" },
                new { Value = "Cancelada", Text = "Cancelada" }
            }, "Value", "Text");
        }
    }
}