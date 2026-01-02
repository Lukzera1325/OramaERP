using CommunityToolkit.Mvvm.Input;
using OramaGo.Services;

namespace OramaGo.ViewModels;

public partial class AppShellViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;

    public AppShellViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        Title = "Órama Go";
    }

    [RelayCommand]
    private async Task SyncAsync()
    {
        try
        {
            SetBusy(true);
            await _navigationService.DisplayAlertAsync("Sincronização", "Funcionalidade em desenvolvimento", "OK");
            // TODO: Implementar sincronização
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro na sincronização: {ex.Message}", "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    [RelayCommand]
    private async Task SettingsAsync()
    {
        await _navigationService.NavigateToAsync("//settings");
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var confirm = await _navigationService.DisplayConfirmAsync(
            "Sair", 
            "Deseja realmente sair do aplicativo?", 
            "Sim", 
            "Não");

        if (confirm)
        {
            // TODO: Implementar logout
            await _navigationService.DisplayAlertAsync("Logout", "Funcionalidade em desenvolvimento", "OK");
        }
    }
}