namespace Orama.Application.Services;

/// <summary>
/// Interface para Relatórios de Lucratividade
/// Consultas simples baseadas em dados já persistidos
/// </summary>
public interface IRelatorioLucratividadeService
{
    // Relatório 1: Produtos Mais Lucrativos
    Task<IEnumerable<ProdutoLucratividade>> ObterProdutosMaisLucrativosAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        int empresaId, 
        int limite = 20);

    // Relatório 2: Vendas com Margem Negativa
    Task<IEnumerable<VendaMargemNegativa>> ObterVendasMargemNegativaAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        int empresaId);

    // Relatório 3: Evolução de Margem no Tempo
    Task<IEnumerable<EvolucaoMargem>> ObterEvolucaoMargemMensalAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        int empresaId);

    Task<IEnumerable<EvolucaoMargem>> ObterEvolucaoMargemDiariaAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        int empresaId);

    // Resumo Executivo
    Task<ResumoLucratividade> ObterResumoLucratividadeAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        int empresaId);
}