using Orama.Domain.Entities;

namespace Orama.Application.Services;

/// <summary>
/// Interface para serviços de controle de estoque
/// </summary>
public interface IEstoqueService
{
    /// <summary>
    /// Obtém todas as movimentações de estoque por empresa
    /// </summary>
    Task<IEnumerable<MovimentacaoEstoque>> ObterMovimentacoesAsync(int empresaId);
    
    /// <summary>
    /// Obtém movimentações de estoque por produto
    /// </summary>
    Task<IEnumerable<MovimentacaoEstoque>> ObterMovimentacoesPorProdutoAsync(int empresaId, int produtoId);
    
    /// <summary>
    /// Obtém movimentações de estoque por período
    /// </summary>
    Task<IEnumerable<MovimentacaoEstoque>> ObterMovimentacoesPorPeriodoAsync(int empresaId, DateTime inicio, DateTime fim);
    
    /// <summary>
    /// Obtém movimentações de estoque por tipo
    /// </summary>
    Task<IEnumerable<MovimentacaoEstoque>> ObterMovimentacoesPorTipoAsync(int empresaId, TipoMovimentacaoEstoque tipo);
    
    /// <summary>
    /// Obtém posição atual do estoque por empresa
    /// </summary>
    Task<IEnumerable<Produto>> ObterPosicaoEstoqueAsync(int empresaId);
    
    /// <summary>
    /// Obtém produtos com estoque baixo
    /// </summary>
    Task<IEnumerable<Produto>> ObterProdutosEstoqueBaixoAsync(int empresaId);
    
    /// <summary>
    /// Obtém produtos sem movimentação
    /// </summary>
    Task<IEnumerable<Produto>> ObterProdutosSemMovimentacaoAsync(int empresaId, int dias = 30);
    
    /// <summary>
    /// Registra uma movimentação de estoque
    /// </summary>
    Task<MovimentacaoEstoque> RegistrarMovimentacaoAsync(MovimentacaoEstoque movimentacao);
    
    /// <summary>
    /// Registra entrada de estoque (compra, ajuste positivo, etc.)
    /// </summary>
    Task<MovimentacaoEstoque> RegistrarEntradaAsync(int empresaId, int produtoId, decimal quantidade, 
        TipoMovimentacaoEstoque tipo, string motivo, decimal? custoUnitario = null, 
        int? referenciaId = null, int? usuarioId = null);
    
    /// <summary>
    /// Registra saída de estoque (venda, ajuste negativo, etc.)
    /// </summary>
    Task<MovimentacaoEstoque> RegistrarSaidaAsync(int empresaId, int produtoId, decimal quantidade, 
        TipoMovimentacaoEstoque tipo, string motivo, int? referenciaId = null, int? usuarioId = null);
    
    /// <summary>
    /// Realiza ajuste de estoque (inventário)
    /// </summary>
    Task<MovimentacaoEstoque?> AjustarEstoqueAsync(int empresaId, int produtoId, decimal quantidadeReal, 
        string motivo, int usuarioId);
    
    /// <summary>
    /// Realiza transferência entre produtos (se aplicável)
    /// </summary>
    Task<IEnumerable<MovimentacaoEstoque>> TransferirEstoqueAsync(int empresaId, int produtoOrigemId, 
        int produtoDestinoId, decimal quantidade, string motivo, int usuarioId);
    
    /// <summary>
    /// Obtém histórico de um produto específico
    /// </summary>
    Task<IEnumerable<MovimentacaoEstoque>> ObterHistoricoProdutoAsync(int empresaId, int produtoId, 
        DateTime? inicio = null, DateTime? fim = null);
    
    /// <summary>
    /// Calcula valor total do estoque
    /// </summary>
    Task<decimal> CalcularValorTotalEstoqueAsync(int empresaId);
    
    /// <summary>
    /// Obtém relatório de movimentações por período
    /// </summary>
    Task<object> ObterRelatorioMovimentacoesAsync(int empresaId, DateTime inicio, DateTime fim);
    
    /// <summary>
    /// Obtém relatório de posição de estoque
    /// </summary>
    Task<object> ObterRelatorioPosicaoEstoqueAsync(int empresaId);
    
    /// <summary>
    /// Valida se há estoque suficiente para uma operação
    /// </summary>
    Task<bool> ValidarEstoqueDisponivelAsync(int empresaId, int produtoId, decimal quantidade);
    
    /// <summary>
    /// Obtém produtos para inventário (contagem física)
    /// </summary>
    Task<IEnumerable<Produto>> ObterProdutosParaInventarioAsync(int empresaId);
    
    /// <summary>
    /// Processa inventário físico
    /// </summary>
    Task<IEnumerable<MovimentacaoEstoque>> ProcessarInventarioAsync(int empresaId, 
        Dictionary<int, decimal> contagemFisica, int usuarioId, string? observacoes = null);
}