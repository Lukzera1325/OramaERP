using Microsoft.Extensions.Logging;
using OramaGo.Models;
using System.Text.Json;

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

            // TODO: Implementar chamada real para API quando estiver pronta
            // Por enquanto, simular login para desenvolvimento
            var response = await SimulateLoginAsync(request);

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
            if (currentUser?.RefreshToken == null)
                return false;

            _logger.LogInformation("Renovando token...");

            // TODO: Implementar chamada real para API quando estiver pronta
            // Por enquanto, simular renovação
            var newExpiry = DateTime.Now.AddHours(8);
            currentUser.ExpiresAt = newExpiry;

            await SaveUserSessionAsync(currentUser);
            _currentUser = currentUser;

            _logger.LogInformation("Token renovado com sucesso");
            return true;
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

    // Método temporário para simular login durante desenvolvimento
    private async Task<LoginResponse> SimulateLoginAsync(LoginRequest request)
    {
        await Task.Delay(1000); // Simular delay de rede

        // Credenciais de teste
        if (request.Email == "admin@orama.com.br" && request.Senha == "Admin@123")
        {
            return new LoginResponse
            {
                Sucesso = true,
                Token = "fake_jwt_token_" + Guid.NewGuid().ToString("N")[..16],
                RefreshToken = "fake_refresh_token_" + Guid.NewGuid().ToString("N")[..16],
                ExpiresAt = DateTime.Now.AddHours(8),
                Usuario = new UsuarioInfo
                {
                    Id = 1,
                    Nome = "Administrador",
                    Email = request.Email,
                    EmpresaId = 1,
                    EmpresaNome = "Empresa Demo",
                    TemPermissaoOramaGo = true,
                    Permissoes = new List<string> 
                    { 
                        "Vendas.Visualizar", 
                        "Vendas.Incluir", 
                        "Vendas.Alterar",
                        "Clientes.Visualizar",
                        "Clientes.Incluir",
                        "Clientes.Alterar",
                        "Produtos.Visualizar"
                    }
                }
            };
        }

        if (request.Email == "vendedor@orama.com.br" && request.Senha == "123456")
        {
            return new LoginResponse
            {
                Sucesso = true,
                Token = "fake_jwt_token_" + Guid.NewGuid().ToString("N")[..16],
                RefreshToken = "fake_refresh_token_" + Guid.NewGuid().ToString("N")[..16],
                ExpiresAt = DateTime.Now.AddHours(8),
                Usuario = new UsuarioInfo
                {
                    Id = 2,
                    Nome = "João Vendedor",
                    Email = request.Email,
                    EmpresaId = 1,
                    EmpresaNome = "Empresa Demo",
                    TemPermissaoOramaGo = true,
                    Permissoes = new List<string> 
                    { 
                        "Vendas.Visualizar", 
                        "Vendas.Incluir",
                        "Clientes.Visualizar",
                        "Clientes.Incluir",
                        "Produtos.Visualizar"
                    }
                }
            };
        }

        if (request.Email == "semacesso@orama.com.br" && request.Senha == "123456")
        {
            return new LoginResponse
            {
                Sucesso = true,
                Token = "fake_jwt_token_" + Guid.NewGuid().ToString("N")[..16],
                Usuario = new UsuarioInfo
                {
                    Id = 3,
                    Nome = "Usuário Sem Acesso",
                    Email = request.Email,
                    EmpresaId = 1,
                    EmpresaNome = "Empresa Demo",
                    TemPermissaoOramaGo = false, // Não tem permissão
                    Permissoes = new List<string>()
                }
            };
        }

        return new LoginResponse
        {
            Sucesso = false,
            Erro = "Email ou senha inválidos"
        };
    }
}