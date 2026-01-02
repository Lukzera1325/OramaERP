using System.ComponentModel.DataAnnotations;

namespace OramaGo.Models;

public class LoginRequest
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Senha deve ter pelo menos 6 caracteres")]
    public string Senha { get; set; } = string.Empty;

    public bool LembrarMe { get; set; }
}

public class LoginResponse
{
    public bool Sucesso { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UsuarioInfo? Usuario { get; set; }
    public string? Erro { get; set; }
}

public class UsuarioInfo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int EmpresaId { get; set; }
    public string EmpresaNome { get; set; } = string.Empty;
    public bool TemPermissaoOramaGo { get; set; }
    public List<string> Permissoes { get; set; } = new();
}

public class RefreshTokenRequest
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class UserSession
{
    public int UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int EmpresaId { get; set; }
    public string EmpresaNome { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime LoginAt { get; set; }
    public List<string> Permissoes { get; set; } = new();

    public bool IsTokenExpired => DateTime.Now >= ExpiresAt;
    public bool IsValid => !string.IsNullOrEmpty(Token) && !IsTokenExpired;
}