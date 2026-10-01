using Microsoft.Extensions.Logging;
using OramaGo.Models;
using System.Text.Json;
using System.Net.Http.Json;

namespace OramaGo.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthService> _logger;
    private UserSession? _currentUser;

    private const string TokenKey = "auth_token";
    private const string RefreshTokenKey = "refresh_token";
    private const string UserSessionKey = "user_session";

    public event EventHandler<bool>? AuthenticationChanged;

    public AuthService(HttpClient httpClient, ILogger<AuthService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation($"Tentando fazer login para: {request.Email}");

            using var httpResponse = await _httpClient.PostAsJsonAsync("api/AuthApi/login", new
            {
                request.Email,
                request.Senha
            });

            var envelope = await httpResponse.Content.ReadFromJsonAsync<ApiEnvelope<ApiLoginData>>();
            if (!httpResponse.IsSuccessStatusCode || envelope?.Success != true || envelope.Data == null)
            {
                return new LoginResponse
                {
                    Sucesso = false,
                    Erro = envelope?.Message ?? "Email ou senha inválidos"
                };
            }

            var payload = envelope.Data;
            var response = new LoginResponse
            {
                Sucesso = true,
                Token = payload.Token,
                ExpiresAt = payload.TokenExpiration,
                Usuario = new UsuarioInfo
                {
                    Id = payload.Usuario.Id,
                    Nome = payload.Usuario.Nome,
                    Email = payload.Usuario.Email,
                    EmpresaId = payload.Usuario.EmpresaId,
                    EmpresaNome = payload.Usuario.EmpresaNome,
                    TemPermissaoOramaGo = payload.Usuario.Permissoes.Contains("OramaGo.Acesso"),
                    Permissoes = payload.Usuario.Permissoes
                }
            };

            if (response.Sucesso && response.Usuario != null)
            {
                // Verificar se tem permissão para Órama Go
                if (!response.Usuario.TemPermissaoOramaGo)
                {
                    return new LoginResponse
                    {
                        Sucesso = false,
                        Erro = "Usuário não tem permissão para acessar o Órama Go"
                    };
                }

                // Salvar sessão
                var userSession = new UserSession
                {
                    UsuarioId = response.Usuario.Id,
                    Nome = response.Usuario.Nome,
                    Email = response.Usuario.Email,
                    EmpresaId = response.Usuario.EmpresaId,
                    EmpresaNome = response.Usuario.EmpresaNome,
                    Token = response.Token!,
                    RefreshToken = response.RefreshToken,
                    ExpiresAt = response.ExpiresAt ?? DateTime.Now.AddHours(8),
                    LoginAt = DateTime.Now,
                    Permissoes = response.Usuario.Permissoes
                };

                await SaveUserSessionAsync(userSession);
                _currentUser = userSession;

                // Configurar header de autorização
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userSession.Token);

                AuthenticationChanged?.Invoke(this, true);
                _logger.LogInformation($"Login realizado com sucesso para: {request.Email}");
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao fazer login para: {request.Email}");
            return new LoginResponse
            {
                Sucesso = false,
                Erro = "Erro interno. Tente novamente."
            };
        }
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            _logger.LogInformation("Fazendo logout...");

            // TODO: Chamar API para invalidar token quando estiver pronta

            await ClearSessionAsync();
            AuthenticationChanged?.Invoke(this, false);

            _logger.LogInformation("Logout realizado com sucesso");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer logout");
            return false;
        }
    }

    public async Task<UserSession?> GetCurrentUserAsync()
    {
        if (_currentUser != null && _currentUser.IsValid)
            return _currentUser;

        try
        {
            var sessionJson = await SecureStorage.GetAsync(UserSessionKey);
            if (!string.IsNullOrEmpty(sessionJson))
            {
                _currentUser = JsonSerializer.Deserialize<UserSession>(sessionJson);
                
                if (_currentUser?.IsValid == true)
                {
                    // Configurar header de autorização
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _currentUser.Token);
                    
                    return _currentUser;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao recuperar sessão do usuário");
        }

        return null;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.IsValid == true;
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return false;

            _logger.LogInformation("Renovando token...");

            // O backend ainda não possui endpoint de refresh; não prolongamos tokens localmente.
            await ClearSessionAsync();
            AuthenticationChanged?.Invoke(this, false);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao renovar token");
            return false;
        }
    }

    public async Task<bool> HasPermissionAsync(string permission)
    {
        var user = await GetCurrentUserAsync();
        return user?.Permissoes.Contains(permission) == true;
    }

    public async Task ClearSessionAsync()
    {
        try
        {
            SecureStorage.Remove(TokenKey);
            SecureStorage.Remove(RefreshTokenKey);
            SecureStorage.Remove(UserSessionKey);
            
            _currentUser = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao limpar sessão");
        }
    }

    private async Task SaveUserSessionAsync(UserSession session)
    {
        try
        {
            var sessionJson = JsonSerializer.Serialize(session);
            await SecureStorage.SetAsync(UserSessionKey, sessionJson);
            await SecureStorage.SetAsync(TokenKey, session.Token);
            
            if (!string.IsNullOrEmpty(session.RefreshToken))
                await SecureStorage.SetAsync(RefreshTokenKey, session.RefreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar sessão do usuário");
            throw;
        }
    }

    private sealed class ApiEnvelope<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }

    private sealed class ApiLoginData
    {
        public string Token { get; set; } = string.Empty;
        public DateTime TokenExpiration { get; set; }
        public ApiUser Usuario { get; set; } = new();
    }

    private sealed class ApiUser
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int EmpresaId { get; set; }
        public string EmpresaNome { get; set; } = string.Empty;
        public List<string> Permissoes { get; set; } = new();
    }
}
