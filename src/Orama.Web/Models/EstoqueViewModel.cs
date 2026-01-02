using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orama.Domain.Entities;

namespace Orama.Web.Models;

/// <summary>
/// ViewModel para ajuste de estoque
/// </summary>
public class EstoqueViewModel
{
    [Display(Name = "Produto")]
    [Required(ErrorMessage = "Produto é obrigatório")]
    public int ProdutoId { get; set; }

    [Display(Name = "Novo Estoque")]
    [Required(ErrorMessage = "Novo estoque é obrigatório")]
    [Range(0, double.MaxValue, ErrorMessage = "Estoque deve ser maior ou igual a zero")]
    public decimal NovoEstoque { get; set; }

    [Display(Name = "Motivo")]
    [Required(ErrorMessage = "Motivo é obrigatório")]
    [StringLength(200, ErrorMessage = "Motivo deve ter no máximo 200 caracteres")]
    public string Motivo { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel para movimentações de estoque
/// </summary>
public class MovimentacaoEstoqueViewModel
{
    public int Id { get; set; }

    [Display(Name = "Produto")]
    public int ProdutoId { get; set; }
    
    [Display(Name = "Produto")]
    public string? ProdutoNome { get; set; }
    
    [Display(Name = "Código")]
    public string? ProdutoCodigo { get; set; }

    [Display(Name = "Tipo de Movimentação")]
    public TipoMovimentacaoEstoque Tipo { get; set; }

    [Display(Name = "Tipo")]
    public string TipoDescricao => Tipo switch
    {
        TipoMovimentacaoEstoque.EntradaCompra => "Entrada - Compra",
        TipoMovimentacaoEstoque.SaidaVenda => "Saída - Venda",
        TipoMovimentacaoEstoque.AjustePositivo => "Ajuste Positivo",
        TipoMovimentacaoEstoque.AjusteNegativo => "Ajuste Negativo",
        TipoMovimentacaoEstoque.Transferencia => "Transferência",
        TipoMovimentacaoEstoque.Devolucao => "Devolução",
        TipoMovimentacaoEstoque.Perda => "Perda",
        _ => "Desconhecido"
    };

    [Display(Name = "Data")]
    public DateTime DataMovimentacao { get; set; }

    [Display(Name = "Quantidade")]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
    public decimal Quantidade { get; set; }

    [Display(Name = "Estoque Anterior")]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
    public decimal EstoqueAnterior { get; set; }

    [Display(Name = "Estoque Posterior")]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
    public decimal EstoquePosterior { get; set; }

    [Display(Name = "Custo Unitário")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal? CustoUnitario { get; set; }

    [Display(Name = "Motivo")]
    public string? Motivo { get; set; }

    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }

    [Display(Name = "Usuário")]
    public string? UsuarioNome { get; set; }

    // Propriedades auxiliares
    public string TipoBadgeClass => Tipo switch
    {
        TipoMovimentacaoEstoque.EntradaCompra => "bg-success",
        TipoMovimentacaoEstoque.SaidaVenda => "bg-primary",
        TipoMovimentacaoEstoque.AjustePositivo => "bg-info",
        TipoMovimentacaoEstoque.AjusteNegativo => "bg-warning",
        TipoMovimentacaoEstoque.Transferencia => "bg-secondary",
        TipoMovimentacaoEstoque.Devolucao => "bg-success",
        TipoMovimentacaoEstoque.Perda => "bg-danger",
        _ => "bg-dark"
    };

    public bool IsEntrada => Tipo == TipoMovimentacaoEstoque.EntradaCompra ||
                            Tipo == TipoMovimentacaoEstoque.AjustePositivo ||
                            Tipo == TipoMovimentacaoEstoque.Devolucao;
}

/// <summary>
/// ViewModel para posição de estoque
/// </summary>
public class PosicaoEstoqueViewModel
{
    public int Id { get; set; }
    
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;
    
    [Display(Name = "Produto")]
    public string Nome { get; set; } = string.Empty;
    
    [Display(Name = "Categoria")]
    public string? Categoria { get; set; }
    
    [Display(Name = "Estoque Atual")]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
    public decimal EstoqueAtual { get; set; }
    
    [Display(Name = "Estoque Mínimo")]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
    public decimal EstoqueMinimo { get; set; }
    
    [Display(Name = "Estoque Máximo")]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
    public decimal EstoqueMaximo { get; set; }
    
    [Display(Name = "Preço Custo")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal PrecoCusto { get; set; }
    
    [Display(Name = "Valor Total")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal ValorTotal => EstoqueAtual * PrecoCusto;
    
    [Display(Name = "Diferença Mínimo")]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
    public decimal DiferencaMinimo => EstoqueAtual - EstoqueMinimo;
    
    [Display(Name = "Status")]
    public string Status => EstoqueAtual <= 0 ? "Sem Estoque" : 
                           EstoqueAtual <= EstoqueMinimo ? "Estoque Baixo" : "Normal";
    
    public string StatusBadgeClass => EstoqueAtual <= 0 ? "bg-danger" : 
                                     EstoqueAtual <= EstoqueMinimo ? "bg-warning" : "bg-success";
}

/// <summary>
/// ViewModel para ajuste de estoque
/// </summary>
public class AjusteEstoqueViewModel
{
    [Required(ErrorMessage = "Produto é obrigatório")]
    [Display(Name = "Produto")]
    public int ProdutoId { get; set; }
    
    [Display(Name = "Produto")]
    public string? ProdutoNome { get; set; }
    
    [Display(Name = "Estoque Atual")]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
    public decimal EstoqueAtual { get; set; }
    
    [Required(ErrorMessage = "Quantidade real é obrigatória")]
    [Display(Name = "Quantidade Real (Contagem)")]
    [Range(0, double.MaxValue, ErrorMessage = "Quantidade deve ser maior ou igual a zero")]
    public decimal QuantidadeReal { get; set; }
    
    [Display(Name = "Diferença")]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
    public decimal Diferenca => QuantidadeReal - EstoqueAtual;
    
    [Required(ErrorMessage = "Motivo é obrigatório")]
    [Display(Name = "Motivo")]
    [StringLength(200, ErrorMessage = "Motivo deve ter no máximo 200 caracteres")]
    public string Motivo { get; set; } = string.Empty;
    
    [Display(Name = "Observações")]
    [StringLength(500, ErrorMessage = "Observações devem ter no máximo 500 caracteres")]
    public string? Observacoes { get; set; }
    
    // Propriedades para os selects
    public SelectList? Produtos { get; set; }
    
    // Propriedades auxiliares
    public string TipoAjuste => Diferenca > 0 ? "Ajuste Positivo" : 
                               Diferenca < 0 ? "Ajuste Negativo" : "Sem Diferença";
    
    public string TipoAjusteBadgeClass => Diferenca > 0 ? "bg-success" : 
                                         Diferenca < 0 ? "bg-warning" : "bg-secondary";
}

/// <summary>
/// ViewModel para inventário físico
/// </summary>
public class InventarioViewModel
{
    [Display(Name = "Data do Inventário")]
    public DateTime DataInventario { get; set; } = DateTime.Now;
    
    [Display(Name = "Observações Gerais")]
    [StringLength(500, ErrorMessage = "Observações devem ter no máximo 500 caracteres")]
    public string? ObservacoesGerais { get; set; }
    
    public List<InventarioItemViewModel> Itens { get; set; } = new();
}

/// <summary>
/// ViewModel para item do inventário
/// </summary>
public class InventarioItemViewModel
{
    public int ProdutoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    
    [Display(Name = "Estoque Sistema")]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
    public decimal EstoqueSistema { get; set; }
    
    [Display(Name = "Contagem Física")]
    [Range(0, double.MaxValue, ErrorMessage = "Quantidade deve ser maior ou igual a zero")]
    public decimal ContagemFisica { get; set; }
    
    [Display(Name = "Diferença")]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
    public decimal Diferenca => ContagemFisica - EstoqueSistema;
    
    public bool TemDiferenca => Diferenca != 0;
    
    public string DiferencaBadgeClass => Diferenca > 0 ? "bg-success" : 
                                        Diferenca < 0 ? "bg-warning" : "bg-secondary";
}

/// <summary>
/// ViewModel para filtros de movimentação
/// </summary>
public class MovimentacaoFiltroViewModel
{
    [Display(Name = "Produto")]
    public int? ProdutoId { get; set; }
    
    [Display(Name = "Tipo")]
    public TipoMovimentacaoEstoque? Tipo { get; set; }
    
    [Display(Name = "Data Inicial")]
    [DataType(DataType.Date)]
    public DateTime? DataInicial { get; set; }
    
    [Display(Name = "Data Final")]
    [DataType(DataType.Date)]
    public DateTime? DataFinal { get; set; }
    
    // Propriedades para os selects
    public SelectList? Produtos { get; set; }
    public SelectList? Tipos { get; set; }
}

/// <summary>
/// ViewModel para relatórios de estoque
/// </summary>
public class RelatorioEstoqueViewModel
{
    public DateTime DataRelatorio { get; set; }
    public int TotalProdutos { get; set; }
    public decimal ValorTotalEstoque { get; set; }
    public int ProdutosEstoqueBaixo { get; set; }
    public int ProdutosSemEstoque { get; set; }
    
    public List<PosicaoEstoqueViewModel> Produtos { get; set; } = new();
}