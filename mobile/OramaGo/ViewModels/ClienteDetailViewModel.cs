using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OramaGo.Models;
using OramaGo.Services;

namespace OramaGo.ViewModels;

[QueryProperty(nameof(ClienteId), "ClienteId")]
public partial class ClienteDetailViewModel : BaseViewModel
{
    private readonly IClienteService _clienteService;
    private readonly INavigationService _navigationService;
    private readonly IPermissionService _permissionService;

    [ObservableProperty]
    private ClienteLocal? cliente;

    [ObservableProperty]
    private int clienteId;

    [ObservableProperty]
    private bool canEdit;

    [ObservableProperty]
    private bool hasCliente;

    public ClienteDetailViewModel(
        IClienteService clienteService,
        INavigationService navigationService,
        IPermissionService permissionService)
    {
        _clienteService = clienteService;
        _navigationService = navigationService;
        _permissionService = permissionService;
        
        Title = "Detalhes do Cliente";
    }

    public override async Task InitializeAsync()
    {
        await CheckPermissionsAsync();
        await LoadClienteAsync();
    }

    [RelayCommand]
    private async Task EditClienteAsync()
    {
        if (Cliente == null || !CanEdit) return;

        await _navigationService.NavigateToAsync("clientes/editar", new Dictionary<string, object>
        {
            ["ClienteId"] = Cliente.Id,
            ["IsNew"] = false
        });
    }

    [RelayCommand]
    private async Task CallClienteAsync()
    {
        if (Cliente == null || string.IsNullOrEmpty(Cliente.TelefoneCompleto)) return;

        try
        {
            // Simular abertura do discador - funcionalidade será implementada quando APIs estiverem disponíveis
            await _navigationService.DisplayAlertAsync("Ligar", $"Ligando para {Cliente.TelefoneCompleto}", "OK");
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Não foi possível fazer a ligação: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task EmailClienteAsync()
    {
        if (Cliente == null || string.IsNullOrEmpty(Cliente.Email)) return;

        try
        {
            var message = new Microsoft.Maui.ApplicationModel.Communication.EmailMessage
            {
                To = new List<string> { Cliente.Email },
                Subject = "Contato - Órama Go"
            };
            await Microsoft.Maui.ApplicationModel.Communication.Email.ComposeAsync(message);
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Não foi possível enviar email: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task ShareClienteAsync()
    {
        if (Cliente == null) return;

        try
        {
            var text = $"Cliente: {Cliente.Nome}\n";
            if (!string.IsNullOrEmpty(Cliente.CpfCnpj))
                text += $"CPF/CNPJ: {Cliente.CpfCnpjFormatado}\n";
            if (!string.IsNullOrEmpty(Cliente.Email))
                text += $"Email: {Cliente.Email}\n";
            if (!string.IsNullOrEmpty(Cliente.TelefoneCompleto))
                text += $"Telefone: {Cliente.TelefoneCompleto}\n";
            if (!string.IsNullOrEmpty(Cliente.EnderecoCompleto))
                text += $"Endereço: {Cliente.EnderecoCompleto}";

            await Microsoft.Maui.ApplicationModel.DataTransfer.Share.RequestAsync(new Microsoft.Maui.ApplicationModel.DataTransfer.ShareTextRequest
            {
                Text = text,
                Title = "Compartilhar Cliente"
            });
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Não foi possível compartilhar: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task ViewLocationAsync()
    {
        if (Cliente == null || string.IsNullOrEmpty(Cliente.Endereco)) return;

        try
        {
            // Simular abertura do mapa - funcionalidade será implementada quando APIs estiverem disponíveis
            await _navigationService.DisplayAlertAsync("Mapa", $"Abrindo mapa para: {Cliente.EnderecoCompleto}", "OK");
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Não foi possível abrir o mapa: {ex.Message}", "OK");
        }
    }

    private async Task LoadClienteAsync()
    {
        try
        {
            SetBusy(true);

            if (ClienteId > 0)
            {
                Cliente = await _clienteService.GetByIdAsync(ClienteId);
                HasCliente = Cliente != null;
                
                if (Cliente != null)
                {
                    Title = $"Cliente: {Cliente.Nome}";
                }
                else
                {
                    await _navigationService.DisplayAlertAsync("Erro", "Cliente não encontrado", "OK");
                    await _navigationService.NavigateBackAsync();
                }
            }
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao carregar cliente: {ex.Message}", "OK");
            await _navigationService.NavigateBackAsync();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task CheckPermissionsAsync()
    {
        CanEdit = await _permissionService.HasPermissionAsync(OramaGo.Services.Permissions.ClientesAlterar);
    }

    partial void OnClienteIdChanged(int value)
    {
        if (value > 0)
        {
            Task.Run(async () => await LoadClienteAsync());
        }
    }
}