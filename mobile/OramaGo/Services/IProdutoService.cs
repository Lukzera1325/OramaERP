using OramaGo.Models;

namespace OramaGo.Services;

public interface IProdutoService
{
    Task<IEnumerable<ProdutoLocal>> GetAllAsync();
    Task<ProdutoLocal?> GetByIdAsync(int id);
    Task<IEnumerable<ProdutoLocal>> SearchAsync(string termo);
    Task<ProdutoLocal> CreateAsync(ProdutoLocal produto);
    Task<ProdutoLocal> UpdateAsync(ProdutoLocal produto);
    Task<bool> DeleteAsync(int id);
    Task<int> CountAsync();
    Task<IEnumerable<ProdutoLocal>> GetPendingSyncAsync();
    Task<bool> ExistsByCodigoAsync(string codigo, int? excludeId = null);
    Task<IEnumerable<ProdutoLocal>> GetByCategoriaAsync(string categoria);
    Task<IEnumerable<ProdutoLocal>> GetComEstoqueBaixoAsync();
    Task<IEnumerable<ProdutoLocal>> GetAtivosParaVendaAsync();
    Task<IEnumerable<string>> GetCategoriasAsync();
    Task<ProdutoLocal?> GetByCodigoBarrasAsync(string codigoBarras);
}