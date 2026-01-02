using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orama.Application.Services;
using Orama.Web.Models.Api;
using System.Security.Claims;

namespace Orama.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requer autenticação JWT
public class VendasApiController : ControllerBase
{
    private readonly IVendaService _vendaService;
    private readonly IClienteService _clienteService;
    private readonly IProdutoService _produtoService;
    private readonly ILogger<VendasApiController> _logger;

    public VendasApiController(
        IVendaService vendaService,
        IClienteService clienteService,
        IProdutoService produtoService,
        ILogger<VendasApiController> logger)
    {
        _vendaService = vendaService;
        _clienteService = clienteService;
        _produtoService = produtoService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém lista paginada de vendas
    /// </summary>
    /// <param name="page">Página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 50, máximo: 100)</param>
    /// <param name="clienteId">Filtro por cliente (opcional)</param>
    /// <param name="dataInicio">Data inicial (opcional)</param>
    /// <param name="dataFim">Data final (opcional)</param>
    /// <param name="status">Filtro por status (opcional)</param>
    /// <param name="lastSync">Data da última sincronização (opcional)</param>
    /// <returns>Lista paginada de vendas</returns>
    [HttpGet]
    public async Task<IActionResult> GetVendas(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] int? clienteId = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null,
        [FromQuery] string? status = null,
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

            var vendas = await _vendaService.ObterTodosAsync(empresaId);

            // Aplicar filtros
            if (clienteId.HasValue)
            {
                vendas = vendas.Where(v => v.ClienteId == clienteId.Value);
            }

            if (dataInicio.HasValue)
            {
                vendas = vendas.Where(v => v.DataVenda >= dataInicio.Value);
            }

            if (dataFim.HasValue)
            {
                vendas = vendas.Where(v => v.DataVenda <= dataFim.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                vendas = vendas.Where(v => v.Status.ToString() == status);
            }

            // Filtro de sincronização incremental
            if (lastSync.HasValue)
            {
                vendas = vendas.Where(v => v.DataModificacao > lastSync.Value);
            }

            var totalRecords = vendas.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var vendasPaginadas = vendas
                .OrderByDescending(v => v.DataVenda)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new VendaApiDto
                {
                    Id = v.Id,
                    Numero = v.Numero,
                    ClienteId = v.ClienteId,
                    ClienteNome = v.Cliente?.Nome ?? "",
                    VendedorId = v.VendedorId,
                    DataVenda = v.DataVenda,
                    DataEntrega = v.DataEntrega,
                    Status = v.Status.ToString(),
                    SubTotal = v.SubTotal,
                    ValorDesconto = v.ValorDesconto,
                    PercentualDesconto = v.PercentualDesconto,
                    ValorFrete = v.ValorFrete,
                    ValorTotal = v.ValorTotal,
                    FormaPagamento = v.FormaPagamento.ToString(),
                    Parcelas = v.Parcelas,
                    Observacoes = v.Observacoes,
                    DataCriacao = v.DataCriacao,
                    DataModificacao = v.DataModificacao,
                    Itens = v.Itens?.Select(i => new VendaItemApiDto
                    {
                        Id = i.Id,
                        ProdutoId = i.ProdutoId,
                        ProdutoNome = i.Produto?.Descricao ?? "",
                        ProdutoCodigo = i.Produto?.Codigo ?? "",
                        Quantidade = i.Quantidade,
                        PrecoUnitario = i.PrecoUnitario,
                        PercentualDesconto = i.PercentualDesconto,
                        ValorDesconto = i.ValorDesconto,
                        ValorTotal = i.ValorTotal,
                        Observacoes = i.Observacoes
                    }).ToList() ?? new List<VendaItemApiDto>()
                })
                .ToList();

            var response = new PagedApiResponse<VendaApiDto>
            {
                Success = true,
                Message = "Vendas obtidas com sucesso",
                Data = vendasPaginadas,
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter vendas");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Obtém venda por ID
    /// </summary>
    /// <param name="id">ID da venda</param>
    /// <returns>Dados da venda</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVenda(int id)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            var venda = await _vendaService.ObterPorIdAsync(id, empresaId);

            if (venda == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Venda não encontrada"
                });
            }

            var vendaDto = new VendaApiDto
            {
                Id = venda.Id,
                Numero = venda.Numero,
                ClienteId = venda.ClienteId,
                ClienteNome = venda.Cliente?.Nome ?? "",
                VendedorId = venda.VendedorId,
                DataVenda = venda.DataVenda,
                DataEntrega = venda.DataEntrega,
                Status = venda.Status.ToString(),
                SubTotal = venda.SubTotal,
                ValorDesconto = venda.ValorDesconto,
                PercentualDesconto = venda.PercentualDesconto,
                ValorFrete = venda.ValorFrete,
                ValorTotal = venda.ValorTotal,
                FormaPagamento = venda.FormaPagamento.ToString(),
                Parcelas = venda.Parcelas,
                Observacoes = venda.Observacoes,
                DataCriacao = venda.DataCriacao,
                DataModificacao = venda.DataModificacao,
                Itens = venda.Itens?.Select(i => new VendaItemApiDto
                {
                    Id = i.Id,
                    ProdutoId = i.ProdutoId,
                    ProdutoNome = i.Produto?.Descricao ?? "",
                    ProdutoCodigo = i.Produto?.Codigo ?? "",
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario,
                    PercentualDesconto = i.PercentualDesconto,
                    ValorDesconto = i.ValorDesconto,
                    ValorTotal = i.ValorTotal,
                    Observacoes = i.Observacoes
                }).ToList() ?? new List<VendaItemApiDto>()
            };

