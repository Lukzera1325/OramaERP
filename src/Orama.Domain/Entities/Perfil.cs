using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities;

/// <summary>
/// Entidade que representa um perfil de acesso no sistema
/// Ex: Administrador, Financeiro, Vendas, Compras, etc.
/// </summary>
public class Perfil : BaseEntity
{
    [Required(ErrorMessage = "Nome do perfil é obrigatório")]
    [StringLength(50, ErrorMessage = "Nome do perfil deve ter no máximo 50 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Descrição deve ter no máximo 200 caracteres")]
    public string? Descricao { get; set; }

    // Relacionamentos
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public virtual ICollection<PerfilPermissao> PerfilPermissoes { get; set; } = new List<PerfilPermissao>();
}