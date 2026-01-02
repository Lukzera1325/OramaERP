using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orama.Application.Services;
using Orama.Domain.Entities;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller para controle de estoque
/// </summary>
public class EstoqueController : BaseController
{
    private readonly IEstoqueService _estoqueService;
    private readonly IProdutoService _produtoService;

    public EstoqueController(IEstoqueService estoqueService, IProdutoService produtoService)
    {
        _estoqueService = estoqueService;
        _produtoService = produtoService;
    }

    /// <summary>
    /// Lista movimentações de estoque
    /// </summary>
    public async Task<IActionResult> Index(MovimentacaoFiltroViewModel? filtro = null)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            IEnumerable<MovimentacaoEstoque> movimentacoes;

            // Aplicar filtros
            if (filtro?.ProdutoId.HasValue == true)
            {
                movimentacoes = await _estoqueService.ObterMovimentacoesPorProdutoAsync(empresaId, filtro.ProdutoId.Value);
            }
            else if (filtro?.Tipo.HasValue == true)
            {
                movimentacoes = await _estoqueService.ObterMovimentacoesPorTipoAsync(empresaId, filtro.Tipo.Value);
            }
            else if (filtro?.DataInicial.HasValue == true && filtro?.DataFinal.HasValue == true)
            {
                movimentacoes = await _estoqueService.ObterMovimentacoesPorPeriodoAsync(empresaId, filtro.DataInicial.Value, filtro.DataFinal.Value);
            }
            else
            {
                movimentacoes = await _estoqueService.ObterMovimentacoesAsync(empresaId);
            }

            var viewModel = movimentacoes.Take(100).Select(m => new MovimentacaoEstoqueViewModel
            {
                Id = m.Id,
                ProdutoId = m.ProdutoId,
                ProdutoNome = m.Produto?.Descricao,
                ProdutoCodigo = m.Produto?.Codigo,
                Tipo = m.Tipo,
                DataMovimentacao = m.DataMovimentacao,
                Quantidade = m.Quantidade,
                EstoqueAnterior = m.EstoqueAnterior,
                EstoquePosterior = m.EstoquePosterior,
                CustoUnitario = m.CustoUnitario,
                Motivo = m.Motivo,
                Observacoes = m.Observacoes,
                UsuarioNome = m.Usuario?.Nome
            }).ToList();

            // Preparar dados para filtros
            await PrepararDadosFiltro(filtro ?? new MovimentacaoFiltroViewModel());

