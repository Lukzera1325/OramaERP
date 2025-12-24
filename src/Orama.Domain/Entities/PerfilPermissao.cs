namespace Orama.Domain.Entities;

/// <summary>
/// Entidade de relacionamento entre Perfil e Permissão
/// Permite controle granular de permissões por perfil
/// </summary>
public class PerfilPermissao : BaseEntity
{
    public int PerfilId { get; set; }
    public virtual Perfil Perfil { get; set; } = null!;

    public int PermissaoId { get; set; }
    public virtual Permissao Permissao { get; set; } = null!;

    /// <summary>
    /// Indica se a permissão está concedida (true) ou negada (false)
    /// </summary>
    public bool Concedida { get; set; } = true;
}