using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities;

/// <summary>
/// Entidade que representa um usuário do sistema
/// </summary>
public class Usuario : BaseEntity
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [StringLength(150, ErrorMessage = "Email deve ter no máximo 150 caracteres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [StringLength(255, ErrorMessage = "Senha deve ter no máximo 255 caracteres")]
    public string Senha { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
    public string? Telefone { get; set; }

    public DateTime? UltimoLogin { get; set; }

    // Relacionamentos
    public int PerfilId { get; set; }
    public virtual Perfil Perfil { get; set; } = null!;
}