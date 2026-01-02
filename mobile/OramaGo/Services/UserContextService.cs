using Microsoft.Extensions.Logging;
using OramaGo.Models;

namespace OramaGo.Services;

public class UserContextService : IUserContextService
{
    private readonly IAuthService _authService;
    private readonly ILogger<UserContextService> _logger;
    private UserSession? _currentUser;

    public event EventHandler<UserSession?>? UserChanged;

    public UserContextService(IAuthService authService, ILogger<UserContextService> logger)
    {
        _authService = authService;
        _logger = logger;

        // Escutar mudanças de autenticação
        _authService.AuthenticationChanged += OnAuthenticationChanged;
    }

    public async Task<UserSession?> GetCurrentUserAsync()
    {
        if (_currentUser?.IsValid == true)
            return _currentUser;

        _currentUser = await _authService.GetCurrentUserAsync();
        return _currentUser;
    }

    public async Task<int> GetCurrentEmpresaIdAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            _logger.LogWarning("Tentativa de obter EmpresaId sem usuário logado");
            return 0;
        }
        return user.EmpresaId;
    }

    public async Task<string> GetCurrentUserNameAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.Nome ?? "Usuário";
    }

    public async Task<string> GetCurrentUserEmailAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.Email ?? "";
    }

    public async Task<bool> IsUserLoggedInAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.IsValid == true;
    }

    public async Task SetCurrentUserAsync(UserSession user)
    {
        _currentUser = user;
        UserChanged?.Invoke(this, user);
        _logger.LogInformation($"Usuário definido no contexto: {user.Email}");
    }

    public async Task ClearCurrentUserAsync()
    {
        _currentUser = null;
        UserChanged?.Invoke(this, null);
        _logger.LogInformation("Usuário removido do contexto");
    }

    private async void OnAuthenticationChanged(object? sender, bool isAuthenticated)
    {
        if (!isAuthenticated)
        {
            await ClearCurrentUserAsync();
        }
        else
        {
            // Recarregar usuário atual
            _currentUser = await _authService.GetCurrentUserAsync();
            UserChanged?.Invoke(this, _currentUser);
        }
    }
}