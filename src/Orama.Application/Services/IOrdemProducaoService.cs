using Orama.Domain.Entities;

namespace Orama.Application.Services;

/// <summary>
/// Interface para gerenciar ordens de produção
/// </summary>
public interface IOrdemProducaoService
{
    // Consultas
    Task<IEnumerable<OrdemProducao>> ObterTodosAsync(int empresaId);
    Task<OrdemProducao?> ObterPorIdAsync(int id, int empresaId);
    Task<IEnumerable<OrdemProducao>> ObterPorStatusAsync(StatusOrdemProducao status, int empresaId);

    // Operações
    Task<OrdemProducao> CriarAsync(int produtoId, decimal quantidade, string? observacoes, int empresaId, int usuarioId);
    Task<bool> LiberarAsync(int id, int empresaId, int usuarioId);
    Task<bool> IniciarAsync(int id, int empresaId, int usuarioId);
    Task<bool> FinalizarAsync(int id, decimal quantidadeProduzida, int empresaId, int usuarioId);
    Task<bool> CancelarAsync(int id, string motivo, int empresaId, int usuarioId);

    // Relatórios
    Task<decimal> CalcularCustoProducaoAsync(int ordemProducaoId, int empresaId);
    Task<IEnumerable<OrdemProducao>> ObterOrdensAtrasadasAsync(int empresaId);
    Task<IEnumerable<OrdemProducao>> ObterRelatorioCustomPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, int empresaId);
    Task<IEnumerable<OrdemProducao>> ObterOrdensMaiorCustoAsync(int empresaId, int quantidade = 10);
}