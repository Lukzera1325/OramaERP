using Microsoft.Extensions.Logging;

namespace OramaGo.Services;

public class StartupService : IStartupService
{
    private readonly IDatabaseService _databaseService;
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly IConnectivityService _connectivityService;
    private readonly ISyncService _syncService;
    private readonly ILogger<StartupService> _logger;

    public StartupService(
        IDatabaseService databaseService,
        IAuthService authService,
        INavigationService navigationService,
        IConnectivityService connectivityService,
        ISyncService syncService,
        ILogger<StartupService> logger)
    {
        _databaseService = databaseService;
        _authService = authService;
        _navigationService = navigationService;
        _connectivityService = connectivityService;
        _syncService = syncService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Iniciando aplicação...");

            // Inicializar banco de dados
            await _databaseService.InitializeDatabaseAsync();
            await _databaseService.SeedDataAsync();

            // Inicializar serviços de conectividade e sincronização
            await _connectivityService.StartMonitoringAsync();
            
            // Verificar autenticação
            var isAuthenticated = await _authService.IsAuthenticatedAsync();
            
            if (isAuthenticated)
            {
                _logger.LogInformation("Usuário já autenticado, navegando para dashboard");
                
                // Iniciar sincronização automática para usuários autenticados
                await _syncService.StartAutoSyncAsync();
                
                await _navigationService.NavigateToAsync("//dashboard");
            }
            else
            {
                _logger.LogInformation("Usuário não autenticado, navegando para login");
                await _navigationService.NavigateToAsync("//login");
            }

            _logger.LogInformation("Aplicação inicializada com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao inicializar aplicação");
            await _navigationService.DisplayAlertAsync("Erro", "Erro ao inicializar aplicação", "OK");
        }
    }
}