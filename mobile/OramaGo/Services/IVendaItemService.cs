using OramaGo.Models;

namespace OramaGo.Services;

public interface IVendaItemService : IBaseService<VendaItemLocal>
{
    Task<IEnumerable<VendaItemLocal>> GetByVendaIdAsync(int vendaId);
    Task<VendaItemLocal> AdicionarItemAsync(int vendaId, int produtoId, decimal quantidade, decimal precoUnitario);
    Task<VendaItemLocal> AtualizarQuantidadeAsync(int itemId, decimal quantidade);
    Task<VendaItemLocal> AtualizarPrecoAsync(int itemId, decimal precoUnitario);
    Task<VendaItemLocal> AtualizarDescontoAsync(int itemId, decimal percentualDesconto);
    Task RemoverItemAsync(int itemId);
    Task<bool> ValidarEstoqueItemAsync(int itemId);
    Task RecalcularItemAsync(int itemId);
}