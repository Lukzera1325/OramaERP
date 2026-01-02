namespace OramaGo.Services;

public interface INavigationService
{
    Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null);
    Task NavigateBackAsync();
    Task PopToRootAsync();
    Task DisplayAlertAsync(string title, string message, string cancel = "OK");
    Task<bool> DisplayConfirmAsync(string title, string message, string accept = "Sim", string cancel = "Não");
    Task<string?> DisplayPromptAsync(string title, string message, string placeholder = "", string initialValue = "");
}