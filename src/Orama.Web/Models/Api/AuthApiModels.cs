using System.ComponentModel.DataAnnotations;

namespace Orama.Web.Models.Api;

/// <summary>
/// Request para login na API
/// </summary>
public class LoginApiRequest
{
    /// <summary>
    /// Email do usuário
    /// </summary>
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Senha do usuário
    /// </summary>
    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Senha deve ter pelo menos 6 caracteres")]
    public string Senha { get; set; } = string.Empty;

    /// <summary>
    /// Informações do dispositivo (opcional)
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// Versão do app (opcional)
    /// </summary>
    public string? AppVersion { get; set; }
}

/// <summary>
/// Response do login na API
/// </summary>
public class LoginApiResponse
{
    /// <summary>
    /// Token JWT para autenticação
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Informações do usuário autenticado
    /// </summary>
    public UsuarioApiDto Usuario { get; set; } = new();

    /// <summary>
    /// Data de expiração do token
    /// </summary>
    public DateTime TokenExpiration { get; set; } = DateTime.UtcNow.AddHours(8);
}

/// <summary>
/// DTO do usuário para API
/// </summary>
public class UsuarioApiDto
{
    /// <summary>
    /// ID do usuário
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nome do usuário
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Email do usuário
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// ID da empresa
    /// </summary>
    public int EmpresaId { get; set; }

    /// <summary>
    /// Nome da empresa
    /// </summary>
    public string EmpresaNome { get; set; } = string.Empty;

    /// <summary>
    /// Lista de permissões do usuário
    /// </summary>
    public List<string> Permissoes { get; set; } = new();

    /// <summary>
    /// Data do último acesso
    /// </summary>
    public DateTime? DataUltimoAcesso { get; set; }

    /// <summary>
    /// Indica se o usuário está ativo
    /// </summary>
    public bool Ativo { get; set; } = true;
}
