using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Orama.Web.Models;

/// <summary>
/// ViewModel para criação e edição de produtos
/// </summary>
public class ProdutoViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Código é obrigatório")]
    [StringLength(50, ErrorMessage = "Código deve ter no máximo 50 caracteres")]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrição é obrigatória")]
    [StringLength(200, ErrorMessage = "Descrição deve ter no máximo 200 caracteres")]
    [Display(Name = "Descrição")]
    public string Descricao { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Descrição detalhada deve ter no máximo 500 caracteres")]
    [Display(Name = "Descrição Detalhada")]
    public string? DescricaoDetalhada { get; set; }

    [Required(ErrorMessage = "Unidade é obrigatória")]
    [StringLength(10, ErrorMessage = "Unidade deve ter no máximo 10 caracteres")]
    [Display(Name = "Unidade")]
    public string Unidade { get; set; } = "UN";

    [StringLength(10, ErrorMessage = "NCM deve ter no máximo 10 caracteres")]
    [Display(Name = "NCM")]
    public string? Ncm { get; set; }

    [StringLength(10, ErrorMessage = "CEST deve ter no máximo 10 caracteres")]
    [Display(Name = "CEST")]
    public string? Cest { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Preço de custo deve ser maior ou igual a zero")]
    [Display(Name = "Preço de Custo")]
    [DisplayFormat(DataFormatString = "{0:N2}")]
    public decimal PrecoCusto { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Preço de venda deve ser maior ou igual a zero")]
    [Display(Name = "Preço de Venda")]
    [DisplayFormat(DataFormatString = "{0:N2}")]
    public decimal PrecoVenda { get; set; }

    [Range(0, 100, ErrorMessage = "Margem deve estar entre 0 e 100")]
    [Display(Name = "Margem de Lucro (%)")]
    [DisplayFormat(DataFormatString = "{0:N2}")]
    public decimal MargemLucro { get; set; }

    [Display(Name = "Controla Estoque")]
    public bool ControlaEstoque { get; set; } = true;

    [Range(0, double.MaxValue, ErrorMessage = "Estoque atual deve ser maior ou igual a zero")]
    [Display(Name = "Estoque Atual")]
    [DisplayFormat(DataFormatString = "{0:N2}")]
    public decimal EstoqueAtual { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Estoque mínimo deve ser maior ou igual a zero")]
    [Display(Name = "Estoque Mínimo")]
    [DisplayFormat(DataFormatString = "{0:N2}")]
    public decimal EstoqueMinimo { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Estoque máximo deve ser maior ou igual a zero")]
    [Display(Name = "Estoque Máximo")]
    [DisplayFormat(DataFormatString = "{0:N2}")]
    public decimal EstoqueMaximo { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Peso deve ser maior ou igual a zero")]
    [Display(Name = "Peso (kg)")]
    [DisplayFormat(DataFormatString = "{0:N3}")]
    public decimal? Peso { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Altura deve ser maior ou igual a zero")]
    [Display(Name = "Altura (cm)")]
    [DisplayFormat(DataFormatString = "{0:N2}")]
    public decimal? Altura { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Largura deve ser maior ou igual a zero")]
    [Display(Name = "Largura (cm)")]
    [DisplayFormat(DataFormatString = "{0:N2}")]
    public decimal? Largura { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Profundidade deve ser maior ou igual a zero")]
    [Display(Name = "Profundidade (cm)")]
    [DisplayFormat(DataFormatString = "{0:N2}")]
    public decimal? Profundidade { get; set; }

    [Display(Name = "Categoria")]
    public int? CategoriaId { get; set; }

    [StringLength(500, ErrorMessage = "Observações deve ter no máximo 500 caracteres")]
    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }

    // Lista de categorias para dropdown
    public SelectList? Categorias { get; set; }

    // Lista de unidades comuns
    public static List<SelectListItem> UnidadesComuns => new()
    {
        new SelectListItem("UN - Unidade", "UN"),
        new SelectListItem("PC - Peça", "PC"),
        new SelectListItem("CX - Caixa", "CX"),
        new SelectListItem("KG - Quilograma", "KG"),
        new SelectListItem("G - Grama", "G"),
        new SelectListItem("L - Litro", "L"),
        new SelectListItem("ML - Mililitro", "ML"),
        new SelectListItem("M - Metro", "M"),
        new SelectListItem("M² - Metro Quadrado", "M2"),
        new SelectListItem("M³ - Metro Cúbico", "M3"),
        new SelectListItem("PAR - Par", "PAR"),
        new SelectListItem("DZ - Dúzia", "DZ"),
        new SelectListItem("PCT - Pacote", "PCT"),
        new SelectListItem("FD - Fardo", "FD"),
        new SelectListItem("RL - Rolo", "RL")
    };
}
