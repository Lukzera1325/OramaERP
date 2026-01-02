using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Orama.Application.Services;
using Orama.Web.Models.Api;

namespace Orama.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthApiController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUsuarioService _usuarioService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthApiController> _logger;

    public AuthApiController(
        IAuthService authService,
        IUsuarioService usuarioService,
        IConfiguration configuration,
        ILogger<AuthApiController> logger)
    {
        _authService = authService;
        _usuarioService = usuarioService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Autentica usuário do app mobile Órama Go
    /// </summary>
    /// <param name="request">Dados de login</param>
    /// <returns>Token JWT e informações do usuário</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginApiRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Dados inválidos",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            // Validar credenciais
            var usuario = await _authService.ValidarCredenciaisAsync(request.Email, request.Senha);
            if (usuario == null)
            {
                _logger.LogWarning("Tentativa de login inválida para email: {Email}", request.Email);
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Email ou senha inválidos"
                });
            }

            // Verificar se usuário tem permissão para Órama Go
            if (!usuario.Permissoes.Contains("OramaGo.Acesso"))
            {
                _logger.LogWarning("Usuário {Email} tentou acessar Órama Go sem permissão", request.Email);
                return StatusCode(403, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Usuário não possui permissão para acessar o Órama Go"
                });
            }

            // Verificar se usuário está ativo
            if (!usuario.Ativo)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Usuário inativo"
                });
            }

            // Gerar token JWT
            var token = GerarTokenJWT(usuario);

            var response = new LoginApiResponse
            {
                Token = token,
                Usuario = new UsuarioApiDto
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    EmpresaId = usuario.EmpresaId,
                    EmpresaNome = usuario.Empresa?.NomeExibicao ?? "",
                    Permissoes = usuario.Permissoes,
                    DataUltimoAcesso = DateTime.Now
                }
            };

            // Atualizar último acesso
            await _usuarioService.AtualizarUltimoAcessoAsync(usuario.Id);

            _logger.LogInformation("Login realizado com sucesso para usuário: {Email}", request.Email);

            return Ok(new ApiResponse<LoginApiResponse>
            {
                Success = true,
                Message = "Login realizado com sucesso",
                Data = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar login para email: {Email}", request.Email);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Valida token JWT
    /// </summary>
    /// <returns>Informações do usuário autenticado</returns>
    [HttpGet("validate")]
    public async Task<IActionResult> ValidateToken()
    {
        try
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Token não fornecido"
                });
            }

            var principal = ValidarTokenJWT(token);
            if (principal == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Token inválido"
                });
            }

            var userId = int.Parse(principal.FindFirst("UserId")?.Value ?? "0");
            var usuario = await _usuarioService.ObterPorIdAsync(userId);

            if (usuario == null || !usuario.Ativo)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Usuário não encontrado ou inativo"
                });
            }

            var response = new UsuarioApiDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                EmpresaId = usuario.EmpresaId,
                EmpresaNome = usuario.Empresa?.NomeExibicao ?? "",
                Permissoes = usuario.Permissoes,
                DataUltimoAcesso = usuario.DataUltimoAcesso
            };

            return Ok(new ApiResponse<UsuarioApiDto>
            {
                Success = true,
                Message = "Token válido",
                Data = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar token");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Logout do usuário
    /// </summary>
    /// <returns>Confirmação de logout</returns>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // Por enquanto, apenas retorna sucesso
            // Em implementações futuras, pode invalidar o token em uma blacklist
            
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Logout realizado com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar logout");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    private string GerarTokenJWT(Domain.Entities.Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "ChaveSecretaParaOramaGoMobile2024!@#"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("UserId", usuario.Id.ToString()),
            new Claim("Email", usuario.Email),
            new Claim("Nome", usuario.Nome),
            new Claim("EmpresaId", usuario.EmpresaId.ToString()),
            new Claim("EmpresaNome", usuario.Empresa?.NomeExibicao ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Adicionar permissões como claims
        var permissionClaims = usuario.Permissoes.Select(p => new Claim("Permission", p)).ToArray();
        var allClaims = claims.Concat(permissionClaims).ToArray();

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "OramaERP",
            audience: _configuration["Jwt:Audience"] ?? "OramaGoMobile",
            claims: allClaims,
            expires: DateTime.UtcNow.AddDays(30), // Token válido por 30 dias
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? ValidarTokenJWT(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "ChaveSecretaParaOramaGoMobile2024!@#"));
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "OramaERP",
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"] ?? "OramaGoMobile",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}