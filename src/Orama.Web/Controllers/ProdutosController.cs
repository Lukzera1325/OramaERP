using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orama.Application.Services;
using Orama.Domain.Entities;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller para gerenciamento de produtos
/// </summary>
public class ProdutosController : BaseController
{
    private readonly IProdutoService _produtoService;
    private readonly ICategoriaService _categoriaService;

    public ProdutosController(IProdutoService produtoService, ICategoriaService categoriaService)
    {
        _produtoService = produtoService;
        _categoriaService = categoriaService;
    }

    /// <summary>
    /// Lista todos os produtos
    /// </summary>
    public async Task<IActionResult> Index(string? busca)
    {
        if (!TemPermissao("Produtos.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        IEnumerable<Produto> produtos;
        
        if (!string.IsNullOrWhiteSpace(busca))
        {
            produtos = await _produtoService.BuscarAsync(busca);
            ViewBag.Busca = busca;
        }
        else
        {
            produtos = await _produtoService.ObterTodosAsync(empresaId);
        }

        return View(produtos);
    }

    /// <summary>
    /// Exibe detalhes de um produto
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        if (!TemPermissao("Produtos.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var produto = await _produtoService.ObterPorIdAsync(id, empresaId);
        if (produto == null)
        {
            TempData["Erro"] = "Produto não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        return View(produto);
    }

    /// <summary>
    /// Exibe formulário de criação
    /// </summary>
    public async Task<IActionResult> Create()
    {
        if (!TemPermissao("Produtos.Incluir"))
            return RedirectToAction("AccessDenied", "Auth");

        var viewModel = new ProdutoViewModel();
        await CarregarCategoriasAsync(viewModel);
        
        return View(viewModel);
    }

    /// <summary>
    /// Processa criação de produto
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProdutoViewModel viewModel)
    {
        if (!TemPermissao("Produtos.Incluir"))
            return RedirectToAction("AccessDenied", "Auth");

        if (ModelState.IsValid)
        {
            try
            {
                var empresaId = UsuarioLogado?.EmpresaId ?? 0;
                var produto = new Produto
                {
                    Codigo = viewModel.Codigo,
                    Descricao = viewModel.Descricao,
                    DescricaoDetalhada = viewModel.DescricaoDetalhada,
                    Unidade = viewModel.Unidade,
                    Ncm = viewModel.Ncm,
                    Cest = viewModel.Cest,
                    PrecoCusto = viewModel.PrecoCusto,
                    PrecoVenda = viewModel.PrecoVenda,
                    MargemLucro = viewModel.MargemLucro,
                    ControlaEstoque = viewModel.ControlaEstoque,
                    EstoqueAtual = viewModel.EstoqueAtual,
                    EstoqueMinimo = viewModel.EstoqueMinimo,
                    EstoqueMaximo = viewModel.EstoqueMaximo,
                    Peso = viewModel.Peso,
                    Altura = viewModel.Altura,
                    Largura = viewModel.Largura,
                    Profundidade = viewModel.Profundidade,
                    CategoriaId = viewModel.CategoriaId,
                    Observacoes = viewModel.Observacoes,
                    EmpresaId = empresaId,
                    DataCriacao = DateTime.UtcNow,
                    Ativo = true
                };

                await _produtoService.CriarAsync(produto);
                TempData["Sucesso"] = "Produto cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }

        await CarregarCategoriasAsync(viewModel);
        return View(viewModel);
    }

    /// <summary>
    /// Exibe formulário de edição
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        if (!TemPermissao("Produtos.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var produto = await _produtoService.ObterPorIdAsync(id, empresaId);
        if (produto == null)
        {
            TempData["Erro"] = "Produto não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new ProdutoViewModel
        {
            Id = produto.Id,
            Codigo = produto.Codigo,
            Descricao = produto.Descricao,
            DescricaoDetalhada = produto.DescricaoDetalhada,
            Unidade = produto.Unidade,
            Ncm = produto.Ncm,
            Cest = produto.Cest,
            PrecoCusto = produto.PrecoCusto,
            PrecoVenda = produto.PrecoVenda,
            MargemLucro = produto.MargemLucro,
            ControlaEstoque = produto.ControlaEstoque,
            EstoqueAtual = produto.EstoqueAtual,
            EstoqueMinimo = produto.EstoqueMinimo,
            EstoqueMaximo = produto.EstoqueMaximo,
            Peso = produto.Peso,
            Altura = produto.Altura,
            Largura = produto.Largura,
            Profundidade = produto.Profundidade,
            CategoriaId = produto.CategoriaId,
            Observacoes = produto.Observacoes
        };

        await CarregarCategoriasAsync(viewModel);
        return View(viewModel);
    }

    /// <summary>
    /// Processa edição de produto
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProdutoViewModel viewModel)
    {
        if (!TemPermissao("Produtos.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        if (id != viewModel.Id)
        {
            TempData["Erro"] = "Produto não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            try
            {
                var empresaId = UsuarioLogado?.EmpresaId ?? 0;
                var produto = new Produto
                {
                    Id = viewModel.Id,
                    Codigo = viewModel.Codigo,
                    Descricao = viewModel.Descricao,
                    DescricaoDetalhada = viewModel.DescricaoDetalhada,
                    Unidade = viewModel.Unidade,
                    Ncm = viewModel.Ncm,
                    Cest = viewModel.Cest,
                    PrecoCusto = viewModel.PrecoCusto,
                    PrecoVenda = viewModel.PrecoVenda,
                    MargemLucro = viewModel.MargemLucro,
                    ControlaEstoque = viewModel.ControlaEstoque,
                    EstoqueAtual = viewModel.EstoqueAtual,
                    EstoqueMinimo = viewModel.EstoqueMinimo,
                    EstoqueMaximo = viewModel.EstoqueMaximo,
                    Peso = viewModel.Peso,
                    Altura = viewModel.Altura,
                    Largura = viewModel.Largura,
                    Profundidade = viewModel.Profundidade,
                    CategoriaId = viewModel.CategoriaId,
                    Observacoes = viewModel.Observacoes,
                    EmpresaId = empresaId
                };

                await _produtoService.AtualizarAsync(produto);
                TempData["Sucesso"] = "Produto atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }

        await CarregarCategoriasAsync(viewModel);
        return View(viewModel);
    }

    /// <summary>
    /// Exibe confirmação de exclusão
    /// </summary>
    public async Task<IActionResult> Delete(int id)
    {
        if (!TemPermissao("Produtos.Excluir"))
            return RedirectToAction("AccessDenied", "Auth");

        var empresaId = UsuarioLogado?.EmpresaId ?? 0;
        var produto = await _produtoService.ObterPorIdAsync(id, empresaId);
        if (produto == null)
        {
            TempData["Erro"] = "Produto não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        return View(produto);
    }

    /// <summary>
    /// Processa exclusão de produto
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!TemPermissao("Produtos.Excluir"))
            return RedirectToAction("AccessDenied", "Auth");

        await _produtoService.ExcluirAsync(id);
        TempData["Sucesso"] = "Produto excluído com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// API para verificar se código existe
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> VerificarCodigo(string codigo, int? id)
    {
        var existe = await _produtoService.CodigoExisteAsync(codigo, id);
        return Json(new { existe });
    }

    /// <summary>
    /// API para calcular margem de lucro
    /// </summary>
    [HttpGet]
    public IActionResult CalcularMargem(decimal precoCusto, decimal precoVenda)
    {
        if (precoCusto <= 0)
            return Json(new { margem = 0 });

        var margem = ((precoVenda - precoCusto) / precoCusto) * 100;
        return Json(new { margem = Math.Round(margem, 2) });
    }

    /// <summary>
    /// API para calcular preço de venda baseado na margem
    /// </summary>
    [HttpGet]
    public IActionResult CalcularPrecoVenda(decimal precoCusto, decimal margem)
    {
        if (precoCusto <= 0)
            return Json(new { precoVenda = 0 });

        var precoVenda = precoCusto * (1 + (margem / 100));
        return Json(new { precoVenda = Math.Round(precoVenda, 2) });
    }

    /// <summary>
    /// Lista produtos com estoque baixo
    /// </summary>
    public async Task<IActionResult> EstoqueBaixo()
    {
        if (!TemPermissao("Produtos.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var produtos = await _produtoService.ObterProdutosEstoqueBaixoAsync();
        return View("Index", produtos);
    }

    /// <summary>
    /// Carrega categorias para dropdown
    /// </summary>
    private async Task CarregarCategoriasAsync(ProdutoViewModel viewModel)
    {
        var categorias = await _categoriaService.ObterTodosAsync();
        viewModel.Categorias = new SelectList(categorias, "Id", "Nome", viewModel.CategoriaId);
    }
}
