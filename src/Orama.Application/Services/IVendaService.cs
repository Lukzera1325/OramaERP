using Orama.Domain.Entities;

namespace Orama.Application.Services;

public interface IVendaService
{
    Task<IEnumerable<Venda>> ObterTodosAsync(int empresaId);
    Task<Venda?> ObterPorIdAsync(int id, int empresaId);
    Task<IEnumerable<Venda>> ObterPorStatusAsync(int empresaId, StatusVenda status);
    Task<IEnumerable<Venda>> ObterPorClienteAsync(int empresaId, int clienteId);
    Task<IEnumerable<Venda>> ObterPorPeriodoAsync(int empresaId, DateTime inicio, DateTime fim);
    Task<string> GerarNumeroAsync(int empresaId);
    Task<Venda> IncluirAsync(Venda venda);
    Task<Venda> CriarAsync(Venda venda);
    Task AlterarAsync(Venda venda);
    Task ExcluirAsync(int id, int empresaId);
    Task<Venda> AprovarAsync(int id, int empresaId);
    Task<Venda> FaturarAsync(int id, int empresaId, int? contaBancariaId);
    Task<Venda> CancelarAsync(int id, int empresaId);
    Task<decimal> ObterTotalVendasAsync(int empresaId, DateTime? inicio = null, DateTime? fim = null);
    Task<IEnumerable<dynamic>> ObterProdutosMaisVendidosAsync(int empresaId, DateTime inicio, DateTime fim, int limite = 10);
    
    // Novos métodos para integração completa
    Task<Venda> FaturarComParcelasAsync(int id, int empresaId, FormaPagamento formaPagamento, int numeroParcelas);
    Task<IEnumerable<VendaItem>> ObterItensVendaAsync(int vendaId, int empresaId);
    Task<VendaItem> AdicionarItemAsync(int vendaId, VendaItem item, int empresaId);
    Task<VendaItem> AtualizarItemAsync(VendaItem item, int empresaId);
    Task RemoverItemAsync(int itemId, int empresaId);
    Task<Venda> RecalcularTotaisAsync(int vendaId, int empresaId);
    Task<bool> ValidarEstoqueAsync(int vendaId, int empresaId);
}
