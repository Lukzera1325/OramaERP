using System.ComponentModel.DataAnnotations;

namespace Orama.Web.Models.Api;

/// <summary>
/// DTO do produto para API
/// </summary>
public class ProdutoApiDto
{
    /// <summary>
    /// ID do produto
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Código do produto
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Descrição do produto
    /// </summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada do produto
    /// </summary>
    public string? DescricaoDetalhada { get; set; }

    /// <summary>
    /// Categoria do produto
    /// </summary>
    public string? Categoria { get; set; }

    /// <summary>
    /// Unidade de medida
    /// </summary>
    public string Unidade { get; set; } = "UN";

    /// <summary>
    /// Preço de custo
    /// </summary>
    public decimal PrecoCusto { get; set; }

    /// <summary>
    /// Preço de venda
    /// </summary>
    public decimal PrecoVenda { get; set; }

    /// <summary>
    /// Preço mínimo para venda
    /// </summary>
    public decimal PrecoMinimo { get; set; }

    /// <summary>
    /// Margem de lucro em percentual
    /// </summary>
    public decimal MargemLucro { get; set; }

    /// <summary>
    /// Estoque atual
    /// </summary>
    public decimal EstoqueAtual { get; set; }

    /// <summary>
    /// Estoque mínimo
    /// </summary>
    public decimal EstoqueMinimo { get; set; }

    /// <summary>
    /// Peso do produto em kg
    /// </summary>
    public decimal? Peso { get; set; }

    /// <summary>
    /// Código de barras
    /// </summary>
    public string? CodigoBarras { get; set; }

    /// <summary>
    /// Observações sobre o produto
    /// </summary>
    public string? Observacoes { get; set; }

    /// <summary>
    /// Indica se o produto está ativo
    /// </summary>
    public bool Ativo { get; set; }

    /// <summary>
    /// Data de criação do produto
    /// </summary>
    public DateTime DataCriacao { get; set; }

    /// <summary>
    /// Data da última modificação
    /// </summary>
    public DateTime DataModificacao { get; set; }

    /// <summary>
    /// Indica se o produto está disponível para venda (calculado)
    /// </summary>
    public bool Disponivel => Ativo && EstoqueAtual > 0;

    /// <summary>
    /// Indica se o estoque está baixo (calculado)
    /// </summary>
    public bool EstoqueBaixo => EstoqueAtual <= EstoqueMinimo;

    /// <summary>
    /// Status do estoque (calculado)
    /// </summary>
    public string StatusEstoque => EstoqueAtual <= 0 ? "SEM ESTOQUE" : 
                                  EstoqueAtual <= EstoqueMinimo ? "BAIXO" : "DISPONÍVEL";
}