using Microsoft.Extensions.Logging;

namespace OramaGo.Services;

public class PermissionService : IPermissionService
{
    private readonly IAuthService _authService;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(IAuthService authService, ILogger<PermissionService> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(string permission)
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                _logger.LogWarning("Tentativa de verificar permissão sem usuário autenticado");
                return false;
            }

            var hasPermission = user.Permissoes.Contains(permission);
            _logger.LogDebug($"Verificação de permissão '{permission}' para usuário {user.Email}: {hasPermission}");
            
            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao verificar permissão: {permission}");
            return false;
        }
    }

    public async Task<bool> HasAnyPermissionAsync(params string[] permissions)
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return false;

            return permissions.Any(permission => user.Permissoes.Contains(permission));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar permissões (any)");
            return false;
        }
    }

    public async Task<bool> HasAllPermissionsAsync(params string[] permissions)
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return false;

            return permissions.All(permission => user.Permissoes.Contains(permission));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar permissões (all)");
            return false;
        }
    }

    public async Task<bool> CanAccessOramaGoAsync()
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync();
            return user != null; // Se chegou até aqui, já passou pela validação no login
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar acesso ao Órama Go");
            return false;
        }
    }

    public async Task<List<string>> GetUserPermissionsAsync()
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync();
            return user?.Permissoes ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter permissões do usuário");
            return new List<string>();
        }
    }

    public async Task RefreshPermissionsAsync()
    {
        try
        {
            // TODO: Implementar refresh das permissões via API quando estiver pronta
            _logger.LogInformation("Refresh de permissões solicitado");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar permissões");
        }
    }
}