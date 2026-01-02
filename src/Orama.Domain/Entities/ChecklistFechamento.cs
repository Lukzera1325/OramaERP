using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities
{
    public class ChecklistFechamento
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        
        // Período do checklist
        public int Ano { get; set; }
        public int Mes { get; set; }
        public string PeriodoDescricao { get; set; } = string.Empty; // Ex: "Janeiro/2025"
        
        // Status geral
        public StatusChecklist Status { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataConclusao { get; set; }
        public int? UsuarioConclusaoId { get; set; }
        
        // Itens do checklist
        public List<ChecklistItem> Itens { get; set; } = new();
        
        // Relacionamentos
        public Empresa Empresa { get; set; } = null!;
        public Usuario? UsuarioConclusao { get; set; }
        
        // Método de criação
        public static ChecklistFechamento CriarParaPeriodo(int empresaId, int ano, int mes)
        {
            var checklist = new ChecklistFechamento
            {
                EmpresaId = empresaId,
                Ano = ano,
                Mes = mes,
                PeriodoDescricao = $"{ObterNomeMes(mes)}/{ano}",
                Status = StatusChecklist.EmAndamento,
                DataCriacao = DateTime.Now,
                Itens = new List<ChecklistItem>()
            };
            
            // Criar itens padrão do checklist
            checklist.AdicionarItensPadrao();
            
            return checklist;
        }
        
        private void AdicionarItensPadrao()
        {
            var itensPadrao = new[]
            {
                new { Ordem = 1, Descricao = "Revisar vendas com margem negativa", Categoria = CategoriaItem.Vendas, Obrigatorio = true },
                new { Ordem = 2, Descricao = "Analisar alertas pendentes", Categoria = CategoriaItem.Alertas, Obrigatorio = true },
                new { Ordem = 3, Descricao = "Revisar preços de produtos com prejuízo", Categoria = CategoriaItem.Precos, Obrigatorio = true },
                new { Ordem = 4, Descricao = "Atualizar custos de produtos", Categoria = CategoriaItem.Custos, Obrigatorio = false },
                new { Ordem = 5, Descricao = "Verificar estoque de produtos em falta", Categoria = CategoriaItem.Estoque, Obrigatorio = false },
                new { Ordem = 6, Descricao = "Analisar produtos mais lucrativos", Categoria = CategoriaItem.Lucratividade, Obrigatorio = false },
                new { Ordem = 7, Descricao = "Revisar estruturas de produtos (BOM)", Categoria = CategoriaItem.Producao, Obrigatorio = false },
                new { Ordem = 8, Descricao = "Verificar contas a receber em atraso", Categoria = CategoriaItem.Financeiro, Obrigatorio = false },
                new { Ordem = 9, Descricao = "Verificar contas a pagar próximas do vencimento", Categoria = CategoriaItem.Financeiro, Obrigatorio = false },
                new { Ordem = 10, Descricao = "Analisar evolução da margem no período", Categoria = CategoriaItem.Lucratividade, Obrigatorio = false }
            };
            
            foreach (var item in itensPadrao)
            {
                Itens.Add(new ChecklistItem
                {
                    ChecklistFechamentoId = Id,
                    Ordem = item.Ordem,
                    Descricao = item.Descricao,
                    Categoria = item.Categoria,
                    Obrigatorio = item.Obrigatorio,
                    Concluido = false,
                    DataCriacao = DateTime.Now
                });
            }
        }
        
        public void MarcarItemConcluido(int itemId, int usuarioId, string observacoes = "")
        {
            var item = Itens.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                item.MarcarConcluido(usuarioId, observacoes);
                AtualizarStatusGeral();
            }
        }
        
        public void DesmarcarItem(int itemId)
        {
            var item = Itens.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                item.Desmarcar();
                AtualizarStatusGeral();
            }
        }
        
        private void AtualizarStatusGeral()
        {
            var itensObrigatorios = Itens.Where(i => i.Obrigatorio).ToList();
            var todosObrigatoriosConcluidos = itensObrigatorios.All(i => i.Concluido);
            
            if (todosObrigatoriosConcluidos && Status != StatusChecklist.Concluido)
            {
                Status = StatusChecklist.ProntoParaConcluir;
            }
            else if (!todosObrigatoriosConcluidos && Status != StatusChecklist.EmAndamento)
            {
                Status = StatusChecklist.EmAndamento;
            }
        }
        
        public void ConcluirChecklist(int usuarioId)
        {
            var itensObrigatorios = Itens.Where(i => i.Obrigatorio).ToList();
            if (!itensObrigatorios.All(i => i.Concluido))
            {
                throw new InvalidOperationException("Não é possível concluir o checklist. Existem itens obrigatórios pendentes.");
            }
            
            Status = StatusChecklist.Concluido;
            DataConclusao = DateTime.Now;
            UsuarioConclusaoId = usuarioId;
        }
        
        public void ReabrirChecklist()
        {
            if (Status == StatusChecklist.Concluido)
            {
                Status = StatusChecklist.EmAndamento;
                DataConclusao = null;
                UsuarioConclusaoId = null;
            }
        }
        
        // Propriedades calculadas
        public string StatusDescricao => Status switch
        {
            StatusChecklist.EmAndamento => "Em Andamento",
            StatusChecklist.ProntoParaConcluir => "Pronto para Concluir",
            StatusChecklist.Concluido => "Concluído",
            _ => "Desconhecido"
        };
        
        public string CssClassStatus => Status switch
        {
            StatusChecklist.EmAndamento => "badge-warning",
            StatusChecklist.ProntoParaConcluir => "badge-info",
            StatusChecklist.Concluido => "badge-success",
            _ => "badge-secondary"
        };
        
        public int TotalItens => Itens.Count;
        public int ItensConcluidos => Itens.Count(i => i.Concluido);
        public int ItensObrigatorios => Itens.Count(i => i.Obrigatorio);
        public int ItensObrigatoriosConcluidos => Itens.Count(i => i.Obrigatorio && i.Concluido);
        public decimal PercentualConclusao => TotalItens > 0 ? (decimal)ItensConcluidos / TotalItens * 100 : 0;
        public decimal PercentualObrigatorios => ItensObrigatorios > 0 ? (decimal)ItensObrigatoriosConcluidos / ItensObrigatorios * 100 : 0;
        
        private static string ObterNomeMes(int mes) => mes switch
        {
            1 => "Janeiro", 2 => "Fevereiro", 3 => "Março", 4 => "Abril",
            5 => "Maio", 6 => "Junho", 7 => "Julho", 8 => "Agosto",
            9 => "Setembro", 10 => "Outubro", 11 => "Novembro", 12 => "Dezembro",
            _ => "Mês Inválido"
        };
    }
    
    public class ChecklistItem
    {
        public int Id { get; set; }
        public int ChecklistFechamentoId { get; set; }
        
        public int Ordem { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public CategoriaItem Categoria { get; set; }
        public bool Obrigatorio { get; set; }
        
        public bool Concluido { get; set; }
        public DateTime? DataConclusao { get; set; }
        public int? UsuarioConclusaoId { get; set; }
        public string? ObservacoesConclusao { get; set; }
        
        public DateTime DataCriacao { get; set; }
        
        // Relacionamentos
        public ChecklistFechamento ChecklistFechamento { get; set; } = null!;
        public Usuario? UsuarioConclusao { get; set; }
        
        public void MarcarConcluido(int usuarioId, string observacoes = "")
        {
            Concluido = true;
            DataConclusao = DateTime.Now;
            UsuarioConclusaoId = usuarioId;
            ObservacoesConclusao = observacoes;
        }
        
        public void Desmarcar()
        {
            Concluido = false;
            DataConclusao = null;
            UsuarioConclusaoId = null;
            ObservacoesConclusao = null;
        }
        
        // Propriedades calculadas
        public string CategoriaDescricao => Categoria switch
        {
            CategoriaItem.Vendas => "Vendas",
            CategoriaItem.Alertas => "Alertas",
            CategoriaItem.Precos => "Preços",
            CategoriaItem.Custos => "Custos",
            CategoriaItem.Estoque => "Estoque",
            CategoriaItem.Lucratividade => "Lucratividade",
            CategoriaItem.Producao => "Produção",
            CategoriaItem.Financeiro => "Financeiro",
            _ => "Geral"
        };
        
        public string CssClassCategoria => Categoria switch
        {
            CategoriaItem.Vendas => "badge-primary",
            CategoriaItem.Alertas => "badge-danger",
            CategoriaItem.Precos => "badge-warning",
            CategoriaItem.Custos => "badge-info",
            CategoriaItem.Estoque => "badge-secondary",
            CategoriaItem.Lucratividade => "badge-success",
            CategoriaItem.Producao => "badge-dark",
            CategoriaItem.Financeiro => "badge-light",
            _ => "badge-secondary"
        };
        
        public string IconeCategoria => Categoria switch
        {
            CategoriaItem.Vendas => "fas fa-shopping-cart",
            CategoriaItem.Alertas => "fas fa-exclamation-triangle",
            CategoriaItem.Precos => "fas fa-tag",
            CategoriaItem.Custos => "fas fa-calculator",
            CategoriaItem.Estoque => "fas fa-boxes",
            CategoriaItem.Lucratividade => "fas fa-chart-line",
            CategoriaItem.Producao => "fas fa-industry",
            CategoriaItem.Financeiro => "fas fa-dollar-sign",
            _ => "fas fa-check"
        };
    }
    
    public enum StatusChecklist
    {
        EmAndamento = 1,
        ProntoParaConcluir = 2,
        Concluido = 3
    }
    
    public enum CategoriaItem
    {
        Vendas = 1,
        Alertas = 2,
        Precos = 3,
        Custos = 4,
        Estoque = 5,
        Lucratividade = 6,
        Producao = 7,
        Financeiro = 8
    }
}