            ViewBag.Filtro = filtro;
            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao carregar movimentações: {ex.Message}";
            return View(new List<MovimentacaoEstoqueViewModel>());
        }
    }

    /// <summary>
    /// Exibe posição atual do estoque
    /// </summary>
    public async Task<IActionResult> Posicao()
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var produtos = await _estoqueService.ObterPosicaoEstoqueAsync(empresaId);

            var viewModel = produtos.Select(p => new PosicaoEstoqueViewModel
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Nome = p.Descricao,
                Categoria = p.CategoriaNavigation?.Nome,
                EstoqueAtual = p.EstoqueAtual,
                EstoqueMinimo = p.EstoqueMinimo,
                EstoqueMaximo = p.EstoqueMaximo,
                PrecoCusto = p.PrecoCusto
            }).ToList();

            ViewBag.ValorTotalEstoque = await _estoqueService.CalcularValorTotalEstoqueAsync(empresaId);
            ViewBag.ProdutosEstoqueBaixo = viewModel.Count(p => p.EstoqueAtual <= p.EstoqueMinimo);
            ViewBag.ProdutosSemEstoque = viewModel.Count(p => p.EstoqueAtual <= 0);

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao carregar posição de estoque: {ex.Message}";
            return View(new List<PosicaoEstoqueViewModel>());
        }
    }

    /// <summary>
    /// Exibe produtos com estoque baixo
    /// </summary>
    public async Task<IActionResult> EstoqueBaixo()
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var produtos = await _estoqueService.ObterProdutosEstoqueBaixoAsync(empresaId);

            var viewModel = produtos.Select(p => new PosicaoEstoqueViewModel
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Nome = p.Descricao,
                Categoria = p.CategoriaNavigation?.Nome,
                EstoqueAtual = p.EstoqueAtual,
                EstoqueMinimo = p.EstoqueMinimo,
                EstoqueMaximo = p.EstoqueMaximo,
                PrecoCusto = p.PrecoCusto
            }).ToList();

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao carregar produtos com estoque baixo: {ex.Message}";
            return View(new List<PosicaoEstoqueViewModel>());
        }
    }

    /// <summary>
    /// Exibe histórico de um produto
    /// </summary>
    public async Task<IActionResult> Historico(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var produto = await _produtoService.ObterPorIdAsync(id, empresaId);
            
            if (produto == null)
            {
                TempData["Erro"] = "Produto não encontrado";
                return RedirectToAction(nameof(Posicao));
            }

            var movimentacoes = await _estoqueService.ObterHistoricoProdutoAsync(empresaId, id);

            var viewModel = movimentacoes.Select(m => new MovimentacaoEstoqueViewModel
            {
                Id = m.Id,
                ProdutoId = m.ProdutoId,
                ProdutoNome = m.Produto?.Descricao,
                ProdutoCodigo = m.Produto?.Codigo,
                Tipo = m.Tipo,
                DataMovimentacao = m.DataMovimentacao,
                Quantidade = m.Quantidade,
                EstoqueAnterior = m.EstoqueAnterior,
                EstoquePosterior = m.EstoquePosterior,
                CustoUnitario = m.CustoUnitario,
                Motivo = m.Motivo,
                Observacoes = m.Observacoes,
                UsuarioNome = m.Usuario?.Nome
            }).ToList();

            ViewBag.Produto = produto;
            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao carregar histórico: {ex.Message}";
            return RedirectToAction(nameof(Posicao));
        }
    }

    /// <summary>
    /// Exibe formulário para ajuste de estoque
    /// </summary>
    public async Task<IActionResult> Ajustar(int? id = null)
    {
        try
        {
            var viewModel = new AjusteEstoqueViewModel();

            if (id.HasValue)
            {
                var empresaId = ObterEmpresaId();
                var produto = await _produtoService.ObterPorIdAsync(id.Value, empresaId);
                
                if (produto != null)
                {
                    viewModel.ProdutoId = produto.Id;
                    viewModel.ProdutoNome = produto.Descricao;
                    viewModel.EstoqueAtual = produto.EstoqueAtual;
                    viewModel.QuantidadeReal = produto.EstoqueAtual;
                }
            }

            await PrepararDadosAjuste(viewModel);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao preparar ajuste: {ex.Message}";
            return RedirectToAction(nameof(Posicao));
        }
    }

    /// <summary>
    /// Processa ajuste de estoque
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ajustar(AjusteEstoqueViewModel viewModel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await PrepararDadosAjuste(viewModel);
                return View(viewModel);
            }

            var empresaId = ObterEmpresaId();
            var usuarioId = UsuarioLogado?.Id ?? 0;

            var movimentacao = await _estoqueService.AjustarEstoqueAsync(
                empresaId, 
                viewModel.ProdutoId, 
                viewModel.QuantidadeReal, 
                viewModel.Motivo, 
                usuarioId);

            if (movimentacao != null)
            {
                TempData["Sucesso"] = $"Ajuste realizado com sucesso! Diferença: {viewModel.Diferenca:N2}";
            }
            else
            {
                TempData["Info"] = "Não houve diferença entre o estoque atual e a contagem física.";
            }

            return RedirectToAction(nameof(Posicao));
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao realizar ajuste: {ex.Message}";
            await PrepararDadosAjuste(viewModel);
            return View(viewModel);
        }
    }

    /// <summary>
    /// Exibe formulário para inventário físico
    /// </summary>
    public async Task<IActionResult> Inventario()
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var produtos = await _estoqueService.ObterProdutosParaInventarioAsync(empresaId);

            var viewModel = new InventarioViewModel
            {
                Itens = produtos.Select(p => new InventarioItemViewModel
                {
                    ProdutoId = p.Id,
                    Codigo = p.Codigo,
                    Nome = p.Descricao,
                    Categoria = p.CategoriaNavigation?.Nome,
                    EstoqueSistema = p.EstoqueAtual,
                    ContagemFisica = p.EstoqueAtual // Inicializar com estoque atual
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao preparar inventário: {ex.Message}";
            return RedirectToAction(nameof(Posicao));
        }
    }

    /// <summary>
    /// Processa inventário físico
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inventario(InventarioViewModel viewModel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var empresaId = ObterEmpresaId();
            var usuarioId = UsuarioLogado?.Id ?? 0;

            // Preparar dicionário com contagem física
            var contagemFisica = viewModel.Itens.ToDictionary(i => i.ProdutoId, i => i.ContagemFisica);

            var movimentacoes = await _estoqueService.ProcessarInventarioAsync(
                empresaId, 
                contagemFisica, 
                usuarioId, 
                viewModel.ObservacoesGerais);

            var totalAjustes = movimentacoes.Count();
            var totalDiferenca = movimentacoes.Sum(m => m.Tipo == TipoMovimentacaoEstoque.AjustePositivo ? 
                m.Quantidade : -m.Quantidade);

            TempData["Sucesso"] = $"Inventário processado com sucesso! {totalAjustes} ajuste(s) realizado(s). Diferença total: {totalDiferenca:N2}";
            return RedirectToAction(nameof(Posicao));
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro ao processar inventário: {ex.Message}";
            return View(viewModel);
        }
    }

    /// <summary>
    /// Obtém dados do produto para ajuste (AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObterDadosProduto(int produtoId)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var produto = await _produtoService.ObterPorIdAsync(produtoId, empresaId);

            if (produto == null)
                return Json(new { success = false, message = "Produto não encontrado" });

            return Json(new
            {
                success = true,
                data = new
                {
                    id = produto.Id,
                    nome = produto.Descricao,
                    codigo = produto.Codigo,
                    estoqueAtual = produto.EstoqueAtual,
                    estoqueMinimo = produto.EstoqueMinimo,
                    estoqueMaximo = produto.EstoqueMaximo
                }
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Erro ao obter dados do produto: {ex.Message}" });
        }
    }

    /// <summary>
    /// Gera relatório de estoque (AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GerarRelatorio()
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var relatorio = await _estoqueService.ObterRelatorioPosicaoEstoqueAsync(empresaId);

            return Json(new { success = true, data = relatorio });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Erro ao gerar relatório: {ex.Message}" });
        }
    }

    #region Métodos Auxiliares

    /// <summary>
    /// Prepara dados para filtros
    /// </summary>
    private async Task PrepararDadosFiltro(MovimentacaoFiltroViewModel filtro)
    {
        var empresaId = ObterEmpresaId();

        // Produtos
        var produtos = await _produtoService.ObterTodosAsync(empresaId);
        var produtosList = produtos.Where(p => p.ControlaEstoque)
            .Select(p => new { Value = p.Id, Text = $"{p.Codigo} - {p.Descricao}" }).ToList();
        produtosList.Insert(0, new { Value = 0, Text = "Todos os produtos" });
        filtro.Produtos = new SelectList(produtosList, "Value", "Text", filtro.ProdutoId);

        // Tipos de movimentação
        var tipos = Enum.GetValues<TipoMovimentacaoEstoque>()
            .Select(t => new { Value = (int)t, Text = GetTipoDescricao(t) })
            .ToList();
        tipos.Insert(0, new { Value = 0, Text = "Todos os tipos" });
        filtro.Tipos = new SelectList(tipos, "Value", "Text", (int?)filtro.Tipo);
    }

    /// <summary>
    /// Prepara dados para ajuste
    /// </summary>
    private async Task PrepararDadosAjuste(AjusteEstoqueViewModel viewModel)
    {
        var empresaId = ObterEmpresaId();

        // Produtos que controlam estoque
        var produtos = await _produtoService.ObterTodosAsync(empresaId);
        var produtosList = produtos.Where(p => p.ControlaEstoque)
            .Select(p => new { Value = p.Id, Text = $"{p.Codigo} - {p.Descricao}" }).ToList();
        viewModel.Produtos = new SelectList(produtosList, "Value", "Text", viewModel.ProdutoId);
    }

    /// <summary>
    /// Obtém descrição do tipo de movimentação
    /// </summary>
    private static string GetTipoDescricao(TipoMovimentacaoEstoque tipo)
    {
        return tipo switch
        {
            TipoMovimentacaoEstoque.EntradaCompra => "Entrada - Compra",
            TipoMovimentacaoEstoque.SaidaVenda => "Saída - Venda",
            TipoMovimentacaoEstoque.AjustePositivo => "Ajuste Positivo",
            TipoMovimentacaoEstoque.AjusteNegativo => "Ajuste Negativo",
            TipoMovimentacaoEstoque.Transferencia => "Transferência",
            TipoMovimentacaoEstoque.Devolucao => "Devolução",
            TipoMovimentacaoEstoque.Perda => "Perda",
            _ => "Desconhecido"
        };
    }

    #endregion
}