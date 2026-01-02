using System.ComponentModel.DataAnnotations;

namespace Orama.Web.Models.Api;

/// <summary>
/// DTO da venda para API
/// </summary>
public class VendaApiDto
{
    /// <summary>
    /// ID da venda
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Número da venda
    /// </summary>
    public string Numero { get; set; } = string.Empty;

    /// <summary>
    /// ID do cliente
    /// </summary>
    public int ClienteId { get; set; }

    /// <summary>
    /// Nome do cliente
    /// </summary>
    public string ClienteNome { get; set; } = string.Empty;

    /// <summary>
    /// ID do vendedor
    /// </summary>
    public int? VendedorId { get; set; }

    /// <summary>
    /// Data da venda
    /// </summary>
    public DateTime DataVenda { get; set; }

    /// <summary>
    /// Data de entrega
    /// </summary>
    public DateTime? DataEntrega { get; set; }

    /// <summary>
    /// Status da venda
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Subtotal da venda
    /// </summary>
    public decimal SubTotal { get; set; }

    /// <summary>
    /// Valor do desconto
    /// </summary>
    public decimal ValorDesconto { get; set; }

    /// <summary>
    /// Percentual de desconto
    /// </summary>
    public decimal PercentualDesconto { get; set; }

    /// <summary>
    /// Valor do frete
    /// </summary>
    public decimal ValorFrete { get; set; }

    /// <summary>
    /// Valor total da venda
    /// </summary>
    public decimal ValorTotal { get; set; }

    /// <summary>
    /// Forma de pagamento
    /// </summary>
    public string FormaPagamento { get; set; } = string.Empty;

    /// <summary>
    /// Número de parcelas
    /// </summary>
    public int Parcelas { get; set; }

    /// <summary>
    /// Observações da venda
    /// </summary>
    public string? Observacoes { get; set; }

    /// <summary>
    /// Data de criação da venda
    /// </summary>
    public DateTime DataCriacao { get; set; }

    /// <summary>
    /// Data da última modificação
    /// </summary>
    public DateTime DataModificacao { get; set; }

    /// <summary>
    /// Itens da venda
    /// </summary>
    public List<VendaItemApiDto> Itens { get; set; } = new();
}

/// <summary>
/// DTO do item de venda para API
/// </summary>
public class VendaItemApiDto
{
    /// <summary>
    /// ID do item
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID do produto
    /// </summary>
    public int ProdutoId { get; set; }

    /// <summary>
    /// Nome do produto
    /// </summary>
    public string ProdutoNome { get; set; } = string.Empty;

    /// <summary>
    /// Código do produto
    /// </summary>
    public string ProdutoCodigo { get; set; } = string.Empty;

    /// <summary>
    /// Quantidade do item
    /// </summary>
    public decimal Quantidade { get; set; }

    /// <summary>
    /// Preço unitário
    /// </summary>
    public decimal PrecoUnitario { get; set; }

    /// <summary>
    /// Percentual de desconto
    /// </summary>
    public decimal PercentualDesconto { get; set; }

    /// <summary>
    /// Valor do desconto
    /// </summary>
    public decimal ValorDesconto { get; set; }

    /// <summary>
    /// Valor total do item
    /// </summary>
    public decimal ValorTotal { get; set; }

    /// <summary>
    /// Observações do item
    /// </summary>
    public string? Observacoes { get; set; }
}

/// <summary>
/// DTO para criação de venda via API
/// </summary>
public class VendaCreateApiDto
{
    /// <summary>
    /// ID do cliente
    /// </summary>
    [Required(ErrorMessage = "Cliente é obrigatório")]
    public int ClienteId { get; set; }

    /// <summary>
    /// Data da venda
    /// </summary>
    [Required(ErrorMessage = "Data da venda é obrigatória")]
    public DateTime DataVenda { get; set; }

    /// <summary>
    /// Data de entrega
    /// </summary>
    public DateTime? DataEntrega { get; set; }

    /// <summary>
    /// Status da venda
    /// </summary>
    [Required(ErrorMessage = "Status é obrigatório")]
    public string Status { get; set; } = "Orcamento";

    /// <summary>
    /// Valor do desconto
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Valor do desconto deve ser maior ou igual a zero")]
    public decimal ValorDesconto { get; set; }

    /// <summary>
    /// Percentual de desconto
    /// </summary>
    [Range(0, 100, ErrorMessage = "Percentual de desconto deve estar entre 0 e 100")]
    public decimal PercentualDesconto { get; set; }

    /// <summary>
    /// Valor do frete
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Valor do frete deve ser maior ou igual a zero")]
    public decimal ValorFrete { get; set; }

    /// <summary>
    /// Forma de pagamento
    /// </summary>
    [Required(ErrorMessage = "Forma de pagamento é obrigatória")]
    public string FormaPagamento { get; set; } = "AVista";

    /// <summary>
    /// Número de parcelas
    /// </summary>
    [Range(1, 48, ErrorMessage = "Número de parcelas deve estar entre 1 e 48")]
    public int Parcelas { get; set; } = 1;

    /// <summary>
    /// Observações da venda
    /// </summary>
    [StringLength(500, ErrorMessage = "Observações devem ter no máximo 500 caracteres")]
    public string? Observacoes { get; set; }

    /// <summary>
    /// Itens da venda
    /// </summary>
    public List<VendaItemCreateApiDto> Itens { get; set; } = new();
}

/// <summary>
/// DTO para criação de item de venda via API
/// </summary>
public class VendaItemCreateApiDto
{
    /// <summary>
    /// ID do produto
    /// </summary>
    [Required(ErrorMessage = "Produto é obrigatório")]
    public int ProdutoId { get; set; }

    /// <summary>
    /// Quantidade do item
    /// </summary>
    [Required(ErrorMessage = "Quantidade é obrigatória")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
    public decimal Quantidade { get; set; }

    /// <summary>
    /// Preço unitário
    /// </summary>
    [Required(ErrorMessage = "Preço unitário é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Preço unitário deve ser maior que zero")]
    public decimal PrecoUnitario { get; set; }

    /// <summary>
    /// Percentual de desconto
    /// </summary>
    [Range(0, 100, ErrorMessage = "Percentual de desconto deve estar entre 0 e 100")]
    public decimal PercentualDesconto { get; set; }

    /// <summary>
    /// Observações do item
    /// </summary>
    [StringLength(200, ErrorMessage = "Observações devem ter no máximo 200 caracteres")]
    public string? Observacoes { get; set; }
}