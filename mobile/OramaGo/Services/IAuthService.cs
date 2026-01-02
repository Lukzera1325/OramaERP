using OramaGo.Models;

namespace OramaGo.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<bool> LogoutAsync();
    Task<UserSession?> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<bool> RefreshTokenAsync();
    Task<bool> HasPermissionAsync(string permission);
    Task ClearSessionAsync();
    event EventHandler<bool>? AuthenticationChanged;
}