            return Ok(new ApiResponse<VendaApiDto>
            {
                Success = true,
                Message = "Venda obtida com sucesso",
                Data = vendaDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter venda {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Cria nova venda
    /// </summary>
    /// <param name="vendaDto">Dados da venda</param>
    /// <returns>Venda criada</returns>
    [HttpPost]
    public async Task<IActionResult> CreateVenda([FromBody] VendaCreateApiDto vendaDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Dados inválidos",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var empresaId = GetCurrentEmpresaId();
            var vendedorId = GetCurrentUserId();

            // Verificar se cliente existe
            var cliente = await _clienteService.ObterPorIdAsync(vendaDto.ClienteId, empresaId);
            if (cliente == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Cliente não encontrado"
                });
            }

            // Criar venda
            var venda = new Domain.Entities.Venda
            {
                ClienteId = vendaDto.ClienteId,
                VendedorId = vendedorId,
                DataVenda = vendaDto.DataVenda,
                DataEntrega = vendaDto.DataEntrega,
                Status = Enum.Parse<Domain.Entities.StatusVenda>(vendaDto.Status),
                ValorDesconto = vendaDto.ValorDesconto,
                PercentualDesconto = vendaDto.PercentualDesconto,
                ValorFrete = vendaDto.ValorFrete,
                FormaPagamento = Enum.Parse<Domain.Entities.FormaPagamento>(vendaDto.FormaPagamento),
                Parcelas = vendaDto.Parcelas,
                Observacoes = vendaDto.Observacoes,
                EmpresaId = empresaId
            };

            // Adicionar itens
            if (vendaDto.Itens?.Any() == true)
            {
                foreach (var itemDto in vendaDto.Itens)
                {
                    var produto = await _produtoService.ObterPorIdAsync(itemDto.ProdutoId, empresaId);
                    if (produto == null)
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = $"Produto com ID {itemDto.ProdutoId} não encontrado"
                        });
                    }

                    var item = new Domain.Entities.VendaItem
                    {
                        ProdutoId = itemDto.ProdutoId,
                        Quantidade = itemDto.Quantidade,
                        PrecoUnitario = itemDto.PrecoUnitario,
                        PercentualDesconto = itemDto.PercentualDesconto,
                        Observacoes = itemDto.Observacoes
                    };

                    // Calcular valores do item
                    item.ValorDesconto = item.PrecoUnitario * item.PercentualDesconto / 100;
                    item.ValorTotal = item.Quantidade * (item.PrecoUnitario - item.ValorDesconto);

                    venda.Itens.Add(item);
                }
            }

            var vendaCriada = await _vendaService.CriarAsync(venda);

            var response = new VendaApiDto
            {
                Id = vendaCriada.Id,
                Numero = vendaCriada.Numero,
                ClienteId = vendaCriada.ClienteId,
                ClienteNome = vendaCriada.Cliente?.Nome ?? "",
                VendedorId = vendaCriada.VendedorId,
                DataVenda = vendaCriada.DataVenda,
                DataEntrega = vendaCriada.DataEntrega,
                Status = vendaCriada.Status.ToString(),
                SubTotal = vendaCriada.SubTotal,
                ValorDesconto = vendaCriada.ValorDesconto,
                PercentualDesconto = vendaCriada.PercentualDesconto,
                ValorFrete = vendaCriada.ValorFrete,
                ValorTotal = vendaCriada.ValorTotal,
                FormaPagamento = vendaCriada.FormaPagamento.ToString(),
                Parcelas = vendaCriada.Parcelas,
                Observacoes = vendaCriada.Observacoes,
                DataCriacao = vendaCriada.DataCriacao,
                DataModificacao = vendaCriada.DataModificacao,
                Itens = vendaCriada.Itens?.Select(i => new VendaItemApiDto
                {
                    Id = i.Id,
                    ProdutoId = i.ProdutoId,
                    ProdutoNome = i.Produto?.Descricao ?? "",
                    ProdutoCodigo = i.Produto?.Codigo ?? "",
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario,
                    PercentualDesconto = i.PercentualDesconto,
                    ValorDesconto = i.ValorDesconto,
                    ValorTotal = i.ValorTotal,
                    Observacoes = i.Observacoes
                }).ToList() ?? new List<VendaItemApiDto>()
            };

            return CreatedAtAction(nameof(GetVenda), new { id = vendaCriada.Id }, new ApiResponse<VendaApiDto>
            {
                Success = true,
                Message = "Venda criada com sucesso",
                Data = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar venda");
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
            var vendas = await _vendaService.ObterTodosAsync(empresaId);

            var stats = new SyncStatsApiDto
            {
                TotalRecords = vendas.Count(),
                LastSyncDate = lastSync,
                NewRecords = lastSync.HasValue ? vendas.Count(v => v.DataCriacao > lastSync.Value) : vendas.Count(),
                UpdatedRecords = lastSync.HasValue ? vendas.Count(v => v.DataModificacao > lastSync.Value && v.DataCriacao <= lastSync.Value) : 0,
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

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}