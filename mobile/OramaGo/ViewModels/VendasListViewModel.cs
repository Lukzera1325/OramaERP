using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OramaGo.Models;
using OramaGo.Services;
using System.Collections.ObjectModel;

namespace OramaGo.ViewModels;

public partial class VendasListViewModel : BaseViewModel
{
    private readonly IVendaService _vendaService;
    private readonly INavigationService _navigationService;
    private readonly IPermissionService _permissionService;

    [ObservableProperty]
    private ObservableCollection<VendaLocal> vendas = new();

    [ObservableProperty]
    private VendaLocal? vendaSelecionada;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private StatusVendaLocal? statusFiltro;

    [ObservableProperty]
    private DateTime? dataInicio;

    [ObservableProperty]
    private DateTime? dataFim;

    [ObservableProperty]
    private bool canCreate = true;

    [ObservableProperty]
    private bool canEdit = true;

    [ObservableProperty]
    private bool canDelete = true;

    [ObservableProperty]
    private int totalVendas;

    [ObservableProperty]
    private decimal valorTotalVendas;

    public List<StatusVendaLocal?> StatusOptions { get; } = new()
    {
        null, // Todos
        StatusVendaLocal.Orcamento,
        StatusVendaLocal.Aprovado,
        StatusVendaLocal.Faturado,
        StatusVendaLocal.Entregue,
        StatusVendaLocal.Cancelado
    };

    public VendasListViewModel(
        IVendaService vendaService,
        INavigationService navigationService,
        IPermissionService permissionService)
    {
        _vendaService = vendaService;
        _navigationService = navigationService;
        _permissionService = permissionService;
        
        Title = "Vendas";
        
        // Definir período padrão (último mês)
        DataFim = DateTime.Today;
        DataInicio = DateTime.Today.AddDays(-30);
    }

    public override async Task InitializeAsync()
    {
        await CheckPermissionsAsync();
        await LoadVendasAsync();
    }

    protected override async Task LoadDataAsync()
    {
        await LoadVendasAsync();
    }

    [RelayCommand]
    private async Task LoadVendasAsync()
    {
        try
        {
            SetBusy(true);
            
            IEnumerable<VendaLocal> vendasList;
            
            // Aplicar filtros
            if (DataInicio.HasValue && DataFim.HasValue)
            {
                vendasList = await _vendaService.GetByPeriodoAsync(DataInicio.Value, DataFim.Value);
            }
            else
            {
                vendasList = await _vendaService.GetAllAsync();
            }
            
            // Filtrar por status se selecionado
            if (StatusFiltro.HasValue)
            {
                vendasList = vendasList.Where(v => v.Status == StatusFiltro.Value);
            }
            
            // Filtrar por texto de busca
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                vendasList = vendasList.Where(v => 
                    v.Numero.ToLower().Contains(searchLower) ||
                    (v.Cliente?.Nome?.ToLower().Contains(searchLower) ?? false) ||
                    (v.Observacoes?.ToLower().Contains(searchLower) ?? false));
            }
            
            // Ordenar por data (mais recentes primeiro)
            vendasList = vendasList.OrderByDescending(v => v.DataVenda);
            
            Vendas.Clear();
            foreach (var venda in vendasList)
            {
                Vendas.Add(venda);
            }
            
            // Calcular totais
            TotalVendas = Vendas.Count;
            ValorTotalVendas = Vendas.Where(v => v.Status != StatusVendaLocal.Cancelado).Sum(v => v.ValorTotal);
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao carregar vendas: {ex.Message}", "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    [RelayCommand]
    private async Task SearchVendasAsync()
    {
        await LoadVendasAsync();
    }

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        StatusFiltro = null;
        DataInicio = DateTime.Today.AddDays(-30);
        DataFim = DateTime.Today;
        
        await LoadVendasAsync();
    }

    [RelayCommand]
    private async Task CreateVendaAsync()
    {
        if (!CanCreate) return;
        
        await _navigationService.NavigateToAsync("vendas/criar");
    }

    [RelayCommand]
    private async Task ViewVendaAsync(VendaLocal venda)
    {
        if (venda == null) return;
        
        await _navigationService.NavigateToAsync("vendas/detalhes", new Dictionary<string, object>
        {
            ["VendaId"] = venda.Id
        });
    }

