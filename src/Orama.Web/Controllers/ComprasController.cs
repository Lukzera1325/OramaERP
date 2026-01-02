using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orama.Application.Services;
using Orama.Domain.Entities;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller para gerenciamento de compras
/// </summary>
public class ComprasController : BaseController
{
    private readonly ICompraService _compraService;
    private readonly IFornecedorService _fornecedorService;
    private readonly IProdutoService _produtoService;

    public ComprasController(
        ICompraService compraService,
        IFornecedorService fornecedorService,
        IProdutoService produtoService)
    {
        _compraService = compraService;
        _fornecedorService = fornecedorService;
        _produtoService = produtoService;
    }

    /// <summary>
    /// Lista todas as compras
    /// </summary>
    public async Task<IActionResult> Index(CompraFiltroViewModel? filtro = null)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            IEnumerable<Compra> compras;

            // Aplicar filtros
            if (filtro?.FornecedorId.HasValue == true)
            {
                compras = await _compraService.ObterPorFornecedorAsync(empresaId, filtro.FornecedorId.Value);
            }
            else if (filtro?.Status.HasValue == true)
            {
                compras = await _compraService.ObterPorStatusAsync(empresaId, filtro.Status.Value);
            }
            else if (filtro?.DataInicial.HasValue == true && filtro?.DataFinal.HasValue == true)
            {
                compras = await _compraService.ObterPorPeriodoAsync(empresaId, filtro.DataInicial.Value, filtro.DataFinal.Value);
            }
            else
            {
                compras = await _compraService.ObterTodosAsync(empresaId);
            }

            // Aplicar filtros adicionais
            if (!string.IsNullOrEmpty(filtro?.NumeroCompra))
            {
                compras = compras.Where(c => c.NumeroCompra.Contains(filtro.NumeroCompra, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(filtro?.NumeroNF))
            {
                compras = compras.Where(c => !string.IsNullOrEmpty(c.NumeroNF) && 
                                           c.NumeroNF.Contains(filtro.NumeroNF, StringComparison.OrdinalIgnoreCase));
            }

            var viewModel = compras.Select(c => new CompraViewModel
            {
                Id = c.Id,
                NumeroCompra = c.NumeroCompra,
                FornecedorNome = c.Fornecedor?.Nome,
                NumeroNF = c.NumeroNF,
                DataCompra = c.DataCompra,
                DataEntrega = c.DataEntrega,
                DataRecebimento = c.DataRecebimento,
                Status = c.Status,
                ValorTotal = c.ValorTotal,
                DataCriacao = c.DataCriacao
            }).ToList();

            // Preparar dados para filtros
            await PrepararDadosFiltro(filtro ?? new CompraFiltroViewModel());

            ViewBag.Filtro = filtro;
            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao carregar compras: {ex.Message}";
            return View(new List<CompraViewModel>());
        }
    }

    /// <summary>
    /// Exibe detalhes de uma compra
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var compra = await _compraService.ObterPorIdAsync(id, empresaId);

            if (compra == null)
            {
                TempData["Erro"] = "Compra não encontrada";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = await MapearParaViewModel(compra);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao carregar compra: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Exibe formulário para criar nova compra
    /// </summary>
    public async Task<IActionResult> Create()
    {
        try
        {
            var viewModel = new CompraViewModel
            {
                DataCompra = DateTime.Now,
                Parcelas = 1,
                FormaPagamento = FormaPagamento.Boleto
            };

            await PrepararDadosFormulario(viewModel);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao preparar formulário: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Processa criação de nova compra
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CompraViewModel viewModel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await PrepararDadosFormulario(viewModel);
                return View(viewModel);
            }

            var empresaId = ObterEmpresaId();
            var compra = new Compra
            {
                EmpresaId = empresaId,
                FornecedorId = viewModel.FornecedorId,
                NumeroNF = viewModel.NumeroNF,
                DataCompra = viewModel.DataCompra,
                DataEntrega = viewModel.DataEntrega,
                FormaPagamento = viewModel.FormaPagamento,
                Parcelas = viewModel.Parcelas,
                PercentualDesconto = viewModel.PercentualDesconto,
                ValorFrete = viewModel.ValorFrete,
                Observacoes = viewModel.Observacoes
            };

            await _compraService.CriarAsync(compra);

            TempData["Sucesso"] = "Compra criada com sucesso!";
            return RedirectToAction(nameof(Details), new { id = compra.Id });
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao criar compra: {ex.Message}";
            await PrepararDadosFormulario(viewModel);
            return View(viewModel);
        }
    }

