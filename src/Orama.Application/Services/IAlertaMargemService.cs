using Orama.Domain.Entities;

namespace Orama.Application.Services;

/// <summary>
/// Interface para Service de Alertas de Margem
/// </summary>
public interface IAlertaMargemService
{
    // Criação Automática de Alertas
    Task ProcessarAlertasVendaAsync(int vendaId, int empresaId);

    // Consultas de Alertas
    Task<IEnumerable<AlertaMargem>> ObterAlertasAtivosAsync(int empresaId);
    Task<IEnumerable<AlertaMargem>> ObterAlertasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, int empresaId, StatusAlerta? status = null);
    Task<IEnumerable<AlertaMargem>> ObterAlertasPorTipoAsync(TipoAlertaMargem tipo, int empresaId, StatusAlerta? status = null);
    Task<AlertaMargem?> ObterAlertaPorIdAsync(int id, int empresaId);
    Task<int> ContarAlertasAtivosAsync(int empresaId);

    // Resolução de Alertas
    Task<bool> ResolverAlertaAsync(int alertaId, int usuarioId, int empresaId, string? observacoes = null);
    Task<bool> ReativarAlertaAsync(int alertaId, int empresaId);
    Task<int> ResolverAlertasEmLoteAsync(IEnumerable<int> alertaIds, int usuarioId, int empresaId, string? observacoes = null);

    // Estatísticas
    Task<dynamic> ObterEstatisticasAlertasAsync(DateTime dataInicio, DateTime dataFim, int empresaId);
}