    [RelayCommand]
    private async Task EditVendaAsync(VendaLocal venda)
    {
        if (venda == null || !CanEdit) return;
        
        if (!venda.PodeEditar)
        {
            await _navigationService.DisplayAlertAsync("Atenção", "Esta venda não pode ser editada", "OK");
            return;
        }
        
        await _navigationService.NavigateToAsync("vendas/editar", new Dictionary<string, object>
        {
            ["VendaId"] = venda.Id
        });
    }

    [RelayCommand]
    private async Task DeleteVendaAsync(VendaLocal venda)
    {
        if (venda == null || !CanDelete) return;
        
        if (!venda.PodeCancelar)
        {
            await _navigationService.DisplayAlertAsync("Atenção", "Esta venda não pode ser cancelada", "OK");
            return;
        }
        
        var confirm = await _navigationService.DisplayConfirmAsync(
            "Confirmar Cancelamento",
            $"Deseja cancelar a venda {venda.Numero}?",
            "Sim",
            "Não");
        
        if (confirm)
        {
            var motivo = await _navigationService.DisplayPromptAsync(
                "Motivo do Cancelamento",
                "Informe o motivo:",
                "Motivo...");
            
            if (!string.IsNullOrWhiteSpace(motivo))
            {
                try
                {
                    SetBusy(true);
                    await _vendaService.CancelarVendaAsync(venda.Id, motivo);
                    await LoadVendasAsync();
                    
                    await _navigationService.DisplayAlertAsync("Sucesso", "Venda cancelada com sucesso", "OK");
                }
                catch (Exception ex)
                {
                    await _navigationService.DisplayAlertAsync("Erro", $"Erro ao cancelar venda: {ex.Message}", "OK");
                }
                finally
                {
                    SetBusy(false);
                }
            }
        }
    }

    [RelayCommand]
    private async Task AprovarVendaAsync(VendaLocal venda)
    {
        if (venda == null) return;
        
        if (!venda.PodeAprovar)
        {
            await _navigationService.DisplayAlertAsync("Atenção", "Esta venda não pode ser aprovada", "OK");
            return;
        }
        
        var confirm = await _navigationService.DisplayConfirmAsync(
            "Confirmar Aprovação",
            $"Deseja aprovar a venda {venda.Numero}?",
            "Sim",
            "Não");
        
        if (confirm)
        {
            try
            {
                SetBusy(true);
                await _vendaService.AprovarVendaAsync(venda.Id);
                await LoadVendasAsync();
                
                await _navigationService.DisplayAlertAsync("Sucesso", "Venda aprovada com sucesso", "OK");
            }
            catch (Exception ex)
            {
                await _navigationService.DisplayAlertAsync("Erro", $"Erro ao aprovar venda: {ex.Message}", "OK");
            }
            finally
            {
                SetBusy(false);
            }
        }
    }

    [RelayCommand]
    private async Task ReloadDataAsync()
    {
        await LoadVendasAsync();
    }

    private async Task CheckPermissionsAsync()
    {
        CanCreate = await _permissionService.HasPermissionAsync(OramaGo.Services.Permissions.VendasIncluir);
        CanEdit = await _permissionService.HasPermissionAsync(OramaGo.Services.Permissions.VendasAlterar);
        CanDelete = await _permissionService.HasPermissionAsync(OramaGo.Services.Permissions.VendasExcluir);
    }

    partial void OnVendaSelecionadaChanged(VendaLocal? value)
    {
        if (value != null)
        {
            // Auto-navegar para detalhes quando selecionado
            _ = Task.Run(async () => await ViewVendaAsync(value));
        }
    }

    partial void OnStatusFiltroChanged(StatusVendaLocal? value)
    {
        _ = Task.Run(async () => await LoadVendasAsync());
    }

    partial void OnDataInicioChanged(DateTime? value)
    {
        if (value.HasValue && DataFim.HasValue && value > DataFim)
        {
            DataFim = value.Value.AddDays(30);
        }
    }

    partial void OnDataFimChanged(DateTime? value)
    {
        if (value.HasValue && DataInicio.HasValue && value < DataInicio)
        {
            DataInicio = value.Value.AddDays(-30);
        }
    }
}