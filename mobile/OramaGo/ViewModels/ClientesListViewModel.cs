using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OramaGo.Models;
using OramaGo.Services;
using System.Collections.ObjectModel;

namespace OramaGo.ViewModels;

public partial class ClientesListViewModel : BaseViewModel
{
    private readonly IClienteService _clienteService;
    private readonly INavigationService _navigationService;
    private readonly IPermissionService _permissionService;
    private readonly ISyncService _syncService;
    private readonly IClienteSyncService _clienteSyncService;

    [ObservableProperty]
    private ObservableCollection<ClienteLocal> clientes = new();

    [ObservableProperty]
    private ObservableCollection<ClienteLocal> clientesFiltrados = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool hasClientes;

    [ObservableProperty]
    private int totalClientes;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool canAddCliente;

    [ObservableProperty]
    private bool canEditCliente;

    [ObservableProperty]
    private bool isSyncing;

    [ObservableProperty]
    private int pendingSyncCount;

    public ClientesListViewModel(
        IClienteService clienteService,
        INavigationService navigationService,
        IPermissionService permissionService,
        ISyncService syncService,
        IClienteSyncService clienteSyncService)
    {
        _clienteService = clienteService;
        _navigationService = navigationService;
        _permissionService = permissionService;
        _syncService = syncService;
        _clienteSyncService = clienteSyncService;
        
        Title = "Clientes";

        // Registrar eventos de sincronização
        _syncService.SyncStatusChanged += OnSyncStatusChanged;
    }

    public override async Task InitializeAsync()
    {
        await CheckPermissionsAsync();
        await LoadDataAsync();
        await UpdateSyncStatusAsync();
    }

    protected override async Task LoadDataAsync()
    {
        try
        {
            SetBusy(true);
            StatusMessage = "Carregando clientes...";

            var clientesList = await _clienteService.GetAllAsync();
            
            Clientes.Clear();
            foreach (var cliente in clientesList.OrderBy(c => c.Nome))
            {
                Clientes.Add(cliente);
            }

            await ApplyFilterAsync();
            
            TotalClientes = Clientes.Count;
            HasClientes = TotalClientes > 0;
            
            await UpdateSyncStatusAsync();
            
            StatusMessage = HasClientes 
                ? $"{TotalClientes} cliente(s) encontrado(s)" + (PendingSyncCount > 0 ? $" ({PendingSyncCount} pendente(s))" : "")
                : "Nenhum cliente cadastrado";
        }
        catch (Exception ex)
        {
            StatusMessage = "Erro ao carregar clientes";
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao carregar clientes: {ex.Message}", "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await ApplyFilterAsync();
    }

    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        SearchText = string.Empty;
        await ApplyFilterAsync();
    }

    [RelayCommand]
    private async Task AddClienteAsync()
    {
        if (!CanAddCliente)
        {
            await _navigationService.DisplayAlertAsync("Acesso Negado", "Você não tem permissão para adicionar clientes", "OK");
            return;
        }

        await _navigationService.NavigateToAsync("clientes/editar", new Dictionary<string, object>
        {
            ["IsNew"] = true
        });
    }

    [RelayCommand]
    private async Task ViewClienteAsync(ClienteLocal cliente)
    {
        if (cliente == null) return;

        await _navigationService.NavigateToAsync("clientes/detalhes", new Dictionary<string, object>
        {
            ["ClienteId"] = cliente.Id
        });
    }

    [RelayCommand]
    private async Task EditClienteAsync(ClienteLocal cliente)
    {
        if (cliente == null) return;

        if (!CanEditCliente)
        {
            await _navigationService.DisplayAlertAsync("Acesso Negado", "Você não tem permissão para editar clientes", "OK");
            return;
        }

        await _navigationService.NavigateToAsync("clientes/editar", new Dictionary<string, object>
        {
            ["ClienteId"] = cliente.Id,
            ["IsNew"] = false
        });
    }

    [RelayCommand]
    private async Task DeleteClienteAsync(ClienteLocal cliente)
    {
        if (cliente == null) return;

        if (!CanEditCliente)
        {
            await _navigationService.DisplayAlertAsync("Acesso Negado", "Você não tem permissão para excluir clientes", "OK");
            return;
        }

        var confirm = await _navigationService.DisplayConfirmAsync(
            "Confirmar Exclusão",
            $"Deseja realmente excluir o cliente '{cliente.Nome}'?",
            "Sim",
            "Não");

        if (confirm)
        {
            try
            {
                SetBusy(true);
                await _clienteService.DeleteAsync(cliente.Id);
                
                Clientes.Remove(cliente);
                await ApplyFilterAsync();
                
                TotalClientes = Clientes.Count;
                HasClientes = TotalClientes > 0;
                
                await _navigationService.DisplayAlertAsync("Sucesso", "Cliente excluído com sucesso", "OK");
            }
            catch (Exception ex)
            {
                await _navigationService.DisplayAlertAsync("Erro", $"Erro ao excluir cliente: {ex.Message}", "OK");
            }
            finally
            {
                SetBusy(false);
            }
        }
    }

