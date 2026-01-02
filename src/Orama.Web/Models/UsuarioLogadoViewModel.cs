namespace Orama.Web.Models;

/// <summary>
/// ViewModel com informações do usuário logado
/// </summary>
public class UsuarioLogadoViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
    public DateTime? UltimoLogin { get; set; }
    public IEnumerable<string> Permissoes { get; set; } = new List<string>();
    
    // Multi-Tenant
    public bool IsSuperAdmin { get; set; }
    public int EmpresaId { get; set; }
    public string EmpresaNome { get; set; } = string.Empty;
    public List<EmpresaSimplificada> EmpresasDisponiveis { get; set; } = new();
}

/// <summary>
/// Dados simplificados de uma empresa
/// </summary>
public class EmpresaSimplificada
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
}