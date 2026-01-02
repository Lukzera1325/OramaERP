using Orama.Domain.Entities;

namespace Orama.Application.Services;

/// <summary>
/// Interface simplificada para o VendaService
/// Focada em operações essenciais e claras
/// </summary>
public interface IVendaService
{
    // CRUD Básico
    Task<IEnumerable<Venda>> ObterTodosAsync(int empresaId);
    Task<Venda?> ObterPorIdAsync(int id, int empresaId);
    Task<IEnumerable<Venda>> ObterPorStatusAsync(int empresaId, StatusVenda status);
    Task<IEnumerable<Venda>> ObterPorClienteAsync(int empresaId, int clienteId);
    Task<IEnumerable<Venda>> ObterPorPeriodoAsync(int empresaId, DateTime inicio, DateTime fim);
    
    // Operações Simples
    Task<string> GerarNumeroAsync(int empresaId);
    Task<Venda> CriarAsync(Venda venda);
    Task<Venda> AtualizarAsync(Venda venda);
    Task ExcluirAsync(int id, int empresaId);
    
    // Operações de Negócio
    Task<Venda> AprovarAsync(int id, int empresaId);
    Task<Venda> FaturarAsync(int id, int empresaId);
    Task<Venda> CancelarAsync(int id, int empresaId, string motivo = "");
    
    // Consultas e Relatórios
    Task<decimal> ObterTotalVendasAsync(int empresaId, DateTime? inicio = null, DateTime? fim = null);
    Task<IEnumerable<dynamic>> ObterProdutosMaisVendidosAsync(int empresaId, DateTime inicio, DateTime fim, int limite = 10);
    
    // Relatórios de Lucratividade
    Task<IEnumerable<Venda>> ObterRelatorioLucratividadeAsync(DateTime dataInicio, DateTime dataFim, int empresaId);
    Task<IEnumerable<Venda>> ObterVendasMaisLucrativasAsync(int empresaId, int quantidade = 10);
    Task<IEnumerable<Venda>> ObterVendasComPrejuizoAsync(int empresaId);
    Task<dynamic> ObterResumoLucratividadeAsync(DateTime dataInicio, DateTime dataFim, int empresaId);
}
