using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services
{
    public class ChecklistFechamentoService : IChecklistFechamentoService
    {
        private readonly OramaDbContext _context;
        
        public ChecklistFechamentoService(OramaDbContext context)
        {
            _context = context;
        }
        
        public async Task<ChecklistFechamento> CriarChecklistMensalAsync(int ano, int mes)
        {
            // Para simplificar, vou usar empresa ID 1 por padrão
            const int empresaId = 1;
            
            // Verificar se já existe checklist para o período
            var checklistExistente = await _context.ChecklistsFechamento
                .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.Ano == ano && c.Mes == mes);
                
            if (checklistExistente != null)
                return checklistExistente;
                
            var checklist = ChecklistFechamento.CriarParaPeriodo(empresaId, ano, mes);
            
            _context.ChecklistsFechamento.Add(checklist);
            await _context.SaveChangesAsync();
            
            return checklist;
        }
        
        public async Task<ChecklistFechamento?> ObterChecklistAtualAsync()
        {
            var agora = DateTime.Now;
            return await ObterChecklistPorPeriodoAsync(agora.Year, agora.Month);
        }
        
        public async Task<ChecklistFechamento?> ObterChecklistPorPeriodoAsync(int ano, int mes)
        {
            return await _context.ChecklistsFechamento
                .Include(c => c.Itens)
                .Include(c => c.UsuarioConclusao)
                .FirstOrDefaultAsync(c => c.Ano == ano && c.Mes == mes);
        }
        
        public async Task<List<ChecklistFechamento>> ObterHistoricoChecklistsAsync()
        {
            return await _context.ChecklistsFechamento
                .Include(c => c.UsuarioConclusao)
                .OrderByDescending(c => c.Ano)
                .ThenByDescending(c => c.Mes)
                .ToListAsync();
        }
        
        public async Task<bool> MarcarItemConcluidoAsync(int checklistId, int itemId, string observacoes = "")
        {
            var checklist = await _context.ChecklistsFechamento
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.Id == checklistId);
                
            if (checklist == null) return false;
            
            checklist.MarcarItemConcluido(itemId, 1, observacoes); // TODO: Pegar usuário real
            
            return await _context.SaveChangesAsync() > 0;
        }
        
        public async Task<bool> DesmarcarItemAsync(int checklistId, int itemId)
        {
            var checklist = await _context.ChecklistsFechamento
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.Id == checklistId);
                
            if (checklist == null) return false;
            
            checklist.DesmarcarItem(itemId);
            
            return await _context.SaveChangesAsync() > 0;
        }
        
        public async Task<bool> ConcluirChecklistAsync(int checklistId)
        {
            var checklist = await _context.ChecklistsFechamento
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.Id == checklistId);
                
            if (checklist == null) return false;
            
            try
            {
                checklist.ConcluirChecklist(1); // TODO: Pegar usuário real
                return await _context.SaveChangesAsync() > 0;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }
        
        public async Task<bool> ReabrirChecklistAsync(int checklistId)
        {
            var checklist = await _context.ChecklistsFechamento
                .FirstOrDefaultAsync(c => c.Id == checklistId);
                
            if (checklist == null) return false;
            
            checklist.ReabrirChecklist();
            
            return await _context.SaveChangesAsync() > 0;
        }
        
        public async Task<ChecklistFechamento?> ObterPorIdAsync(int id)
        {
            return await _context.ChecklistsFechamento
                .Include(c => c.Itens)
                    .ThenInclude(i => i.UsuarioConclusao)
                .Include(c => c.UsuarioConclusao)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        
        public async Task<List<ChecklistItem>> ObterItensPendentesAsync(int checklistId)
        {
            return await _context.ChecklistItens
                .Where(i => i.ChecklistFechamentoId == checklistId && !i.Concluido)
                .OrderBy(i => i.Ordem)
                .ToListAsync();
        }
        
        public async Task<Dictionary<string, int>> ObterEstatisticasChecklistAsync(int checklistId)
        {
            var checklist = await _context.ChecklistsFechamento
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.Id == checklistId);
                
            if (checklist == null)
                return new Dictionary<string, int>();
                
            var estatisticas = new Dictionary<string, int>
            {
                ["Total Itens"] = checklist.TotalItens,
                ["Itens Concluídos"] = checklist.ItensConcluidos,
                ["Itens Pendentes"] = checklist.TotalItens - checklist.ItensConcluidos,
                ["Itens Obrigatórios"] = checklist.ItensObrigatorios,
                ["Obrigatórios Concluídos"] = checklist.ItensObrigatoriosConcluidos,
                ["Obrigatórios Pendentes"] = checklist.ItensObrigatorios - checklist.ItensObrigatoriosConcluidos
            };
            
            // Estatísticas por categoria
            foreach (CategoriaItem categoria in Enum.GetValues<CategoriaItem>())
            {
                var itensCategoria = checklist.Itens.Where(i => i.Categoria == categoria).ToList();
                if (itensCategoria.Any())
                {
                    estatisticas[$"{categoria} - Total"] = itensCategoria.Count;
                    estatisticas[$"{categoria} - Concluídos"] = itensCategoria.Count(i => i.Concluido);
                }
            }
            
            return estatisticas;
        }
        
        public async Task<List<ChecklistFechamento>> ObterChecklistsAtrasadosAsync()
        {
            // Checklists que deveriam estar concluídos (mês anterior ou anterior)
            var agora = DateTime.Now;
            var mesPassado = agora.AddMonths(-1);
            
            return await _context.ChecklistsFechamento
                .Where(c => c.Status != StatusChecklist.Concluido &&
                           (c.Ano < mesPassado.Year || (c.Ano == mesPassado.Year && c.Mes <= mesPassado.Month)))
                .OrderBy(c => c.Ano)
                .ThenBy(c => c.Mes)
                .ToListAsync();
        }
        
        public async Task<ChecklistResumoDto> ObterResumoMensalAsync(int ano, int mes)
        {
            var checklist = await ObterChecklistPorPeriodoAsync(ano, mes);
            
            var resumo = new ChecklistResumoDto
            {
                Ano = ano,
                Mes = mes,
                PeriodoDescricao = $"{ObterNomeMes(mes)}/{ano}",
                ChecklistExiste = checklist != null
            };
            
            if (checklist != null)
            {
                resumo.Status = checklist.Status;
                resumo.StatusDescricao = checklist.StatusDescricao;
                resumo.TotalItens = checklist.TotalItens;
                resumo.ItensConcluidos = checklist.ItensConcluidos;
                resumo.ItensObrigatorios = checklist.ItensObrigatorios;
                resumo.ItensObrigatoriosConcluidos = checklist.ItensObrigatoriosConcluidos;
                resumo.PercentualConclusao = checklist.PercentualConclusao;
                resumo.PercentualObrigatorios = checklist.PercentualObrigatorios;
                resumo.DataCriacao = checklist.DataCriacao;
                resumo.DataConclusao = checklist.DataConclusao;
                resumo.UsuarioConclusao = checklist.UsuarioConclusao?.Nome;
                
                // Itens pendentes
                resumo.ItensPendentes = checklist.Itens
                    .Where(i => !i.Concluido)
                    .Select(i => new ItemPendenteDto
                    {
                        Id = i.Id,
                        Descricao = i.Descricao,
                        Categoria = i.CategoriaDescricao,
                        Obrigatorio = i.Obrigatorio,
                        DiasDesdeCreacao = (DateTime.Now - i.DataCriacao).Days
                    })
                    .OrderBy(i => i.Obrigatorio ? 0 : 1)
                    .ThenBy(i => i.Descricao)
                    .ToList();
                
                // Recomendações
                var recomendacoes = new List<string>();
                
                if (checklist.Status == StatusChecklist.EmAndamento)
                {
                    var itensPendentes = resumo.ItensPendentes.Count;
                    var itensObrigatoriosPendentes = resumo.ItensPendentes.Count(i => i.Obrigatorio);
                    
                    if (itensObrigatoriosPendentes > 0)
                    {
                        recomendacoes.Add($"Existem {itensObrigatoriosPendentes} itens obrigatórios pendentes");
                    }
                    
                    if (itensPendentes > 5)
                    {
                        recomendacoes.Add("Muitos itens pendentes - priorize os obrigatórios");
                    }
                    
                    var diasNoMes = DateTime.DaysInMonth(ano, mes);
                    var diasPassados = DateTime.Now.Day;
                    
                    if (ano == DateTime.Now.Year && mes == DateTime.Now.Month && diasPassados > diasNoMes * 0.8)
                    {
                        recomendacoes.Add("Final do mês se aproximando - acelere a conclusão");
                    }
                }
                else if (checklist.Status == StatusChecklist.ProntoParaConcluir)
                {
                    recomendacoes.Add("Todos os itens obrigatórios foram concluídos - pode finalizar o checklist");
                }
                else if (checklist.Status == StatusChecklist.Concluido)
                {
                    recomendacoes.Add("Checklist concluído com sucesso");
                }
                
                resumo.Recomendacoes = recomendacoes;
            }
            else
            {
                resumo.Recomendacoes.Add("Checklist ainda não foi criado para este período");
                
                // Se for mês atual ou passado, recomendar criação
                var agora = DateTime.Now;
                if ((ano == agora.Year && mes <= agora.Month) || ano < agora.Year)
                {
                    resumo.Recomendacoes.Add("Recomenda-se criar o checklist para este período");
                }
            }
            
            return resumo;
        }
        
        private static string ObterNomeMes(int mes) => mes switch
        {
            1 => "Janeiro", 2 => "Fevereiro", 3 => "Março", 4 => "Abril",
            5 => "Maio", 6 => "Junho", 7 => "Julho", 8 => "Agosto",
            9 => "Setembro", 10 => "Outubro", 11 => "Novembro", 12 => "Dezembro",
            _ => "Mês Inválido"
        };
    }
}