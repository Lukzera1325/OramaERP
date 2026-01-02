namespace OramaGo.Services;

public class NavigationService : INavigationService
{
    public async Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        if (parameters != null)
        {
            await Shell.Current.GoToAsync(route, parameters);
        }
        else
        {
            await Shell.Current.GoToAsync(route);
        }
    }

    public async Task NavigateBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    public async Task PopToRootAsync()
    {
        await Shell.Current.GoToAsync("//");
    }

    public async Task DisplayAlertAsync(string title, string message, string cancel = "OK")
    {
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }
    }

    public async Task<bool> DisplayConfirmAsync(string title, string message, string accept = "Sim", string cancel = "Não")
    {
        if (Application.Current?.MainPage != null)
        {
            return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
        }
        return false;
    }

    public async Task<string?> DisplayPromptAsync(string title, string message, string placeholder = "", string initialValue = "")
    {
        if (Application.Current?.MainPage != null)
        {
            return await Application.Current.MainPage.DisplayPromptAsync(title, message, placeholder: placeholder, initialValue: initialValue);
        }
        return null;
    }
}