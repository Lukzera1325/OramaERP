using Orama.Domain.Entities;

namespace Orama.Application.Services
{
    public interface ISugestaoAcaoService
    {
        // Geração de sugestões
        Task GerarSugestoesAutomaticasAsync();
        Task GerarSugestoesProdutoAsync(int produtoId);
        
        // Consultas
        Task<List<SugestaoAcao>> ObterSugestoesPendentesAsync();
        Task<List<SugestaoAcao>> ObterSugestoesPorPrioridadeAsync(PrioridadeSugestao prioridade);
        Task<SugestaoAcao?> ObterPorIdAsync(int id);
        Task<List<SugestaoAcao>> ObterPorProdutoAsync(int produtoId);
        
        // Gestão de sugestões
        Task<bool> ResolverSugestaoAsync(int sugestaoId, int usuarioId, string observacao);
        Task<bool> DescartarSugestaoAsync(int sugestaoId, int usuarioId, string motivo);
        Task<int> ResolverSugestoesEmLoteAsync(List<int> sugestaoIds, int usuarioId, string observacao);
        
        // Estatísticas
        Task<Dictionary<string, int>> ObterEstatisticasSugestoesAsync();
        Task<List<SugestaoAcao>> ObterSugestoesRecentesAsync(int quantidade = 10);
    }
}