    [RelayCommand]
    private async Task CallClienteAsync(ClienteLocal cliente)
    {
        if (cliente == null || string.IsNullOrEmpty(cliente.TelefoneCompleto)) return;

        try
        {
            // Simular abertura do discador - funcionalidade será implementada quando APIs estiverem disponíveis
            await _navigationService.DisplayAlertAsync("Ligar", $"Ligando para {cliente.TelefoneCompleto}", "OK");
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Não foi possível fazer a ligação: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task EmailClienteAsync(ClienteLocal cliente)
    {
        if (cliente == null || string.IsNullOrEmpty(cliente.Email)) return;

        try
        {
            var message = new Microsoft.Maui.ApplicationModel.Communication.EmailMessage
            {
                To = new List<string> { cliente.Email },
                Subject = "Contato - Órama Go"
            };
            await Microsoft.Maui.ApplicationModel.Communication.Email.ComposeAsync(message);
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Não foi possível enviar email: {ex.Message}", "OK");
        }
    }

    private async Task ApplyFilterAsync()
    {
        try
        {
            ClientesFiltrados.Clear();

            var filteredClientes = string.IsNullOrWhiteSpace(SearchText)
                ? Clientes
                : await _clienteService.SearchAsync(SearchText);

            foreach (var cliente in filteredClientes.OrderBy(c => c.Nome))
            {
                ClientesFiltrados.Add(cliente);
            }

            StatusMessage = ClientesFiltrados.Count > 0
                ? $"{ClientesFiltrados.Count} cliente(s) encontrado(s)"
                : string.IsNullOrWhiteSpace(SearchText) 
                    ? "Nenhum cliente cadastrado"
                    : "Nenhum cliente encontrado com os critérios de busca";
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao filtrar clientes: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task SyncClientesAsync()
    {
        if (IsSyncing) return;

        try
        {
            IsSyncing = true;
            StatusMessage = "Sincronizando clientes...";

            var result = await _clienteSyncService.SyncClientesAsync();
            
            if (result.Success)
            {
                await _navigationService.DisplayAlertAsync("Sucesso", result.Message, "OK");
                await LoadDataAsync(); // Recarregar dados após sincronização
            }
            else
            {
                var errorMessage = result.Message;
                if (result.Errors.Any())
                {
                    errorMessage += "\n\nErros:\n" + string.Join("\n", result.Errors.Take(3));
                    if (result.Errors.Count > 3)
                        errorMessage += $"\n... e mais {result.Errors.Count - 3} erro(s)";
                }
                
                await _navigationService.DisplayAlertAsync("Erro na Sincronização", errorMessage, "OK");
            }
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro durante sincronização: {ex.Message}", "OK");
        }
        finally
        {
            IsSyncing = false;
            await UpdateSyncStatusAsync();
        }
    }

    private async Task UpdateSyncStatusAsync()
    {
        try
        {
            PendingSyncCount = await _clienteSyncService.GetPendingCountAsync();
        }
        catch (Exception)
        {
            PendingSyncCount = 0;
        }
    }

    private void OnSyncStatusChanged(object? sender, SyncStatusChangedEventArgs e)
    {
        IsSyncing = e.CurrentStatus == Services.SyncStatus.Syncing;
        
        if (e.CurrentStatus == Services.SyncStatus.Success)
        {
            // Recarregar dados após sincronização bem-sucedida
            Task.Run(async () => await LoadDataAsync());
        }
    }

    private async Task CheckPermissionsAsync()
    {
        CanAddCliente = await _permissionService.HasPermissionAsync(OramaGo.Services.Permissions.ClientesIncluir);
        CanEditCliente = await _permissionService.HasPermissionAsync(OramaGo.Services.Permissions.ClientesAlterar);
    }

    partial void OnSearchTextChanged(string value)
    {
        // Aplicar filtro com delay para evitar muitas chamadas
        Task.Run(async () =>
        {
            await Task.Delay(300);
            if (SearchText == value) // Verificar se ainda é o mesmo texto
            {
                await ApplyFilterAsync();
            }
        });
    }
}