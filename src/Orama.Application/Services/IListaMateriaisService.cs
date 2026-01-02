using Orama.Domain.Entities;

namespace Orama.Application.Services
{
    public interface IListaMateriaisService
    {
        Task<IEnumerable<ListaMateriais>> ObterTodosAsync(int empresaId);
        Task<ListaMateriais?> ObterPorIdAsync(int id, int empresaId);
        Task<ListaMateriais?> ObterPorProdutoAsync(int produtoId, int empresaId);
        Task<ListaMateriais> CriarAsync(ListaMateriais listaMateriais);
        Task<ListaMateriais> AtualizarAsync(ListaMateriais listaMateriais);
        Task<bool> ExcluirAsync(int id, int empresaId);
        
        // Operações com itens
        Task<ListaMateriaisItem> AdicionarItemAsync(ListaMateriaisItem item);
        Task<ListaMateriaisItem> AtualizarItemAsync(ListaMateriaisItem item);
        Task<bool> RemoverItemAsync(int itemId, int empresaId);
        
        // Cálculos
        Task<decimal> CalcularCustoTotalAsync(int id, int empresaId);
        Task<decimal> CalcularCustoProducaoAsync(int produtoId, decimal quantidade, int empresaId);
        
        // Validações
        Task<bool> ValidarDisponibilidadeMateriaisAsync(int produtoId, decimal quantidade, int empresaId);
        Task<IEnumerable<(int ProdutoId, string Descricao, decimal QuantidadeNecessaria, decimal EstoqueAtual)>> 
            ObterMateriaisInsuficientesAsync(int produtoId, decimal quantidade, int empresaId);
        
        // Explosão de materiais
        Task<IEnumerable<(int ProdutoId, string Descricao, decimal QuantidadeTotal, string Unidade)>> 
            ExplodirMateriaisAsync(int produtoId, decimal quantidade, int empresaId);
    }
}