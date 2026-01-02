using Orama.Domain.Entities;

namespace Orama.Application.Services;

/// <summary>
/// Interface simplificada para controle de estoque
/// Cada método = um caso de uso específico
/// </summary>
public interface IEstoqueService
{
    // Consultas
    Task<IEnumerable<Produto>> ObterPosicaoEstoqueAsync(int empresaId);
    Task<IEnumerable<Produto>> ObterProdutosEstoqueBaixoAsync(int empresaId);
    Task<IEnumerable<MovimentacaoEstoque>> ObterHistoricoAsync(int empresaId, int? produtoId = null);
    Task<decimal> CalcularValorTotalEstoqueAsync(int empresaId);

    // Movimentações
    Task<bool> AjustarEstoqueAsync(int produtoId, decimal novoEstoque, string motivo, int empresaId, int usuarioId);
    Task<bool> EntradaEstoqueAsync(int produtoId, decimal quantidade, string motivo, int empresaId, int usuarioId);
    Task<bool> SaidaEstoqueAsync(int produtoId, decimal quantidade, string motivo, int empresaId, int usuarioId);

    // Inventário
    Task<IEnumerable<Produto>> ObterProdutosParaInventarioAsync(int empresaId);
    Task<bool> ProcessarInventarioAsync(Dictionary<int, decimal> contagemInventario, int empresaId, int usuarioId);
}