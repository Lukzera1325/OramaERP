using System.ComponentModel.DataAnnotations;

namespace Orama.Web.Models;

/// <summary>
/// ViewModel para perfis de usuário
/// </summary>
public class PerfilViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nome do perfil é obrigatório")]
    [StringLength(50, ErrorMessage = "Nome deve ter no máximo 50 caracteres")]
    [Display(Name = "Nome do Perfil")]
    public string Nome { get; set; } = string.Empty;

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
    [Display(Name = "Usuários Vinculados")]
    public int QuantidadeUsuarios { get; set; }

    [Display(Name = "Permissões")]
    public int QuantidadePermissoes { get; set; }

    public bool PodeExcluir { get; set; }
}

/// <summary>
/// ViewModel para gerenciar permissões de um perfil
/// </summary>
public class PerfilPermissoesViewModel
{
    public int PerfilId { get; set; }
    public string NomePerfil { get; set; } = string.Empty;
    public string? DescricaoPerfil { get; set; }

    public List<ModuloPermissoesViewModel> Modulos { get; set; } = new();
}

/// <summary>
/// ViewModel para agrupar permissões por módulo
/// </summary>
public class ModuloPermissoesViewModel
{
    public string NomeModulo { get; set; } = string.Empty;
    public List<PermissaoCheckboxViewModel> Permissoes { get; set; } = new();
}

/// <summary>
/// ViewModel para checkbox de permissão
/// </summary>
public class PermissaoCheckboxViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Acao { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Selecionada { get; set; }
}