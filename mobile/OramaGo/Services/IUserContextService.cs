using OramaGo.Models;

namespace OramaGo.Services;

public interface IUserContextService
{
    Task<UserSession?> GetCurrentUserAsync();
    Task<int> GetCurrentEmpresaIdAsync();
    Task<string> GetCurrentUserNameAsync();
    Task<string> GetCurrentUserEmailAsync();
    Task<bool> IsUserLoggedInAsync();
    Task SetCurrentUserAsync(UserSession user);
    Task ClearCurrentUserAsync();
    event EventHandler<UserSession?>? UserChanged;
}