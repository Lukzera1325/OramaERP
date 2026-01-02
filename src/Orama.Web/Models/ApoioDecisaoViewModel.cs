using Orama.Application.Services;
using Orama.Domain.Entities;

namespace Orama.Web.Models
{
    // ViewModel para o dashboard
    public class ApoioDecisaoViewModel
    {
        public List<SugestaoAcao> SugestoesPendentes { get; set; } = new();
        public Dictionary<string, int> EstatisticasSugestoes { get; set; } = new();
        
        public List<DecisaoGerencial> DecisoesRecentes { get; set; } = new();
        public Dictionary<string, int> EstatisticasDecisoes { get; set; } = new();
        
        public ChecklistFechamento? ChecklistAtual { get; set; }
        public ChecklistResumoDto ResumoMensal { get; set; } = new();
    }
}