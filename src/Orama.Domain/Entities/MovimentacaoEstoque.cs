using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities;

/// <summary>
/// Movimentação de estoque
/// </summary>
public class MovimentacaoEstoque : BaseEntity
{
    public int EmpresaId { get; set; }
    public virtual Empresa? Empresa { get; set; }

    public int ProdutoId { get; set; }
    public virtual Produto Produto { get; set; } = null!;

    public TipoMovimentacaoEstoque Tipo { get; set; }

    public DateTime DataMovimentacao { get; set; } = DateTime.Now;

    public decimal Quantidade { get; set; }
    public decimal EstoqueAnterior { get; set; }
    public decimal EstoquePosterior { get; set; }

    public decimal? CustoUnitario { get; set; }

    // Referências opcionais
    public int? VendaId { get; set; }
    public int? CompraId { get; set; }

    [StringLength(200)]
    public string? Motivo { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }

    public int? UsuarioId { get; set; }
    public virtual Usuario? Usuario { get; set; }
}

public enum TipoMovimentacaoEstoque
{
    EntradaCompra = 1,
    SaidaVenda = 2,
    AjustePositivo = 3,
    AjusteNegativo = 4,
    Transferencia = 5,
    Devolucao = 6,
    Perda = 7
}
