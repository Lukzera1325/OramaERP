using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using OramaGo.Services;

namespace OramaGo.Services;

/// <summary>
/// Serviço de sincronização automática
/// </summary>
public class SyncService : ISyncService
{
    private readonly ILogger<SyncService> _logger;
    private readonly IConnectivityService _connectivityService;
    private readonly IClienteSyncService _clienteSyncService;
    
    private Timer? _autoSyncTimer;
    private bool _isAutoSyncEnabled = true;
    private bool _isSyncing;
    private SyncStatus _status = SyncStatus.Idle;
    private DateTime? _lastSyncTime;
    private int _progress;
    private string _statusMessage = "Pronto para sincronizar";
    private CancellationTokenSource? _cancellationTokenSource;

    // Configurações de sincronização
    private readonly TimeSpan _autoSyncInterval = TimeSpan.FromMinutes(5); // Sincronizar a cada 5 minutos
    private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(30); // Aguardar 30s antes de tentar novamente
    private readonly int _maxRetries = 3; // Máximo 3 tentativas

    public SyncService(
        ILogger<SyncService> logger,
        IConnectivityService connectivityService,
        IClienteSyncService clienteSyncService)
    {
        _logger = logger;
        _connectivityService = connectivityService;
        _clienteSyncService = clienteSyncService;

        // Registrar eventos de conectividade
        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
    }

    #region Propriedades

