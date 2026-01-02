using System.ComponentModel.DataAnnotations;

namespace Orama.Web.Models;

/// <summary>
/// ViewModel para cadastro e edição de usuários
/// </summary>
public class UsuarioViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [StringLength(150, ErrorMessage = "Email deve ter no máximo 150 caracteres")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [Required(ErrorMessage = "Perfil é obrigatório")]
    [Display(Name = "Perfil")]
    public int PerfilId { get; set; }

    [Display(Name = "Senha")]
    [DataType(DataType.Password)]
    public string? Senha { get; set; }

    [Display(Name = "Confirmar Senha")]
    [DataType(DataType.Password)]
    [Compare("Senha", ErrorMessage = "As senhas não coincidem")]
    public string? ConfirmarSenha { get; set; }

    // Propriedades para exibição
    public string? NomePerfil { get; set; }
    public DateTime? UltimoLogin { get; set; }
    public DateTime DataCriacao { get; set; }
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Indica se é um novo usuário (para validação de senha obrigatória)
    /// </summary>
    public bool IsNovoUsuario => Id == 0;
}