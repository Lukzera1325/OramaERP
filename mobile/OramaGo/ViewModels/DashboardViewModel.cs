using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OramaGo.Services;

namespace OramaGo.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IClienteService _clienteService;
    private readonly IProdutoService _produtoService;
    private readonly IDatabaseService _databaseService;
    private readonly IUserContextService _userContext;
    private readonly ISyncService _syncService;
    private readonly IConnectivityService _connectivityService;

    [ObservableProperty]
    private int totalClientes;

    [ObservableProperty]
    private int totalProdutos;

    [ObservableProperty]
    private int totalVendas;

    [ObservableProperty]
    private string statusSync = "Offline";

    [ObservableProperty]
    private string databaseStatus = "Conectado";

    [ObservableProperty]
    private string ultimaSincronizacao = "Nunca";

    [ObservableProperty]
    private bool isSyncing;

    [ObservableProperty]
    private int syncProgress;

    [ObservableProperty]
    private string syncStatusMessage = "Pronto";

    public DashboardViewModel(
        INavigationService navigationService,
        IClienteService clienteService,
        IProdutoService produtoService,
        IDatabaseService databaseService,
        IUserContextService userContext,
        ISyncService syncService,
        IConnectivityService connectivityService)
    {
        _navigationService = navigationService;
        _clienteService = clienteService;
        _produtoService = produtoService;
        _databaseService = databaseService;
        _userContext = userContext;
        _syncService = syncService;
        _connectivityService = connectivityService;
        
        Title = "Dashboard";

        // Registrar eventos de sincronização
        _syncService.PropertyChanged += OnSyncServicePropertyChanged;
        _syncService.SyncStatusChanged += OnSyncStatusChanged;
        _connectivityService.PropertyChanged += OnConnectivityServicePropertyChanged;
    }

    public override async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    protected override async Task LoadDataAsync()
    {
        try
        {
            SetBusy(true);

            // Verificar se usuário está autenticado
            if (!await _userContext.IsUserLoggedInAsync())
            {
                await _navigationService.NavigateToAsync("//login");
                return;
            }

            // Carregar estatísticas usando contexto do usuário
            TotalClientes = await _clienteService.GetCountAsync();
            TotalProdutos = await _produtoService.CountAsync();
            TotalVendas = 0; // TODO: Implementar quando VendaService estiver pronto

            // Verificar status do banco
            var dbExists = await _databaseService.DatabaseExistsAsync();
            DatabaseStatus = dbExists ? "Conectado" : "Erro";

            // Status de sincronização
            UpdateSyncStatus();
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>
    /// Atualiza o status de sincronização
    /// </summary>
    private void UpdateSyncStatus()
    {
        IsSyncing = _syncService.IsSyncing;
        SyncProgress = _syncService.Progress;
        SyncStatusMessage = _syncService.StatusMessage;

        // Atualizar status baseado na conectividade e sincronização
        if (_syncService.IsSyncing)
        {
            StatusSync = "Sincronizando...";
        }
        else if (_connectivityService.IsApiReachable)
        {
            StatusSync = "Online";
        }
        else if (_connectivityService.IsConnected)
        {
            StatusSync = "Sem API";
        }
        else
        {
            StatusSync = "Offline";
        }

        // Atualizar última sincronização
        if (_syncService.LastSyncTime.HasValue)
        {
            var lastSync = _syncService.LastSyncTime.Value;
            var timeSpan = DateTime.Now - lastSync;

            if (timeSpan.TotalMinutes < 1)
                UltimaSincronizacao = "Agora";
            else if (timeSpan.TotalHours < 1)
                UltimaSincronizacao = $"{(int)timeSpan.TotalMinutes}min atrás";
            else if (timeSpan.TotalDays < 1)
                UltimaSincronizacao = $"{(int)timeSpan.TotalHours}h atrás";
            else
                UltimaSincronizacao = lastSync.ToString("dd/MM HH:mm");
        }
        else
        {
            UltimaSincronizacao = "Nunca";
        }
    }

    /// <summary>
    /// Manipula mudanças no serviço de sincronização
    /// </summary>
    private void OnSyncServicePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateSyncStatus);
    }

    /// <summary>
    /// Manipula mudanças de status de sincronização
    /// </summary>
    private void OnSyncStatusChanged(object? sender, SyncStatusChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateSyncStatus();
            
            // Recarregar dados após sincronização bem-sucedida
            if (e.CurrentStatus == SyncStatus.Success)
            {
                _ = Task.Run(LoadDataAsync);
            }
        });
    }

    /// <summary>
    /// Manipula mudanças no serviço de conectividade
    /// </summary>
    private void OnConnectivityServicePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateSyncStatus);
    }

    [RelayCommand]
    private async Task NavigateToClientesAsync()
    {
        await _navigationService.NavigateToAsync("//clientes");
    }

    [RelayCommand]
    private async Task NavigateToProdutosAsync()
    {
        await _navigationService.NavigateToAsync("//produtos");
    }

    [RelayCommand]
    private async Task NovaVendaAsync()
    {
        await _navigationService.NavigateToAsync("//vendas/criar");
    }

    [RelayCommand]
    private async Task SincronizarAsync()
    {
        try
        {
            if (_syncService.IsSyncing)
            {
                await _navigationService.DisplayAlertAsync("Sincronização", "Sincronização já está em andamento", "OK");
                return;
            }

            if (!_connectivityService.IsApiReachable)
            {
                await _navigationService.DisplayAlertAsync("Sem Conexão", 
                    "Não é possível sincronizar sem conexão com a API", "OK");
                return;
            }

            var result = await _syncService.SyncAllAsync();
            
            if (result.Success)
            {
                await _navigationService.DisplayAlertAsync("Sucesso", 
                    $"Sincronização concluída!\n\n{result.Message}", "OK");
            }
            else
            {
                await _navigationService.DisplayAlertAsync("Erro", 
                    $"Erro na sincronização:\n\n{result.Message}", "OK");
            }
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro na sincronização: {ex.Message}", "OK");
        }
    }
}