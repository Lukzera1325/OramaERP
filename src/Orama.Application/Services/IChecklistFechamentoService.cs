using Orama.Domain.Entities;

namespace Orama.Application.Services
{
    public interface IChecklistFechamentoService
    {
        // Gestão de checklists
        Task<ChecklistFechamento> CriarChecklistMensalAsync(int ano, int mes);
        Task<ChecklistFechamento?> ObterChecklistAtualAsync();
        Task<ChecklistFechamento?> ObterChecklistPorPeriodoAsync(int ano, int mes);
        Task<List<ChecklistFechamento>> ObterHistoricoChecklistsAsync();
        
        // Gestão de itens
        Task<bool> MarcarItemConcluidoAsync(int checklistId, int itemId, string observacoes = "");
        Task<bool> DesmarcarItemAsync(int checklistId, int itemId);
        Task<bool> ConcluirChecklistAsync(int checklistId);
        Task<bool> ReabrirChecklistAsync(int checklistId);
        
        // Consultas
        Task<ChecklistFechamento?> ObterPorIdAsync(int id);
        Task<List<ChecklistItem>> ObterItensPendentesAsync(int checklistId);
        Task<Dictionary<string, int>> ObterEstatisticasChecklistAsync(int checklistId);
        
        // Análise
        Task<List<ChecklistFechamento>> ObterChecklistsAtrasadosAsync();
        Task<ChecklistResumoDto> ObterResumoMensalAsync(int ano, int mes);
    }
    
    public class ChecklistResumoDto
    {
        public int Ano { get; set; }
        public int Mes { get; set; }
        public string PeriodoDescricao { get; set; } = string.Empty;
        public bool ChecklistExiste { get; set; }
        public StatusChecklist? Status { get; set; }
        public string StatusDescricao { get; set; } = string.Empty;
        
        public int TotalItens { get; set; }
        public int ItensConcluidos { get; set; }
        public int ItensObrigatorios { get; set; }
        public int ItensObrigatoriosConcluidos { get; set; }
        
        public decimal PercentualConclusao { get; set; }
        public decimal PercentualObrigatorios { get; set; }
        
        public DateTime? DataCriacao { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string? UsuarioConclusao { get; set; }
        
        public List<ItemPendenteDto> ItensPendentes { get; set; } = new();
        public List<string> Recomendacoes { get; set; } = new();
    }
    
    public class ItemPendenteDto
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public bool Obrigatorio { get; set; }
        public int DiasDesdeCreacao { get; set; }
    }
}