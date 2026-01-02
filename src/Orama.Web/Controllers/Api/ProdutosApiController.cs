using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orama.Application.Services;
using Orama.Web.Models.Api;
using System.Security.Claims;

namespace Orama.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requer autenticação JWT
public class ProdutosApiController : ControllerBase
{
    private readonly IProdutoService _produtoService;
    private readonly ILogger<ProdutosApiController> _logger;

    public ProdutosApiController(IProdutoService produtoService, ILogger<ProdutosApiController> logger)
    {
        _produtoService = produtoService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém lista paginada de produtos
    /// </summary>
    /// <param name="page">Página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 50, máximo: 100)</param>
    /// <param name="search">Termo de busca (opcional)</param>
    /// <param name="categoria">Filtro por categoria (opcional)</param>
    /// <param name="ativo">Filtro por status ativo (opcional)</param>
    /// <param name="lastSync">Data da última sincronização (opcional)</param>
    /// <returns>Lista paginada de produtos</returns>
    [HttpGet]
    public async Task<IActionResult> GetProdutos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] string? categoria = null,
        [FromQuery] bool? ativo = null,
        [FromQuery] DateTime? lastSync = null)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == 0)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Empresa não identificada"
                });
            }

            // Validar parâmetros
            page = Math.Max(1, page);
            pageSize = Math.Min(100, Math.Max(1, pageSize));

            var produtos = await _produtoService.ObterTodosAsync(empresaId);

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                produtos = produtos.Where(p => 
                    p.Descricao.ToLower().Contains(searchLower) ||
                    p.Codigo.ToLower().Contains(searchLower) ||
                    (p.CodigoBarras?.Contains(search) ?? false) ||
                    (p.Categoria?.ToLower().Contains(searchLower) ?? false));
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                produtos = produtos.Where(p => p.Categoria == categoria);
            }

            if (ativo.HasValue)
            {
                produtos = produtos.Where(p => p.Ativo == ativo.Value);
            }

            // Filtro de sincronização incremental
            if (lastSync.HasValue)
            {
                produtos = produtos.Where(p => p.DataModificacao > lastSync.Value);
            }

            var totalRecords = produtos.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var produtosPaginados = produtos
                .OrderBy(p => p.Descricao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProdutoApiDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    Descricao = p.Descricao,
                    DescricaoDetalhada = p.DescricaoDetalhada,
                    Categoria = p.Categoria,
                    Unidade = p.Unidade,
                    PrecoCusto = p.PrecoCusto,
                    PrecoVenda = p.PrecoVenda,
                    PrecoMinimo = p.PrecoMinimo,
                    MargemLucro = p.MargemLucro,
                    EstoqueAtual = p.EstoqueAtual,
                    EstoqueMinimo = p.EstoqueMinimo,
                    Peso = p.Peso,
                    CodigoBarras = p.CodigoBarras,
                    Observacoes = p.Observacoes,
                    Ativo = p.Ativo,
                    DataCriacao = p.DataCriacao,
                    DataModificacao = p.DataModificacao
                })
                .ToList();

            var response = new PagedApiResponse<ProdutoApiDto>
            {
                Success = true,
                Message = "Produtos obtidos com sucesso",
                Data = produtosPaginados,
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produtos");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Obtém produto por ID
    /// </summary>
    /// <param name="id">ID do produto</param>
    /// <returns>Dados do produto</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduto(int id)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            var produto = await _produtoService.ObterPorIdAsync(id, empresaId);

            if (produto == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Produto não encontrado"
                });
            }

            var produtoDto = new ProdutoApiDto
            {
                Id = produto.Id,
                Codigo = produto.Codigo,
                Descricao = produto.Descricao,
                DescricaoDetalhada = produto.DescricaoDetalhada,
                Categoria = produto.Categoria,
                Unidade = produto.Unidade,
                PrecoCusto = produto.PrecoCusto,
                PrecoVenda = produto.PrecoVenda,
                PrecoMinimo = produto.PrecoMinimo,
                MargemLucro = produto.MargemLucro,
                EstoqueAtual = produto.EstoqueAtual,
                EstoqueMinimo = produto.EstoqueMinimo,
                Peso = produto.Peso,
                CodigoBarras = produto.CodigoBarras,
                Observacoes = produto.Observacoes,
                Ativo = produto.Ativo,
                DataCriacao = produto.DataCriacao,
                DataModificacao = produto.DataModificacao
            };

            return Ok(new ApiResponse<ProdutoApiDto>
            {
                Success = true,
                Message = "Produto obtido com sucesso",
                Data = produtoDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produto {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Obtém lista de categorias
    /// </summary>
    /// <returns>Lista de categorias</returns>
    [HttpGet("categorias")]
    public async Task<IActionResult> GetCategorias()
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            var produtos = await _produtoService.ObterTodosAsync(empresaId);

            var categorias = produtos
                .Where(p => !string.IsNullOrEmpty(p.Categoria))
                .Select(p => p.Categoria!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return Ok(new ApiResponse<List<string>>
            {
                Success = true,
                Message = "Categorias obtidas com sucesso",
                Data = categorias
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter categorias");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Busca produto por código de barras
    /// </summary>
    /// <param name="codigoBarras">Código de barras</param>
    /// <returns>Produto encontrado</returns>
    [HttpGet("barcode/{codigoBarras}")]
    public async Task<IActionResult> GetProdutoPorCodigoBarras(string codigoBarras)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            var produtos = await _produtoService.ObterTodosAsync(empresaId);
            var produto = produtos.FirstOrDefault(p => p.CodigoBarras == codigoBarras);

            if (produto == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Produto não encontrado"
                });
            }

            var produtoDto = new ProdutoApiDto
            {
                Id = produto.Id,
                Codigo = produto.Codigo,
                Descricao = produto.Descricao,
                DescricaoDetalhada = produto.DescricaoDetalhada,
                Categoria = produto.Categoria,
                Unidade = produto.Unidade,
                PrecoCusto = produto.PrecoCusto,
                PrecoVenda = produto.PrecoVenda,
                PrecoMinimo = produto.PrecoMinimo,
                MargemLucro = produto.MargemLucro,
                EstoqueAtual = produto.EstoqueAtual,
                EstoqueMinimo = produto.EstoqueMinimo,
                Peso = produto.Peso,
                CodigoBarras = produto.CodigoBarras,
                Observacoes = produto.Observacoes,
                Ativo = produto.Ativo,
                DataCriacao = produto.DataCriacao,
                DataModificacao = produto.DataModificacao
            };

            return Ok(new ApiResponse<ProdutoApiDto>
            {
                Success = true,
                Message = "Produto obtido com sucesso",
                Data = produtoDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produto por código de barras {CodigoBarras}", codigoBarras);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Obtém estatísticas de sincronização
    /// </summary>
    /// <param name="lastSync">Data da última sincronização</param>
    /// <returns>Estatísticas de sincronização</returns>
    [HttpGet("sync/stats")]
    public async Task<IActionResult> GetSyncStats([FromQuery] DateTime? lastSync = null)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            var produtos = await _produtoService.ObterTodosAsync(empresaId);

            var stats = new SyncStatsApiDto
            {
                TotalRecords = produtos.Count(),
                LastSyncDate = lastSync,
                NewRecords = lastSync.HasValue ? produtos.Count(p => p.DataCriacao > lastSync.Value) : produtos.Count(),
                UpdatedRecords = lastSync.HasValue ? produtos.Count(p => p.DataModificacao > lastSync.Value && p.DataCriacao <= lastSync.Value) : 0,
                ServerTimestamp = DateTime.UtcNow
            };

            return Ok(new ApiResponse<SyncStatsApiDto>
            {
                Success = true,
                Message = "Estatísticas obtidas com sucesso",
                Data = stats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas de sincronização");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    private int GetCurrentEmpresaId()
    {
        var empresaIdClaim = User.FindFirst("EmpresaId")?.Value;
        return int.TryParse(empresaIdClaim, out var empresaId) ? empresaId : 0;
    }
}