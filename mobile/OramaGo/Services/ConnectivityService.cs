using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace OramaGo.Services;

/// <summary>
/// Serviço para monitoramento de conectividade
/// </summary>
public class ConnectivityService : IConnectivityService
{
    private readonly ILogger<ConnectivityService> _logger;
    private readonly HttpClient _httpClient;
    private Timer? _monitoringTimer;
    private bool _isMonitoring;
    private ConnectivityStatus _status = ConnectivityStatus.Unknown;
    private bool _isConnected;
    private bool _isApiReachable;

    public ConnectivityService(
        ILogger<ConnectivityService> logger,
        HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;

        // Configurar timeout para testes de conectividade
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            if (_isConnected != value)
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsApiReachable
    {
        get => _isApiReachable;
        private set
        {
            if (_isApiReachable != value)
            {
                _isApiReachable = value;
                OnPropertyChanged();
            }
        }
    }

    public ConnectivityStatus Status
    {
        get => _status;
        private set
        {
            if (_status != value)
            {
                var previousStatus = _status;
                _status = value;
                OnPropertyChanged();

                // Disparar evento de mudança
                ConnectivityChanged?.Invoke(this, new ConnectivityChangedEventArgs
                {
                    PreviousStatus = previousStatus,
                    CurrentStatus = value,
                    IsConnected = IsConnected,
                    IsApiReachable = IsApiReachable
                });
            }
        }
    }

    public event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Inicia o monitoramento de conectividade
    /// </summary>
    public async Task StartMonitoringAsync()
    {
        if (_isMonitoring)
            return;

        _logger.LogInformation("Iniciando monitoramento de conectividade");
        _isMonitoring = true;

        // Teste inicial
        await CheckConnectivityAsync();

        // Configurar timer para verificações periódicas (a cada 30 segundos)
        _monitoringTimer = new Timer(async _ => await CheckConnectivityAsync(), 
            null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

        // Registrar eventos de mudança de rede do sistema
        Connectivity.ConnectivityChanged += OnSystemConnectivityChanged;
    }

    /// <summary>
    /// Para o monitoramento de conectividade
    /// </summary>
    public void StopMonitoring()
    {
        if (!_isMonitoring)
            return;

        _logger.LogInformation("Parando monitoramento de conectividade");
        _isMonitoring = false;

        _monitoringTimer?.Dispose();
        _monitoringTimer = null;

        Connectivity.ConnectivityChanged -= OnSystemConnectivityChanged;
    }

    /// <summary>
    /// Testa a conectividade com a API manualmente
    /// </summary>
    public async Task<bool> TestApiConnectivityAsync()
    {
        try
        {
            var baseUrl = "https://localhost:5001"; // URL padrão da API
            var healthEndpoint = $"{baseUrl}/api/auth/validate";

            _logger.LogDebug("Testando conectividade com API: {Endpoint}", healthEndpoint);

            using var response = await _httpClient.GetAsync(healthEndpoint);
            
            // Consideramos sucesso se a API responder (mesmo que seja 401 Unauthorized)
            // pois isso indica que a API está acessível
            var isReachable = response.StatusCode != System.Net.HttpStatusCode.RequestTimeout;
            
            _logger.LogDebug("Teste de API concluído. Acessível: {IsReachable}, Status: {StatusCode}", 
                isReachable, response.StatusCode);

            return isReachable;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("Erro de conectividade com API: {Error}", ex.Message);
            return false;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning("Timeout ao testar conectividade com API");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao testar conectividade com API");
            return false;
        }
    }

    /// <summary>
    /// Verifica a conectividade completa
    /// </summary>
    private async Task CheckConnectivityAsync()
    {
        try
        {
            // 1. Verificar conectividade básica do sistema
            var networkAccess = Connectivity.NetworkAccess;
            var hasBasicConnectivity = networkAccess == NetworkAccess.Internet;

            if (!hasBasicConnectivity)
            {
                UpdateStatus(false, false, ConnectivityStatus.Disconnected);
                return;
            }

            // 2. Testar conectividade real com internet (ping)
            var hasInternetConnectivity = await TestInternetConnectivityAsync();
            
            if (!hasInternetConnectivity)
            {
                UpdateStatus(false, false, ConnectivityStatus.ConnectedNoInternet);
                return;
            }

            // 3. Testar conectividade com API
            var hasApiConnectivity = await TestApiConnectivityAsync();
            
            if (!hasApiConnectivity)
            {
                UpdateStatus(true, false, ConnectivityStatus.ConnectedNoApi);
                return;
            }

            // Tudo funcionando
            UpdateStatus(true, true, ConnectivityStatus.Connected);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar conectividade");
            UpdateStatus(false, false, ConnectivityStatus.Unknown);
        }
    }

    /// <summary>
    /// Testa conectividade real com internet usando ping
    /// </summary>
    private async Task<bool> TestInternetConnectivityAsync()
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync("8.8.8.8", 5000); // Google DNS
            return reply.Status == IPStatus.Success;
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Erro no teste de ping: {Error}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Atualiza o status da conectividade
    /// </summary>
    private void UpdateStatus(bool isConnected, bool isApiReachable, ConnectivityStatus status)
    {
        IsConnected = isConnected;
        IsApiReachable = isApiReachable;
        Status = status;

        _logger.LogDebug("Status de conectividade atualizado: {Status}, Internet: {IsConnected}, API: {IsApiReachable}", 
            status, isConnected, isApiReachable);
    }

    /// <summary>
    /// Manipula eventos de mudança de conectividade do sistema
    /// </summary>
    private async void OnSystemConnectivityChanged(object? sender, Microsoft.Maui.Networking.ConnectivityChangedEventArgs e)
    {
        _logger.LogInformation("Conectividade do sistema mudou: {NetworkAccess}", e.NetworkAccess);
        
        // Aguardar um pouco para a rede estabilizar
        await Task.Delay(2000);
        
        // Verificar conectividade completa
        await CheckConnectivityAsync();
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose()
    {
        StopMonitoring();
    }
}