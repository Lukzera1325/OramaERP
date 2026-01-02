using Orama.Domain.Entities;

namespace Orama.Application.Services
{
    public interface IDecisaoGerencialService
    {
        // Criação de decisões
        Task<bool> RegistrarDecisaoProdutoAsync(int produtoId, string problema, string acao, string observacoes = "");
        Task<bool> RegistrarDecisaoVendaAsync(int vendaId, string problema, string acao, string observacoes = "");
        Task<bool> RegistrarDecisaoSugestaoAsync(int sugestaoId, string acao, string observacoes = "");
        
        // Consultas
        Task<List<DecisaoGerencial>> ObterDecisoesPorProdutoAsync(int produtoId);
        Task<List<DecisaoGerencial>> ObterDecisoesPorPeriodoAsync(DateTime inicio, DateTime fim);
        Task<List<DecisaoGerencial>> ObterDecisoesRecentesAsync(int quantidade = 20);
        Task<DecisaoGerencial?> ObterPorIdAsync(int id);
        
        // Análise de decisões
        Task<List<DecisaoGerencial>> BuscarDecisoesSemelhanteAsync(string problema);
        Task<Dictionary<string, int>> ObterEstatisticasDecisoesAsync();
        Task<List<DecisaoGerencial>> ObterDecisoesPorUsuarioAsync(int usuarioId);
        
        // Avaliação de resultados
        Task<bool> AvaliarResultadoDecisaoAsync(int decisaoId, string resultado, bool foiEfetiva);
        Task<List<DecisaoGerencial>> ObterDecisoesParaAvaliacaoAsync();
    }
}