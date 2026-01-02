using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orama.Application.Services;
using Orama.Web.Models.Api;
using System.Security.Claims;

namespace Orama.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requer autenticação JWT
public class ClientesApiController : ControllerBase
{
    private readonly IClienteService _clienteService;
    private readonly ILogger<ClientesApiController> _logger;

    public ClientesApiController(IClienteService clienteService, ILogger<ClientesApiController> logger)
    {
        _clienteService = clienteService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém lista paginada de clientes
    /// </summary>
    /// <param name="page">Página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 50, máximo: 100)</param>
    /// <param name="search">Termo de busca (opcional)</param>
    /// <param name="lastSync">Data da última sincronização (opcional)</param>
    /// <returns>Lista paginada de clientes</returns>
    [HttpGet]
    public async Task<IActionResult> GetClientes(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
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

            var clientes = await _clienteService.ObterTodosAsync(empresaId);

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                clientes = clientes.Where(c => 
                    c.Nome.ToLower().Contains(searchLower) ||
                    (c.CpfCnpj?.Contains(search) ?? false) ||
                    (c.Email?.ToLower().Contains(searchLower) ?? false));
            }

            // Filtro de sincronização incremental
            if (lastSync.HasValue)
            {
                clientes = clientes.Where(c => c.DataModificacao > lastSync.Value);
            }

            var totalRecords = clientes.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var clientesPaginados = clientes
                .OrderBy(c => c.Nome)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ClienteApiDto
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    CpfCnpj = c.CpfCnpj,
                    Email = c.Email,
                    Telefone = c.Telefone,
                    Celular = c.Celular,
                    Endereco = c.Endereco,
                    Numero = c.Numero,
                    Complemento = c.Complemento,
                    Bairro = c.Bairro,
                    Cidade = c.Cidade,
                    Estado = c.Estado,
                    Cep = c.Cep,
                    Observacoes = c.Observacoes,
                    LimiteCredito = c.LimiteCredito,
                    Ativo = c.Ativo,
                    DataCriacao = c.DataCriacao,
                    DataModificacao = c.DataModificacao
                })
                .ToList();

            var response = new PagedApiResponse<ClienteApiDto>
            {
                Success = true,
                Message = "Clientes obtidos com sucesso",
                Data = clientesPaginados,
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter clientes");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Obtém cliente por ID
    /// </summary>
    /// <param name="id">ID do cliente</param>
    /// <returns>Dados do cliente</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCliente(int id)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            var cliente = await _clienteService.ObterPorIdAsync(id, empresaId);

            if (cliente == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Cliente não encontrado"
                });
            }

            var clienteDto = new ClienteApiDto
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                CpfCnpj = cliente.CpfCnpj,
                Email = cliente.Email,
                Telefone = cliente.Telefone,
                Celular = cliente.Celular,
                Endereco = cliente.Endereco,
                Numero = cliente.Numero,
                Complemento = cliente.Complemento,
                Bairro = cliente.Bairro,
                Cidade = cliente.Cidade,
                Estado = cliente.Estado,
                Cep = cliente.Cep,
                Observacoes = cliente.Observacoes,
                LimiteCredito = cliente.LimiteCredito,
                Ativo = cliente.Ativo,
                DataCriacao = cliente.DataCriacao,
                DataModificacao = cliente.DataModificacao
            };

            return Ok(new ApiResponse<ClienteApiDto>
            {
                Success = true,
                Message = "Cliente obtido com sucesso",
                Data = clienteDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter cliente {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Cria novo cliente
    /// </summary>
    /// <param name="clienteDto">Dados do cliente</param>
    /// <returns>Cliente criado</returns>
    [HttpPost]
    public async Task<IActionResult> CreateCliente([FromBody] ClienteCreateApiDto clienteDto)
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
            
            // Verificar se CPF/CNPJ já existe
            if (!string.IsNullOrEmpty(clienteDto.CpfCnpj))
            {
                var clienteExistente = await _clienteService.ObterPorCpfCnpjAsync(clienteDto.CpfCnpj, empresaId);
                if (clienteExistente != null)
                {
                    return Conflict(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Já existe um cliente com este CPF/CNPJ"
                    });
                }
            }

            var cliente = new Domain.Entities.Cliente
            {
                Nome = clienteDto.Nome,
                CpfCnpj = clienteDto.CpfCnpj,
                Email = clienteDto.Email,
                Telefone = clienteDto.Telefone,
                Celular = clienteDto.Celular,
                Endereco = clienteDto.Endereco,
                Numero = clienteDto.Numero,
                Complemento = clienteDto.Complemento,
                Bairro = clienteDto.Bairro,
                Cidade = clienteDto.Cidade,
                Estado = clienteDto.Estado,
                Cep = clienteDto.Cep,
                Observacoes = clienteDto.Observacoes,
                LimiteCredito = clienteDto.LimiteCredito,
                EmpresaId = empresaId,
                Ativo = true
            };

            var clienteCriado = await _clienteService.CriarAsync(cliente);

            var response = new ClienteApiDto
            {
                Id = clienteCriado.Id,
                Nome = clienteCriado.Nome,
                CpfCnpj = clienteCriado.CpfCnpj,
                Email = clienteCriado.Email,
                Telefone = clienteCriado.Telefone,
                Celular = clienteCriado.Celular,
                Endereco = clienteCriado.Endereco,
                Numero = clienteCriado.Numero,
                Complemento = clienteCriado.Complemento,
                Bairro = clienteCriado.Bairro,
                Cidade = clienteCriado.Cidade,
                Estado = clienteCriado.Estado,
                Cep = clienteCriado.Cep,
                Observacoes = clienteCriado.Observacoes,
                LimiteCredito = clienteCriado.LimiteCredito,
                Ativo = clienteCriado.Ativo,
                DataCriacao = clienteCriado.DataCriacao,
                DataModificacao = clienteCriado.DataModificacao
            };

            return CreatedAtAction(nameof(GetCliente), new { id = clienteCriado.Id }, new ApiResponse<ClienteApiDto>
            {
                Success = true,
                Message = "Cliente criado com sucesso",
                Data = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar cliente");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Atualiza cliente existente
    /// </summary>
    /// <param name="id">ID do cliente</param>
    /// <param name="clienteDto">Dados atualizados do cliente</param>
    /// <returns>Cliente atualizado</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCliente(int id, [FromBody] ClienteUpdateApiDto clienteDto)
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
            var cliente = await _clienteService.ObterPorIdAsync(id, empresaId);

            if (cliente == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Cliente não encontrado"
                });
            }

            // Verificar se CPF/CNPJ já existe em outro cliente
            if (!string.IsNullOrEmpty(clienteDto.CpfCnpj) && clienteDto.CpfCnpj != cliente.CpfCnpj)
            {
                var clienteExistente = await _clienteService.ObterPorCpfCnpjAsync(clienteDto.CpfCnpj, empresaId);
                if (clienteExistente != null && clienteExistente.Id != id)
                {
                    return Conflict(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Já existe um cliente com este CPF/CNPJ"
                    });
                }
            }

            // Atualizar dados
            cliente.Nome = clienteDto.Nome;
            cliente.CpfCnpj = clienteDto.CpfCnpj;
            cliente.Email = clienteDto.Email;
            cliente.Telefone = clienteDto.Telefone;
            cliente.Celular = clienteDto.Celular;
            cliente.Endereco = clienteDto.Endereco;
            cliente.Numero = clienteDto.Numero;
            cliente.Complemento = clienteDto.Complemento;
            cliente.Bairro = clienteDto.Bairro;
            cliente.Cidade = clienteDto.Cidade;
            cliente.Estado = clienteDto.Estado;
            cliente.Cep = clienteDto.Cep;
            cliente.Observacoes = clienteDto.Observacoes;
            cliente.LimiteCredito = clienteDto.LimiteCredito;

            var clienteAtualizado = await _clienteService.AtualizarAsync(cliente);

            var response = new ClienteApiDto
            {
                Id = clienteAtualizado.Id,
                Nome = clienteAtualizado.Nome,
                CpfCnpj = clienteAtualizado.CpfCnpj,
                Email = clienteAtualizado.Email,
                Telefone = clienteAtualizado.Telefone,
                Celular = clienteAtualizado.Celular,
                Endereco = clienteAtualizado.Endereco,
                Numero = clienteAtualizado.Numero,
                Complemento = clienteAtualizado.Complemento,
                Bairro = clienteAtualizado.Bairro,
                Cidade = clienteAtualizado.Cidade,
                Estado = clienteAtualizado.Estado,
                Cep = clienteAtualizado.Cep,
                Observacoes = clienteAtualizado.Observacoes,
                LimiteCredito = clienteAtualizado.LimiteCredito,
                Ativo = clienteAtualizado.Ativo,
                DataCriacao = clienteAtualizado.DataCriacao,
                DataModificacao = clienteAtualizado.DataModificacao
            };

            return Ok(new ApiResponse<ClienteApiDto>
            {
                Success = true,
                Message = "Cliente atualizado com sucesso",
                Data = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar cliente {Id}", id);
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
            var clientes = await _clienteService.ObterTodosAsync(empresaId);

            var stats = new SyncStatsApiDto
            {
                TotalRecords = clientes.Count(),
                LastSyncDate = lastSync,
                NewRecords = lastSync.HasValue ? clientes.Count(c => c.DataCriacao > lastSync.Value) : clientes.Count(),
                UpdatedRecords = lastSync.HasValue ? clientes.Count(c => c.DataModificacao > lastSync.Value && c.DataCriacao <= lastSync.Value) : 0,
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