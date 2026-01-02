using System.ComponentModel.DataAnnotations;

namespace OramaGo.Models;

public class ProdutoLocal : BaseLocalModel
{
    [Required]
    [StringLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Descricao { get; set; } = string.Empty;

    [StringLength(500)]
    public string? DescricaoDetalhada { get; set; }

    [StringLength(100)]
    public string? Categoria { get; set; }

    [StringLength(100)]
    public string? Marca { get; set; }

    [StringLength(10)]
    public string Unidade { get; set; } = "UN";

    /// <summary>
    /// Preço de custo do produto
    /// </summary>
    public decimal PrecoCusto { get; set; }

    /// <summary>
    /// Preço de venda sugerido
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
    /// Estoque atual do produto
    /// </summary>
    public decimal EstoqueAtual { get; set; }

    /// <summary>
    /// Estoque mínimo para alerta
    /// </summary>
    public decimal EstoqueMinimo { get; set; }

    /// <summary>
    /// Peso do produto em kg
    /// </summary>
    public decimal? Peso { get; set; }

    /// <summary>
    /// Dimensões do produto (LxAxP em cm)
    /// </summary>
    [StringLength(50)]
    public string? Dimensoes { get; set; }

    /// <summary>
    /// Código de barras/EAN
    /// </summary>
    [StringLength(50)]
    public string? CodigoBarras { get; set; }

    /// <summary>
    /// URL da imagem do produto
    /// </summary>
    [StringLength(500)]
    public string? ImagemUrl { get; set; }

    /// <summary>
    /// Indica se o produto controla estoque
    /// </summary>
    public bool ControlaEstoque { get; set; } = true;

    /// <summary>
    /// Indica se o produto está ativo para vendas
    /// </summary>
    public bool AtivoVenda { get; set; } = true;

    /// <summary>
    /// Observações sobre o produto
    /// </summary>
    [StringLength(500)]
    public string? Observacoes { get; set; }

    // Propriedades calculadas
    public string CodigoFormatado => $"{Codigo} - {Descricao}";
    public bool EstoqueBaixo => ControlaEstoque && EstoqueAtual <= EstoqueMinimo;
    public bool EstoqueDisponivel => !ControlaEstoque || EstoqueAtual > 0;
    public string StatusEstoque => EstoqueBaixo ? "Baixo" : EstoqueDisponivel ? "Disponível" : "Indisponível";
    public string EstoqueStatus => EstoqueAtual <= 0 ? "SEM ESTOQUE" : EstoqueAtual <= EstoqueMinimo ? "BAIXO" : "DISPONÍVEL";
    public string PrecoVendaFormatado => PrecoVenda.ToString("C2");
    public string EstoqueFormatado => $"{EstoqueAtual:N2} {Unidade}";
    public string Nome => Descricao; // Alias para compatibilidade com a UI
}