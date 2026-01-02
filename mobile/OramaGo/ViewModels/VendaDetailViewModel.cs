using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OramaGo.Models;
using OramaGo.Services;
using System.Collections.ObjectModel;

namespace OramaGo.ViewModels;

public partial class VendaDetailViewModel : BaseViewModel
{
    private readonly IVendaService _vendaService;
    private readonly IVendaItemService _vendaItemService;
    private readonly INavigationService _navigationService;
    private readonly IPermissionService _permissionService;

    [ObservableProperty]
    private VendaLocal? venda;

    [ObservableProperty]
    private ObservableCollection<VendaItemLocal> itens = new();

    [ObservableProperty]
    private bool canEdit = true;

    [ObservableProperty]
    private bool canApprove = true;

    [ObservableProperty]
    private bool canCancel = true;

    [ObservableProperty]
    private bool canInvoice = true;

    public int VendaId { get; set; }

    public VendaDetailViewModel(
        IVendaService vendaService,
        IVendaItemService vendaItemService,
        INavigationService navigationService,
        IPermissionService permissionService)
    {
        _vendaService = vendaService;
        _vendaItemService = vendaItemService;
        _navigationService = navigationService;
        _permissionService = permissionService;
        
        Title = "Detalhes da Venda";
    }

    public override async Task InitializeAsync()
    {
        await CheckPermissionsAsync();
        await LoadVendaAsync();
    }

    protected override async Task LoadDataAsync()
    {
        await LoadVendaAsync();
    }

    private async Task LoadVendaAsync()
    {
        if (VendaId == 0) return;

        try
        {
            SetBusy(true);
            
            var venda = await _vendaService.GetByIdAsync(VendaId);
            if (venda == null)
            {
                await _navigationService.DisplayAlertAsync("Erro", "Venda não encontrada", "OK");
                await _navigationService.NavigateBackAsync();
                return;
            }

            Venda = venda;
            Title = $"Venda {venda.Numero}";
            
            // Carregar itens
            await LoadItensAsync();
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao carregar venda: {ex.Message}", "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task LoadItensAsync()
    {
        if (Venda?.Id == 0) return;

        try
        {
            var itensList = await _vendaItemService.GetByVendaIdAsync(Venda.Id);
            
            Itens.Clear();
            foreach (var item in itensList)
            {
                Itens.Add(item);
            }
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao carregar itens: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task EditVendaAsync()
    {
        if (Venda == null || !CanEdit) return;
        
        if (!Venda.PodeEditar)
        {
            await _navigationService.DisplayAlertAsync("Atenção", "Esta venda não pode ser editada", "OK");
            return;
        }
        
        await _navigationService.NavigateToAsync("vendas/editar", new Dictionary<string, object>
        {
            ["VendaId"] = Venda.Id
        });
    }

    [RelayCommand]
    private async Task AprovarVendaAsync()
    {
        if (Venda == null || !CanApprove) return;
        
        if (!Venda.PodeAprovar)
        {
            await _navigationService.DisplayAlertAsync("Atenção", "Esta venda não pode ser aprovada", "OK");
            return;
        }
        
        var confirm = await _navigationService.DisplayConfirmAsync(
            "Confirmar Aprovação",
            $"Deseja aprovar a venda {Venda.Numero}?",
            "Sim",
            "Não");
        
        if (confirm)
        {
            try
            {
                SetBusy(true);
                Venda = await _vendaService.AprovarVendaAsync(Venda.Id);
                
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
    private async Task CancelarVendaAsync()
    {
        if (Venda == null || !CanCancel) return;
        
        if (!Venda.PodeCancelar)
        {
            await _navigationService.DisplayAlertAsync("Atenção", "Esta venda não pode ser cancelada", "OK");
            return;
        }
        
        var motivo = await _navigationService.DisplayPromptAsync(
            "Cancelar Venda",
            "Informe o motivo do cancelamento:",
            "Motivo...");
        
        if (!string.IsNullOrWhiteSpace(motivo))
        {
            try
            {
                SetBusy(true);
                Venda = await _vendaService.CancelarVendaAsync(Venda.Id, motivo);
                
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

    [RelayCommand]
    private async Task FaturarVendaAsync()
    {
        if (Venda == null || !CanInvoice) return;
        
        if (!Venda.PodeFaturar)
        {
            await _navigationService.DisplayAlertAsync("Atenção", "Esta venda não pode ser faturada", "OK");
            return;
        }
        
        var confirm = await _navigationService.DisplayConfirmAsync(
            "Confirmar Faturamento",
            $"Deseja faturar a venda {Venda.Numero}?",
            "Sim",
            "Não");
        
        if (confirm)
        {
            try
            {
                SetBusy(true);
                Venda = await _vendaService.FaturarVendaAsync(Venda.Id);
                
                await _navigationService.DisplayAlertAsync("Sucesso", "Venda faturada com sucesso", "OK");
            }
            catch (Exception ex)
            {
                await _navigationService.DisplayAlertAsync("Erro", $"Erro ao faturar venda: {ex.Message}", "OK");
            }
            finally
            {
                SetBusy(false);
            }
        }
    }

    [RelayCommand]
    private async Task ShareVendaAsync()
    {
        if (Venda == null) return;
        
        try
        {
            var text = $"Venda {Venda.Numero}\n" +
                      $"Cliente: {Venda.Cliente?.Nome}\n" +
                      $"Data: {Venda.DataVenda:dd/MM/yyyy}\n" +
                      $"Status: {Venda.StatusDescricao}\n" +
                      $"Valor Total: {Venda.ValorTotal:C2}\n" +
                      $"Itens: {Itens.Count}";
            
            await Share.RequestAsync(new ShareTextRequest
            {
                Text = text,
                Title = $"Venda {Venda.Numero}"
            });
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao compartilhar: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task CopyVendaInfoAsync()
    {
        if (Venda == null) return;
        
        try
        {
            var text = $"Venda {Venda.Numero} - {Venda.Cliente?.Nome} - {Venda.ValorTotal:C2}";
            await Clipboard.SetTextAsync(text);
            
            await _navigationService.DisplayAlertAsync("Sucesso", "Informações copiadas para a área de transferência", "OK");
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao copiar: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task ReloadDataAsync()
    {
        await LoadVendaAsync();
    }

    private async Task CheckPermissionsAsync()
    {
        CanEdit = await _permissionService.HasPermissionAsync(OramaGo.Services.Permissions.VendasAlterar);
        CanApprove = await _permissionService.HasPermissionAsync(OramaGo.Services.Permissions.VendasAprovar);
        CanCancel = await _permissionService.HasPermissionAsync(OramaGo.Services.Permissions.VendasExcluir);
        CanInvoice = await _permissionService.HasPermissionAsync(OramaGo.Services.Permissions.VendasFaturar);
    }
}