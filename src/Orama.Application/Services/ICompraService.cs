using Orama.Domain.Entities;

namespace Orama.Application.Services;

/// <summary>
/// Interface para serviços de compra
/// </summary>
public interface ICompraService
{
    /// <summary>
    /// Obtém todas as compras ativas por empresa
    /// </summary>
    Task<IEnumerable<Compra>> ObterTodosAsync(int empresaId);
    
    /// <summary>
    /// Obtém uma compra por ID e empresa
    /// </summary>
    Task<Compra?> ObterPorIdAsync(int id, int empresaId);
    
    /// <summary>
    /// Obtém compras por status
    /// </summary>
    Task<IEnumerable<Compra>> ObterPorStatusAsync(int empresaId, StatusCompra status);
    
    /// <summary>
    /// Obtém compras por fornecedor
    /// </summary>
    Task<IEnumerable<Compra>> ObterPorFornecedorAsync(int empresaId, int fornecedorId);
    
    /// <summary>
    /// Obtém compras por período
    /// </summary>
    Task<IEnumerable<Compra>> ObterPorPeriodoAsync(int empresaId, DateTime inicio, DateTime fim);
    
    /// <summary>
    /// Gera número sequencial para nova compra
    /// </summary>
    Task<string> GerarNumeroAsync(int empresaId);
    
    /// <summary>
    /// Cria uma nova compra
    /// </summary>
    Task<Compra> CriarAsync(Compra compra);
    
    /// <summary>
    /// Atualiza uma compra existente
    /// </summary>
    Task<Compra> AtualizarAsync(Compra compra);
    
    /// <summary>
    /// Exclui uma compra (soft delete)
    /// </summary>
    Task ExcluirAsync(int id, int empresaId);
    
    /// <summary>
    /// Aprova uma compra
    /// </summary>
    Task<Compra> AprovarAsync(int id, int empresaId);
    
    /// <summary>
    /// Recebe uma compra (entrada no estoque)
    /// </summary>
    Task<Compra> ReceberAsync(int id, int empresaId);
    
    /// <summary>
    /// Cancela uma compra
    /// </summary>
    Task<Compra> CancelarAsync(int id, int empresaId);
    
    // Métodos para gerenciamento de itens
    Task<IEnumerable<CompraItem>> ObterItensCompraAsync(int compraId, int empresaId);
    Task<CompraItem> AdicionarItemAsync(int compraId, CompraItem item, int empresaId);
    Task<CompraItem> AtualizarItemAsync(CompraItem item, int empresaId);
    Task RemoverItemAsync(int itemId, int empresaId);
    Task<Compra> RecalcularTotaisAsync(int compraId, int empresaId);
    
    /// <summary>
    /// Obtém total de compras por período
    /// </summary>
    Task<decimal> ObterTotalComprasAsync(int empresaId, DateTime? inicio = null, DateTime? fim = null);
}