    public bool IsSyncing
    {
        get => _isSyncing;
        private set
        {
            if (_isSyncing != value)
            {
                _isSyncing = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsAutoSyncEnabled
    {
        get => _isAutoSyncEnabled;
        set
        {
            if (_isAutoSyncEnabled != value)
            {
                _isAutoSyncEnabled = value;
                OnPropertyChanged();

                if (value)
                    StartAutoSyncTimer();
                else
                    StopAutoSyncTimer();
            }
        }
    }

    public DateTime? LastSyncTime
    {
        get => _lastSyncTime;
        private set
        {
            if (_lastSyncTime != value)
            {
                _lastSyncTime = value;
                OnPropertyChanged();
            }
        }
    }

    public SyncStatus Status
    {
        get => _status;
        private set
        {
            var previousStatus = _status;
            if (_status != value)
            {
                _status = value;
                OnPropertyChanged();

                // Disparar evento de mudança de status
                SyncStatusChanged?.Invoke(this, new SyncStatusChangedEventArgs
                {
                    PreviousStatus = previousStatus,
                    CurrentStatus = value,
                    Message = StatusMessage,
                    Progress = Progress,
                    Timestamp = DateTime.Now
                });
            }
        }
    }

    public int Progress
    {
        get => _progress;
        private set
        {
            if (_progress != value)
            {
                _progress = Math.Max(0, Math.Min(100, value));
                OnPropertyChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Eventos

    public event EventHandler<SyncStatusChangedEventArgs>? SyncStatusChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Métodos Públicos

    public async Task StartAutoSyncAsync()
    {
        _logger.LogInformation("Iniciando sincronização automática");
        IsAutoSyncEnabled = true;
        StartAutoSyncTimer();

        // Executar primeira sincronização se conectado
        if (_connectivityService.IsApiReachable)
        {
            _ = Task.Run(async () => await SyncAllAsync());
        }
    }

    public void StopAutoSync()
    {
        _logger.LogInformation("Parando sincronização automática");
        IsAutoSyncEnabled = false;
        StopAutoSyncTimer();
    }

    public async Task<SyncResult> SyncAllAsync()
    {
        if (IsSyncing)
        {
            _logger.LogWarning("Sincronização já está em andamento");
            return new SyncResult
            {
                Success = false,
                Message = "Sincronização já está em andamento"
            };
        }

        var startTime = DateTime.Now;
        var result = new SyncResult();

        try
        {
            IsSyncing = true;
            Status = SyncStatus.Syncing;
            Progress = 0;
            StatusMessage = "Iniciando sincronização...";

            _cancellationTokenSource = new CancellationTokenSource();

            // Verificar conectividade
            if (!_connectivityService.IsApiReachable)
            {
                Status = SyncStatus.NoConnection;
                StatusMessage = "Sem conexão com a API";
                return new SyncResult
                {
                    Success = false,
                    Message = "Sem conexão com a API"
                };
            }

            _logger.LogInformation("Iniciando sincronização completa");

            // Sincronizar clientes (33%)
            Progress = 10;
            StatusMessage = "Sincronizando clientes...";
            var clientesResult = await _clienteSyncService.SyncClientesAsync();
            result.ClientesProcessados = clientesResult.ClientesProcessados;
            result.Errors.AddRange(clientesResult.Errors);
            Progress = 33;

            // Simular sincronização de produtos (66%)
            Progress = 40;
            StatusMessage = "Sincronizando produtos...";
            await Task.Delay(1000, _cancellationTokenSource.Token);
            Progress = 66;

            // Simular sincronização de vendas (100%)
            Progress = 70;
            StatusMessage = "Sincronizando vendas...";
            await Task.Delay(1000, _cancellationTokenSource.Token);
            Progress = 100;

            // Finalizar
            result.Success = result.Errors.Count == 0;
            result.Message = result.Success 
                ? $"Sincronização concluída com sucesso. {result.ClientesProcessados} registros processados."
                : $"Sincronização concluída com {result.Errors.Count} erro(s).";

            Status = result.Success ? SyncStatus.Success : SyncStatus.Error;
            StatusMessage = result.Message;
            LastSyncTime = DateTime.Now;

            _logger.LogInformation("Sincronização completa finalizada: {Success}", result.Success);
        }
        catch (OperationCanceledException)
        {
            Status = SyncStatus.Cancelled;
            StatusMessage = "Sincronização cancelada";
            result.Success = false;
            result.Message = "Sincronização cancelada pelo usuário";
            _logger.LogInformation("Sincronização cancelada");
        }
        catch (Exception ex)
        {
            Status = SyncStatus.Error;
            StatusMessage = $"Erro na sincronização: {ex.Message}";
            result.Success = false;
            result.Message = ex.Message;
            result.Errors.Add(ex.Message);
            _logger.LogError(ex, "Erro durante sincronização completa");
        }
        finally
        {
            IsSyncing = false;
            Progress = 0;
            result.Duration = DateTime.Now - startTime;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        return result;
    }

    public async Task<SyncResult> SyncClientesAsync()
    {
        return await _clienteSyncService.SyncClientesAsync();
    }

    public async Task<SyncResult> SyncProdutosAsync()
    {
        // Implementação simplificada
        return new SyncResult
        {
            Success = true,
            Message = "Sincronização de produtos não implementada ainda"
        };
    }

    public async Task<SyncResult> SyncVendasAsync()
    {
        // Implementação simplificada
        return new SyncResult
        {
            Success = true,
            Message = "Sincronização de vendas não implementada ainda"
        };
    }

    public async Task<SyncResult> ForceSyncAsync()
    {
        return await SyncAllAsync();
    }

    #endregion

    #region Métodos Privados

    private void StartAutoSyncTimer()
    {
        StopAutoSyncTimer();

        _autoSyncTimer = new Timer(async _ =>
        {
            if (IsAutoSyncEnabled && !IsSyncing && _connectivityService.IsApiReachable)
            {
                _logger.LogDebug("Executando sincronização automática");
                await SyncAllAsync();
            }
        }, null, _autoSyncInterval, _autoSyncInterval);

        _logger.LogDebug("Timer de sincronização automática iniciado (intervalo: {Interval})", _autoSyncInterval);
    }

    private void StopAutoSyncTimer()
    {
        _autoSyncTimer?.Dispose();
        _autoSyncTimer = null;
        _logger.LogDebug("Timer de sincronização automática parado");
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        _logger.LogInformation("Conectividade mudou: {Status}", e.CurrentStatus);

        // Se ficou conectado e sincronização automática está habilitada, sincronizar
        if (e.CurrentStatus == ConnectivityStatus.Connected && 
            IsAutoSyncEnabled && 
            !IsSyncing && 
            _connectivityService.IsApiReachable)
        {
            _logger.LogInformation("Conectividade restaurada, iniciando sincronização");
            _ = Task.Run(async () => await SyncAllAsync());
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        StopAutoSync();
        _connectivityService.ConnectivityChanged -= OnConnectivityChanged;
        _cancellationTokenSource?.Dispose();
    }

    #endregion
}