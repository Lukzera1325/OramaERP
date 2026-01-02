namespace Orama.Web.Models;

/// <summary>
/// ViewModel para seleção de empresa
/// </summary>
public class SelecionarEmpresaViewModel
{
    public List<EmpresaSimplificada> Empresas { get; set; } = new();
    public int? EmpresaSelecionada { get; set; }
    public string NomeUsuario { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }
}