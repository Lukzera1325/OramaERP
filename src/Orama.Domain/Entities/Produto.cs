using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities;

/// <summary>
/// Entidade que representa um produto
/// </summary>
public class Produto : BaseEntity
{
    // Multi-Tenant
    public int EmpresaId { get; set; }
    public virtual Empresa? Empresa { get; set; }

    [Required(ErrorMessage = "Código é obrigatório")]
    [StringLength(50, ErrorMessage = "Código deve ter no máximo 50 caracteres")]
    public string Codigo { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Código de barras deve ter no máximo 50 caracteres")]
    public string? CodigoBarras { get; set; }

    [Required(ErrorMessage = "Descrição é obrigatória")]
    [StringLength(200, ErrorMessage = "Descrição deve ter no máximo 200 caracteres")]
    public string Descricao { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Descrição detalhada deve ter no máximo 500 caracteres")]
    public string? DescricaoDetalhada { get; set; }

    [Required(ErrorMessage = "Unidade é obrigatória")]
    [StringLength(10, ErrorMessage = "Unidade deve ter no máximo 10 caracteres")]
    public string Unidade { get; set; } = string.Empty;

    // Classificação Fiscal
    [StringLength(10, ErrorMessage = "NCM deve ter no máximo 10 caracteres")]
    public string? Ncm { get; set; }

    [StringLength(10, ErrorMessage = "CEST deve ter no máximo 10 caracteres")]
    public string? Cest { get; set; }

    // Preços
    [Range(0, double.MaxValue, ErrorMessage = "Preço de custo deve ser maior ou igual a zero")]
    public decimal PrecoCusto { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Preço de venda deve ser maior ou igual a zero")]
    public decimal PrecoVenda { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Preço mínimo deve ser maior ou igual a zero")]
    public decimal PrecoMinimo { get; set; }

    [Range(0, 100, ErrorMessage = "Margem deve estar entre 0 e 100")]
    public decimal MargemLucro { get; set; }

    // Controle de Estoque
    public bool ControlaEstoque { get; set; } = true;

    [Range(0, double.MaxValue, ErrorMessage = "Estoque atual deve ser maior ou igual a zero")]
    public decimal EstoqueAtual { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Estoque mínimo deve ser maior ou igual a zero")]
    public decimal EstoqueMinimo { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Estoque máximo deve ser maior ou igual a zero")]
    public decimal EstoqueMaximo { get; set; }

    // Dimensões e Peso
    [Range(0, double.MaxValue, ErrorMessage = "Peso deve ser maior ou igual a zero")]
    public decimal? Peso { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Altura deve ser maior ou igual a zero")]
    public decimal? Altura { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Largura deve ser maior ou igual a zero")]
    public decimal? Largura { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Profundidade deve ser maior ou igual a zero")]
    public decimal? Profundidade { get; set; }

    // Relacionamentos
    public int? CategoriaId { get; set; }
    public virtual Categoria? CategoriaNavigation { get; set; }

    // Categoria como string para compatibilidade com API
    public string? Categoria { get; set; }

    [StringLength(500, ErrorMessage = "Observações deve ter no máximo 500 caracteres")]
    public string? Observacoes { get; set; }

    // Propriedades calculadas
    public decimal MargemCalculada
    {
        get
        {
            if (PrecoCusto == 0) return 0;
            return ((PrecoVenda - PrecoCusto) / PrecoCusto) * 100;
        }
    }

    public bool EstoqueBaixo => ControlaEstoque && EstoqueAtual <= EstoqueMinimo;

    public string CodigoFormatado => $"{Codigo} - {Descricao}";

    public string EstoqueFormatado => ControlaEstoque ? 
        $"{EstoqueAtual:N2} {Unidade}" : 
        "Não controlado";
}

/// <summary>
/// Entidade que representa uma categoria de produtos
/// </summary>
public class Categoria : BaseEntity
{
    // Multi-Tenant
    public int EmpresaId { get; set; }
    public virtual Empresa? Empresa { get; set; }

    [Required(ErrorMessage = "Nome da categoria é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome da categoria deve ter no máximo 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres")]
    public string? Descricao { get; set; }

    // Relacionamentos
    public virtual ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}