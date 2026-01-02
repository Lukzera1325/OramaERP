using System.ComponentModel.DataAnnotations;

namespace Orama.Web.Models;

/// <summary>
/// ViewModel para permissões do sistema
/// </summary>
public class PermissaoViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nome da permissão é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    [Display(Name = "Nome da Permissão")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Módulo é obrigatório")]
    [StringLength(50, ErrorMessage = "Módulo deve ter no máximo 50 caracteres")]
    [Display(Name = "Módulo")]
    public string Modulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ação é obrigatória")]
    [StringLength(50, ErrorMessage = "Ação deve ter no máximo 50 caracteres")]
    [Display(Name = "Ação")]
    public string Acao { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Descrição deve ter no máximo 200 caracteres")]
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }

    [Display(Name = "Data de Criação")]
    public DateTime DataCriacao { get; set; }

    [Display(Name = "Última Atualização")]
    public DateTime? DataAtualizacao { get; set; }

    [Display(Name = "Ativo")]
    public bool Ativo { get; set; } = true;

    // Para exibição
    [Display(Name = "Perfis que Utilizam")]
    public int QuantidadePerfis { get; set; }

    public bool PodeExcluir { get; set; }
}

/// <summary>
/// ViewModel para listagem de permissões agrupadas por módulo
/// </summary>
public class PermissoesListViewModel
{
    public List<ModuloPermissoesListViewModel> Modulos { get; set; } = new();
    public string? FiltroModulo { get; set; }
    public string? FiltroAcao { get; set; }
}

/// <summary>
/// ViewModel para módulo na listagem
/// </summary>
public class ModuloPermissoesListViewModel
{
    public string NomeModulo { get; set; } = string.Empty;
    public List<PermissaoViewModel> Permissoes { get; set; } = new();
}