    /// <summary>
    /// Exibe formulário para editar compra
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var compra = await _compraService.ObterPorIdAsync(id, empresaId);

            if (compra == null)
            {
                TempData["Erro"] = "Compra não encontrada";
                return RedirectToAction(nameof(Index));
            }

            if (!compra.Status.Equals(StatusCompra.Pedido))
            {
                TempData["Erro"] = "Apenas pedidos podem ser editados";
                return RedirectToAction(nameof(Details), new { id });
            }

            var viewModel = await MapearParaViewModel(compra);
            await PrepararDadosFormulario(viewModel);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao carregar compra: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Processa edição de compra
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CompraViewModel viewModel)
    {
        try
        {
            if (id != viewModel.Id)
            {
                TempData["Erro"] = "ID da compra não confere";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await PrepararDadosFormulario(viewModel);
                return View(viewModel);
            }

            var empresaId = ObterEmpresaId();
            var compra = new Compra
            {
                Id = viewModel.Id,
                EmpresaId = empresaId,
                FornecedorId = viewModel.FornecedorId,
                NumeroNF = viewModel.NumeroNF,
                DataCompra = viewModel.DataCompra,
                DataEntrega = viewModel.DataEntrega,
                FormaPagamento = viewModel.FormaPagamento,
                Parcelas = viewModel.Parcelas,
                PercentualDesconto = viewModel.PercentualDesconto,
                ValorFrete = viewModel.ValorFrete,
                Observacoes = viewModel.Observacoes,
                Itens = viewModel.Itens.Select(i => new CompraItem
                {
                    Id = i.Id,
                    ProdutoId = i.ProdutoId,
                    Quantidade = i.Quantidade,
                    ValorUnitario = i.ValorUnitario,
                    Observacoes = i.Observacoes
                }).ToList()
            };

            await _compraService.AtualizarAsync(compra);

            TempData["Sucesso"] = "Compra atualizada com sucesso!";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao atualizar compra: {ex.Message}";
            await PrepararDadosFormulario(viewModel);
            return View(viewModel);
        }
    }

    /// <summary>
    /// Exibe confirmação para excluir compra
    /// </summary>
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var compra = await _compraService.ObterPorIdAsync(id, empresaId);

            if (compra == null)
            {
                TempData["Erro"] = "Compra não encontrada";
                return RedirectToAction(nameof(Index));
            }

            if (compra.Status == StatusCompra.Recebida)
            {
                TempData["Erro"] = "Não é possível excluir uma compra recebida";
                return RedirectToAction(nameof(Details), new { id });
            }

            var viewModel = await MapearParaViewModel(compra);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao carregar compra: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Processa exclusão de compra
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            await _compraService.ExcluirAsync(id, empresaId);

            TempData["Sucesso"] = "Compra excluída com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao excluir compra: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    /// <summary>
    /// Aprova uma compra
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Aprovar(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            await _compraService.AprovarAsync(id, empresaId);

            return Json(new { success = true, message = "Compra aprovada com sucesso!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Erro ao aprovar compra: {ex.Message}" });
        }
    }

    /// <summary>
    /// Recebe uma compra (entrada no estoque)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Receber(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            await _compraService.ReceberAsync(id, empresaId);

            return Json(new { success = true, message = "Compra recebida com sucesso! Estoque atualizado e contas a pagar geradas." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Erro ao receber compra: {ex.Message}" });
        }
    }

    /// <summary>
    /// Cancela uma compra
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Cancelar(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            await _compraService.CancelarAsync(id, empresaId);

            return Json(new { success = true, message = "Compra cancelada com sucesso!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Erro ao cancelar compra: {ex.Message}" });
        }
    }

    #region Gerenciamento de Itens

    /// <summary>
    /// Obtém itens de uma compra (AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObterItens(int compraId)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var itens = await _compraService.ObterItensCompraAsync(compraId, empresaId);

            var viewModel = itens.Select(i => new CompraItemViewModel
            {
                Id = i.Id,
                CompraId = i.CompraId,
                ProdutoId = i.ProdutoId,
                ProdutoNome = i.Produto?.Descricao,
                ProdutoCodigo = i.Produto?.Codigo,
                Quantidade = i.Quantidade,
                ValorUnitario = i.ValorUnitario,
                ValorTotal = i.ValorTotal,
                Observacoes = i.Observacoes
            }).ToList();

            return Json(new { success = true, data = viewModel });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Erro ao carregar itens: {ex.Message}" });
        }
    }

    /// <summary>
    /// Adiciona item à compra (AJAX)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AdicionarItem([FromBody] CompraItemViewModel viewModel)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var item = new CompraItem
            {
                ProdutoId = viewModel.ProdutoId,
                Quantidade = viewModel.Quantidade,
                ValorUnitario = viewModel.ValorUnitario,
                Observacoes = viewModel.Observacoes
            };

            var itemCriado = await _compraService.AdicionarItemAsync(viewModel.CompraId, item, empresaId);

            return Json(new { success = true, message = "Item adicionado com sucesso!", itemId = itemCriado.Id });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Erro ao adicionar item: {ex.Message}" });
        }
    }

    /// <summary>
    /// Atualiza item da compra (AJAX)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AtualizarItem([FromBody] CompraItemViewModel viewModel)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var item = new CompraItem
            {
                Id = viewModel.Id,
                ProdutoId = viewModel.ProdutoId,
                Quantidade = viewModel.Quantidade,
                ValorUnitario = viewModel.ValorUnitario,
                Observacoes = viewModel.Observacoes
            };

            await _compraService.AtualizarItemAsync(item, empresaId);

            return Json(new { success = true, message = "Item atualizado com sucesso!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Erro ao atualizar item: {ex.Message}" });
        }
    }

    /// <summary>
    /// Remove item da compra (AJAX)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RemoverItem(int itemId)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            await _compraService.RemoverItemAsync(itemId, empresaId);

            return Json(new { success = true, message = "Item removido com sucesso!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Erro ao remover item: {ex.Message}" });
        }
    }

    /// <summary>
    /// Busca produtos para autocomplete (AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> BuscarProdutos(string termo)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var produtos = await _produtoService.ObterTodosAsync(empresaId);

            var produtosFiltrados = produtos
                .Where(p => p.Descricao.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                           p.Codigo.Contains(termo, StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .Select(p => new
                {
                    id = p.Id,
                    nome = p.Descricao,
                    codigo = p.Codigo,
                    preco = p.PrecoVenda,
                    estoque = p.EstoqueAtual
                })
                .ToList();

            return Json(produtosFiltrados);
        }
        catch (Exception ex)
        {
            return Json(new { error = $"Erro ao buscar produtos: {ex.Message}" });
        }
    }

    #endregion

    #region Métodos Auxiliares

    /// <summary>
    /// Mapeia entidade para ViewModel
    /// </summary>
    private async Task<CompraViewModel> MapearParaViewModel(Compra compra)
    {
        var viewModel = new CompraViewModel
        {
            Id = compra.Id,
            NumeroCompra = compra.NumeroCompra,
            FornecedorId = compra.FornecedorId,
            FornecedorNome = compra.Fornecedor?.Nome,
            NumeroNF = compra.NumeroNF,
            DataCompra = compra.DataCompra,
            DataEntrega = compra.DataEntrega,
            DataRecebimento = compra.DataRecebimento,
            Status = compra.Status,
            SubTotal = compra.SubTotal,
            PercentualDesconto = compra.PercentualDesconto,
            ValorDesconto = compra.ValorDesconto,
            ValorFrete = compra.ValorFrete,
            ValorTotal = compra.ValorTotal,
            FormaPagamento = compra.FormaPagamento,
            Parcelas = compra.Parcelas,
            Observacoes = compra.Observacoes,
            DataCriacao = compra.DataCriacao,
            Itens = compra.Itens?.Select(i => new CompraItemViewModel
            {
                Id = i.Id,
                CompraId = i.CompraId,
                ProdutoId = i.ProdutoId,
                ProdutoNome = i.Produto?.Descricao,
                ProdutoCodigo = i.Produto?.Codigo,
                Quantidade = i.Quantidade,
                ValorUnitario = i.ValorUnitario,
                ValorTotal = i.ValorTotal,
                Observacoes = i.Observacoes
            }).ToList() ?? new List<CompraItemViewModel>()
        };

        return viewModel;
    }

    /// <summary>
    /// Prepara dados para formulário
    /// </summary>
    private async Task PrepararDadosFormulario(CompraViewModel viewModel)
    {
        var empresaId = ObterEmpresaId();

        // Fornecedores
        var fornecedores = await _fornecedorService.ObterTodosAsync(empresaId);
        viewModel.Fornecedores = new SelectList(fornecedores, "Id", "RazaoSocial", viewModel.FornecedorId);

        // Formas de pagamento
        var formasPagamento = Enum.GetValues<FormaPagamento>()
            .Select(fp => new { Value = (int)fp, Text = GetFormaPagamentoDescricao(fp) })
            .ToList();
        viewModel.FormasPagamento = new SelectList(formasPagamento, "Value", "Text", (int)viewModel.FormaPagamento);

        // Produtos para itens
        var produtos = await _produtoService.ObterTodosAsync(empresaId);
        foreach (var item in viewModel.Itens)
        {
            item.Produtos = new SelectList(produtos, "Id", "Nome", item.ProdutoId);
        }
    }

    /// <summary>
    /// Prepara dados para os filtros da tela de listagem
    /// </summary>
    private async Task PrepararDadosFiltro(CompraFiltroViewModel filtro)
    {
        var empresaId = ObterEmpresaId();

        // Fornecedores
        var fornecedores = await _fornecedorService.ObterTodosAsync(empresaId);
        var fornecedoresList = fornecedores.Select(f => new { Value = f.Id, Text = f.Nome }).ToList();
        fornecedoresList.Insert(0, new { Value = 0, Text = "Todos os fornecedores" });
        filtro.Fornecedores = new SelectList(fornecedoresList, "Value", "Text", filtro.FornecedorId);

        // Status
        var statusList = Enum.GetValues<StatusCompra>()
            .Select(s => new { Value = (int)s, Text = GetStatusCompraDescricao(s) })
            .ToList();
        statusList.Insert(0, new { Value = 0, Text = "Todos os status" });
        filtro.StatusList = new SelectList(statusList, "Value", "Text", (int?)filtro.Status);
    }

    /// <summary>
    /// Obtém descrição da forma de pagamento
    /// </summary>
    private static string GetFormaPagamentoDescricao(FormaPagamento formaPagamento)
    {
        return formaPagamento switch
        {
            FormaPagamento.Dinheiro => "Dinheiro",
            FormaPagamento.CartaoCredito => "Cartão de Crédito",
            FormaPagamento.CartaoDebito => "Cartão de Débito",
            FormaPagamento.Pix => "PIX",
            FormaPagamento.Boleto => "Boleto",
            FormaPagamento.Transferencia => "Transferência",
            FormaPagamento.Cheque => "Cheque",
            _ => "Não Informado"
        };
    }

    /// <summary>
    /// Obtém descrição do status
    /// </summary>
    private static string GetStatusDescricao(StatusCompra status)
    {
        return status switch
        {
            StatusCompra.Pedido => "Pedido",
            StatusCompra.Aprovada => "Aprovada",
            StatusCompra.Recebida => "Recebida",
            StatusCompra.Cancelada => "Cancelada",
            _ => "Desconhecido"
        };
    }

    /// <summary>
    /// Obtém descrição do status para filtros
    /// </summary>
    private static string GetStatusCompraDescricao(StatusCompra status)
    {
        return GetStatusDescricao(status);
    }

    #endregion
}