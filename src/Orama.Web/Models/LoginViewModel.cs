using System.ComponentModel.DataAnnotations;

namespace Orama.Web.Models;

/// <summary>
/// ViewModel para a tela de login
/// </summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;

    [Display(Name = "Lembrar-me")]
    public bool LembrarMe { get; set; }

    public string? ReturnUrl { get; set; }
}