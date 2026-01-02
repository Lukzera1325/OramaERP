using Microsoft.Extensions.Logging;
using OramaGo.Models;
using System.Text.Json;

namespace OramaGo.Services;

public interface IClienteSyncService
{
    Task<SyncResult> SyncClientesAsync();
    Task<SyncResult> UploadClientesPendentesAsync();
    Task<SyncResult> DownloadClientesAtualizadosAsync();
    Task<Models.SyncStatus> GetSyncStatusAsync();
    Task<int> GetPendingCountAsync();
}

public class ClienteSyncService : IClienteSyncService
{
    private readonly IClienteService _clienteService;
    private readonly HttpClient _httpClient;
    private readonly IUserContextService _userContextService;
    private readonly IAuthService _authService;
    private readonly ILogger<ClienteSyncService> _logger;

    public ClienteSyncService(
        IClienteService clienteService,
        HttpClient httpClient,
        IUserContextService userContextService,
        IAuthService authService,
        ILogger<ClienteSyncService> logger)
    {
        _clienteService = clienteService;
        _httpClient = httpClient;
        _userContextService = userContextService;
        _authService = authService;
        _logger = logger;
    }

    public async Task<SyncResult> SyncClientesAsync()
    {
        var result = new SyncResult();
        var startTime = DateTime.Now;

        try
        {
            _logger.LogInformation("Iniciando sincronização de clientes");

            // 1. Primeiro, fazer upload dos clientes pendentes
            var uploadResult = await UploadClientesPendentesAsync();
            result.ClientesProcessados += uploadResult.ClientesProcessados;
            result.Errors.AddRange(uploadResult.Errors);
            
            // 2. Depois, fazer download dos clientes atualizados
            var downloadResult = await DownloadClientesAtualizadosAsync();
            result.ClientesProcessados += downloadResult.ClientesProcessados;
            result.Errors.AddRange(downloadResult.Errors);
            
            result.Success = uploadResult.Success && downloadResult.Success;
            result.Message = result.Success 
                ? $"Sincronização concluída. {result.ClientesProcessados} clientes processados."
                : "Sincronização concluída com erros.";

            _logger.LogInformation("Sincronização de clientes concluída: {Success}", result.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante sincronização de clientes");
            result.Success = false;
            result.Message = $"Erro durante sincronização: {ex.Message}";
            result.Errors.Add(ex.Message);
        }
        finally
        {
            result.Duration = DateTime.Now - startTime;
        }

        return result;
    }

    public async Task<SyncResult> UploadClientesPendentesAsync()
    {
        var result = new SyncResult();
        
        try
        {
            _logger.LogInformation("Iniciando upload de clientes pendentes");

            var clientesPendentes = await GetClientesPendentesAsync();
            
            foreach (var cliente in clientesPendentes)
            {
                try
                {
                    var success = await UploadClienteAsync(cliente);
                    if (success)
                    {
                        // Marcar como sincronizado
                        cliente.StatusSync = Models.SyncStatus.Synced;
                        cliente.DataUltimaSync = DateTime.Now;
                        await _clienteService.UpdateAsync(cliente);
                        result.ClientesProcessados++;
                        
                        _logger.LogDebug("Cliente {ClienteId} sincronizado com sucesso", cliente.Id);
                    }
                    else
                    {
                        // Marcar como erro
                        cliente.StatusSync = Models.SyncStatus.Error;
                        await _clienteService.UpdateAsync(cliente);
                        result.Errors.Add($"Falha ao sincronizar cliente {cliente.Nome}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar cliente {ClienteId}", cliente.Id);
                    cliente.StatusSync = Models.SyncStatus.Error;
                    await _clienteService.UpdateAsync(cliente);
                    result.Errors.Add($"Erro ao sincronizar cliente {cliente.Nome}: {ex.Message}");
                }
            }
            
            result.Success = result.Errors.Count == 0;
            result.Message = result.Success 
                ? $"{result.ClientesProcessados} clientes enviados com sucesso"
                : $"{result.ClientesProcessados} clientes enviados, {result.Errors.Count} com erro";

            _logger.LogInformation("Upload de clientes concluído: {ClientesProcessados} processados, {Errors} erros", 
                result.ClientesProcessados, result.Errors.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante upload de clientes");
            result.Success = false;
            result.Message = $"Erro durante upload: {ex.Message}";
            result.Errors.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<SyncResult> DownloadClientesAtualizadosAsync()
    {
        var result = new SyncResult();
        
        try
        {
            _logger.LogInformation("Iniciando download de clientes atualizados");

            // TODO: Implementar quando API estiver disponível
            // Por enquanto, simular download
            await Task.Delay(1000);
            
            // Simular alguns clientes do servidor
            var clientesServidor = new List<ClienteLocal>
            {
                new ClienteLocal
                {
                    ServidorId = 1000,
                    Nome = "Cliente Servidor 1",
                    Email = "cliente1@servidor.com",
                    Telefone = "(11) 1111-1111",
                    StatusSync = Models.SyncStatus.Synced,
                    DataUltimaSync = DateTime.Now
                },
                new ClienteLocal
                {
                    ServidorId = 1001,
                    Nome = "Cliente Servidor 2", 
                    Email = "cliente2@servidor.com",
                    Telefone = "(11) 2222-2222",
                    StatusSync = Models.SyncStatus.Synced,
                    DataUltimaSync = DateTime.Now
                }
            };

            // Salvar clientes do servidor localmente
            foreach (var cliente in clientesServidor)
            {
                try
                {
                    // Verificar se já existe pelo ServidorId
                    var existingCliente = await GetClienteByServidorIdAsync(cliente.ServidorId);
                    if (existingCliente == null)
                    {
                        await _clienteService.CreateAsync(cliente);
                        result.ClientesProcessados++;
                        _logger.LogDebug("Cliente {ServidorId} criado localmente", cliente.ServidorId);
                    }
                    else
                    {
                        // Verificar se precisa atualizar (comparar hash ou data de modificação)
                        if (ShouldUpdateCliente(existingCliente, cliente))
                        {
                            cliente.Id = existingCliente.Id;
                            await _clienteService.UpdateAsync(cliente);
                            result.ClientesProcessados++;
                            _logger.LogDebug("Cliente {ServidorId} atualizado localmente", cliente.ServidorId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar cliente do servidor {ServidorId}", cliente.ServidorId);
                    result.Errors.Add($"Erro ao processar cliente {cliente.Nome}: {ex.Message}");
                }
            }
            
            result.Success = result.Errors.Count == 0;
            result.Message = result.Success 
                ? $"{result.ClientesProcessados} clientes baixados com sucesso"
                : $"{result.ClientesProcessados} clientes baixados, {result.Errors.Count} com erro";

            _logger.LogInformation("Download de clientes concluído: {ClientesProcessados} processados, {Errors} erros", 
                result.ClientesProcessados, result.Errors.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante download de clientes");
            result.Success = false;
            result.Message = $"Erro durante download: {ex.Message}";
            result.Errors.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<Models.SyncStatus> GetSyncStatusAsync()
    {
        try
        {
            var totalClientes = await _clienteService.GetCountAsync();
            var clientesPendentes = await GetClientesPendentesAsync();
            
            if (clientesPendentes.Any())
            {
                return Models.SyncStatus.Pending;
            }
            
            if (totalClientes == 0)
            {
                return Models.SyncStatus.Pending;
            }
            
            return Models.SyncStatus.Synced;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter status de sincronização");
            return Models.SyncStatus.Error;
        }
    }

    public async Task<int> GetPendingCountAsync()
    {
        try
        {
            var clientesPendentes = await GetClientesPendentesAsync();
            return clientesPendentes.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter contagem de clientes pendentes");
            return 0;
        }
    }

    private async Task<IEnumerable<ClienteLocal>> GetClientesPendentesAsync()
    {
        var empresaId = await _userContextService.GetCurrentEmpresaIdAsync();
        var allClientes = await _clienteService.GetAllAsync();
        
        return allClientes.Where(c => 
            c.StatusSync == Models.SyncStatus.Pending || 
            c.StatusSync == Models.SyncStatus.Modified ||
            c.StatusSync == Models.SyncStatus.Error);
    }

    private async Task<ClienteLocal?> GetClienteByServidorIdAsync(int? servidorId)
    {
        if (!servidorId.HasValue) return null;
        
        var allClientes = await _clienteService.GetAllAsync();
        
        return allClientes.FirstOrDefault(c => c.ServidorId == servidorId);
    }

    private bool ShouldUpdateCliente(ClienteLocal local, ClienteLocal servidor)
    {
        // Comparar por data de modificação ou hash
        if (!string.IsNullOrEmpty(servidor.HashDados) && !string.IsNullOrEmpty(local.HashDados))
        {
            return servidor.HashDados != local.HashDados;
        }
        
        // Fallback: comparar por data de modificação
        return servidor.DataModificacao > local.DataModificacao;
    }

    private async Task<bool> UploadClienteAsync(ClienteLocal cliente)
    {
        try
        {
            var userContext = await _userContextService.GetCurrentUserAsync();
            if (userContext == null) 
            {
                _logger.LogWarning("Contexto de usuário não encontrado para upload de cliente");
                return false;
            }

            // TODO: Implementar quando API estiver disponível
            // Por enquanto, simular upload
            await Task.Delay(500);
            
            // Simular chamada para API
            var clienteDto = new
            {
                cliente.Nome,
                cliente.CpfCnpj,
                cliente.RgIe,
                cliente.Email,
                cliente.Telefone,
                cliente.Celular,
                cliente.Cep,
                cliente.Endereco,
                cliente.Numero,
                cliente.Complemento,
                cliente.Bairro,
                cliente.Cidade,
                cliente.Estado,
                cliente.DataNascimento,
                cliente.LimiteCredito,
                cliente.Bloqueado,
                cliente.MotivoBloqueio,
                cliente.Observacoes,
                EmpresaId = userContext.EmpresaId
            };

            var json = JsonSerializer.Serialize(clienteDto);
            _logger.LogDebug("Dados do cliente para upload: {Json}", json);
            
            // Simular sucesso (90% de chance)
            var random = new Random();
            var success = random.NextDouble() > 0.1;
            
            if (success)
            {
                _logger.LogDebug("Upload simulado com sucesso para cliente {ClienteId}", cliente.Id);
            }
            else
            {
                _logger.LogWarning("Upload simulado falhou para cliente {ClienteId}", cliente.Id);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante upload de cliente {ClienteId}", cliente.Id);
            return false;
        }
    }
}

