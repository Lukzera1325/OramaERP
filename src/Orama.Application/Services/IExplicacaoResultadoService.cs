using Orama.Domain.Entities;

namespace Orama.Application.Services;

/// <summary>
/// Interface para Service de Explicação de Resultados
/// </summary>
public interface IExplicacaoResultadoService
{
    // Geração de Explicações
    Task GerarExplicacoesVendaAsync(int vendaId, int empresaId);

    // Consultas de Explicações
    Task<IEnumerable<ExplicacaoResultado>> ObterExplicacoesVendaAsync(int vendaId, int empresaId);
    Task<IEnumerable<ExplicacaoResultado>> ObterExplicacoesPorTipoAsync(TipoResultado tipo, int empresaId, DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<IEnumerable<ExplicacaoResultado>> ObterExplicacoesPorCategoriaAsync(CategoriaExplicacao categoria, int empresaId, DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<ExplicacaoResultado?> ObterExplicacaoPorIdAsync(int id, int empresaId);
    Task<dynamic> ObterResumoExplicacoesAsync(DateTime dataInicio, DateTime dataFim, int empresaId);

    // Análises Específicas
    Task<IEnumerable<dynamic>> ObterPrincipaisMotivosPrejuizoAsync(DateTime dataInicio, DateTime dataFim, int empresaId);
    Task<IEnumerable<dynamic>> ObterProdutosMaisPrejuizoAsync(DateTime dataInicio, DateTime dataFim, int empresaId, int limite = 10);
    Task<bool> VendaPrecisaNovaExplicacaoAsync(int vendaId, int empresaId);
}