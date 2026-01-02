using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orama.Domain.Entities;

namespace Orama.Web.Models
{
    /// <summary>
    /// Tipo de item na produção
    /// </summary>
    public enum TipoItemProducao
    {
        MateriaPrima = 1,
        Componente = 2,
        Embalagem = 3,
        Insumo = 4
    }

    public class OrdemProducaoViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Número")]
        public string Numero { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O produto é obrigatório")]
        [Display(Name = "Produto")]
        public int ProdutoId { get; set; }
        
        [Display(Name = "Produto")]
        public string? ProdutoDescricao { get; set; }
        
        // Propriedades auxiliares para compatibilidade com views
        public string? ProdutoNome => ProdutoDescricao;
        public decimal Quantidade => QuantidadePlanejada;
        public DateTime? DataPrevista => DataPlanejada;
        public DateTime? DataLimite => DataFim;
        
        [Required(ErrorMessage = "A quantidade planejada é obrigatória")]
        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        [Display(Name = "Quantidade Planejada")]
        public decimal QuantidadePlanejada { get; set; }
        
        [Display(Name = "Quantidade Produzida")]
        public decimal QuantidadeProduzida { get; set; }
        
        [Required(ErrorMessage = "A data planejada é obrigatória")]
        [Display(Name = "Data Planejada")]
        [DataType(DataType.Date)]
        public DateTime DataPlanejada { get; set; }
        
        [Display(Name = "Data de Início")]
        [DataType(DataType.DateTime)]
        public DateTime? DataInicio { get; set; }
        
        [Display(Name = "Data de Fim")]
        [DataType(DataType.DateTime)]
        public DateTime? DataFim { get; set; }
        
        [Display(Name = "Status")]
        public StatusOrdemProducao Status { get; set; }
        
        [Display(Name = "Prioridade")]
        public PrioridadeOrdemProducao Prioridade { get; set; }
        
        [Display(Name = "Observações")]
        [StringLength(500, ErrorMessage = "As observações não podem exceder 500 caracteres")]
        public string? Observacoes { get; set; }
        
        [Display(Name = "Custo Material")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal CustoMaterial { get; set; }
        
        [Display(Name = "Custo Mão de Obra")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal CustoMaoObra { get; set; }
        
        [Display(Name = "Custo Total")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal CustoTotal => CustoMaterial + CustoMaoObra;
        
        [Display(Name = "Data de Criação")]
        [DataType(DataType.DateTime)]
        public DateTime DataCriacao { get; set; }
        
        [Display(Name = "Criado por")]
        public string? UsuarioCriacao { get; set; }
        
        // Listas para dropdowns
        public SelectList? Produtos { get; set; }
        
        // Itens da ordem
        public List<OrdemProducaoItemViewModel> Itens { get; set; } = new();
        
        // Etapas da ordem
        public List<OrdemProducaoEtapaViewModel> Etapas { get; set; } = new();
        
        // Apontamentos de horas
        public List<ApontamentoHorasViewModel> ApontamentosHoras { get; set; } = new();
        
        // Inspeções de qualidade
        public List<InspecaoQualidadeViewModel> InspecoesQualidade { get; set; } = new();
        
        // Propriedades auxiliares
        public string StatusDescricao => Status switch
        {
            StatusOrdemProducao.Planejada => "Planejada",
            StatusOrdemProducao.Liberada => "Liberada",
            StatusOrdemProducao.EmAndamento => "Em Andamento",
            StatusOrdemProducao.Pausada => "Pausada",
            StatusOrdemProducao.Finalizada => "Finalizada",
            StatusOrdemProducao.Cancelada => "Cancelada",
            _ => "Desconhecido"
        };
        
        public string StatusCssClass => Status switch
        {
            StatusOrdemProducao.Planejada => "badge bg-secondary",
            StatusOrdemProducao.Liberada => "badge bg-primary",
            StatusOrdemProducao.EmAndamento => "badge bg-warning",
            StatusOrdemProducao.Pausada => "badge bg-danger",
            StatusOrdemProducao.Finalizada => "badge bg-success",
            StatusOrdemProducao.Cancelada => "badge bg-dark",
            _ => "badge bg-light"
        };
        
        public string PrioridadeDescricao => Prioridade switch
        {
            PrioridadeOrdemProducao.Baixa => "Baixa",
            PrioridadeOrdemProducao.Normal => "Normal",
            PrioridadeOrdemProducao.Alta => "Alta",
            PrioridadeOrdemProducao.Urgente => "Urgente",
            _ => "Normal"
        };
        
        public string PrioridadeCssClass => Prioridade switch
        {
            PrioridadeOrdemProducao.Baixa => "text-muted",
            PrioridadeOrdemProducao.Normal => "text-primary",
            PrioridadeOrdemProducao.Alta => "text-warning",
            PrioridadeOrdemProducao.Urgente => "text-danger",
            _ => "text-primary"
        };
        
        public bool PodeEditar => Status == StatusOrdemProducao.Planejada;
        public bool PodeLiberarOrdem => Status == StatusOrdemProducao.Planejada;
        public bool PodeIniciarProducao => Status == StatusOrdemProducao.Liberada;
        public bool PodePausarProducao => Status == StatusOrdemProducao.EmAndamento;
        public bool PodeRetomarProducao => Status == StatusOrdemProducao.Pausada;
        public bool PodeFinalizarProducao => Status == StatusOrdemProducao.EmAndamento || Status == StatusOrdemProducao.Pausada;
        public bool PodeCancelarOrdem => Status != StatusOrdemProducao.Finalizada && Status != StatusOrdemProducao.Cancelada;
        public bool PodeExcluir => Status == StatusOrdemProducao.Planejada;
        
        public decimal PercentualConclusao
        {
            get
            {
                if (QuantidadePlanejada == 0) return 0;
                return Math.Min(100, (QuantidadeProduzida / QuantidadePlanejada) * 100);
            }
        }
    }
    
    public class OrdemProducaoItemViewModel
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public string ProdutoDescricao { get; set; } = string.Empty;
        public decimal QuantidadeNecessaria { get; set; }
        public decimal QuantidadeConsumida { get; set; }
        public decimal CustoUnitario { get; set; }
        public decimal CustoTotal => QuantidadeNecessaria * CustoUnitario;
        public TipoItemProducao Tipo { get; set; }
        public string? Observacoes { get; set; }
        
        public string TipoDescricao => Tipo switch
        {
            TipoItemProducao.MateriaPrima => "Matéria Prima",
            TipoItemProducao.Componente => "Componente",
            TipoItemProducao.Embalagem => "Embalagem",
            TipoItemProducao.Insumo => "Insumo",
            _ => "Desconhecido"
        };
    }
    
    public class OrdemProducaoEtapaViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public int Sequencia { get; set; }
        public decimal TempoEstimado { get; set; }
        public decimal TempoRealizado { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public StatusEtapaProducao Status { get; set; }
        public string? ResponsavelNome { get; set; }
        public string? Observacoes { get; set; }
        
        public string StatusDescricao => Status switch
        {
            StatusEtapaProducao.Pendente => "Pendente",
            StatusEtapaProducao.EmAndamento => "Em Andamento",
            StatusEtapaProducao.Pausada => "Pausada",
            StatusEtapaProducao.Concluida => "Concluída",
            StatusEtapaProducao.Cancelada => "Cancelada",
            _ => "Desconhecido"
        };
        
        public string StatusCssClass => Status switch
        {
            StatusEtapaProducao.Pendente => "badge bg-secondary",
            StatusEtapaProducao.EmAndamento => "badge bg-warning",
            StatusEtapaProducao.Pausada => "badge bg-danger",
            StatusEtapaProducao.Concluida => "badge bg-success",
            StatusEtapaProducao.Cancelada => "badge bg-dark",
            _ => "badge bg-light"
        };
        
        public decimal PercentualConclusao
        {
            get
            {
                if (TempoEstimado == 0) return 0;
                return Math.Min(100, (TempoRealizado / TempoEstimado) * 100);
            }
        }
    }
    
    public class ApontamentoHorasViewModel
    {
        public int Id { get; set; }
        public string? EtapaNome { get; set; }
        public string FuncionarioNome { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public decimal HorasTrabalhadas { get; set; }
        public decimal ValorHora { get; set; }
        public decimal CustoTotal => HorasTrabalhadas * ValorHora;
        public TipoApontamento Tipo { get; set; }
        public string? Observacoes { get; set; }
        
        public string TipoDescricao => Tipo switch
        {
            TipoApontamento.Producao => "Produção",
            TipoApontamento.Setup => "Setup",
            TipoApontamento.Manutencao => "Manutenção",
            TipoApontamento.Parada => "Parada",
            TipoApontamento.Qualidade => "Qualidade",
            _ => "Desconhecido"
        };
    }
    
    public class InspecaoQualidadeViewModel
    {
        public int Id { get; set; }
        public string? EtapaNome { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public TipoInspecao Tipo { get; set; }
        public decimal QuantidadeInspecionada { get; set; }
        public decimal QuantidadeAprovada { get; set; }
        public decimal QuantidadeRejeitada { get; set; }
        public ResultadoInspecao Resultado { get; set; }
        public DateTime DataInspecao { get; set; }
        public string InspetorNome { get; set; } = string.Empty;
        public string? Observacoes { get; set; }
        
        public string TipoDescricao => Tipo switch
        {
            TipoInspecao.Entrada => "Entrada",
            TipoInspecao.Processo => "Processo",
            TipoInspecao.Final => "Final",
            TipoInspecao.Auditoria => "Auditoria",
            _ => "Desconhecido"
        };
        
        public string ResultadoDescricao => Resultado switch
        {
            ResultadoInspecao.Aprovado => "Aprovado",
            ResultadoInspecao.Rejeitado => "Rejeitado",
            ResultadoInspecao.CondicionalmenteAprovado => "Condicionalmente Aprovado",
            ResultadoInspecao.EmAnalise => "Em Análise",
            _ => "Desconhecido"
        };
        
        public string ResultadoCssClass => Resultado switch
        {
            ResultadoInspecao.Aprovado => "badge bg-success",
            ResultadoInspecao.Rejeitado => "badge bg-danger",
            ResultadoInspecao.CondicionalmenteAprovado => "badge bg-warning",
            ResultadoInspecao.EmAnalise => "badge bg-info",
            _ => "badge bg-light"
        };
        
        public decimal PercentualAprovacao
        {
            get
            {
                if (QuantidadeInspecionada == 0) return 0;
                return (QuantidadeAprovada / QuantidadeInspecionada) * 100;
            }
        }
    }
}