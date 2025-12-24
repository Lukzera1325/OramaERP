using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities;

/// <summary>
/// Entidade que representa uma permissão específica no sistema
/// Ex: "Clientes.Incluir", "Vendas.Alterar", "Financeiro.Excluir"
/// </summary>
public class Permissao : BaseEntity
{
    [Required(ErrorMessage = "Nome da permissão é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome da permissão deve ter no máximo 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Módulo é obrigatório")]
    [StringLength(50, ErrorMessage = "Módulo deve ter no máximo 50 caracteres")]
    public string Modulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ação é obrigatória")]
    [StringLength(50, ErrorMessage = "Ação deve ter no máximo 50 caracteres")]
    public string Acao { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Descrição deve ter no máximo 200 caracteres")]
    public string? Descricao { get; set; }

    // Relacionamentos
    public virtual ICollection<PerfilPermissao> PerfilPermissoes { get; set; } = new List<PerfilPermissao>();
}