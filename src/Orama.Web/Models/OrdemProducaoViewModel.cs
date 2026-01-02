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
        
        // Propriedades auxiliares
        public string StatusDescricao => Status switch
        {
            StatusOrdemProducao.Planejada => "Planejada",
            StatusOrdemProducao.Liberada => "Liberada",
            StatusOrdemProducao.EmAndamento => "Em Andamento",
            StatusOrdemProducao.Finalizada => "Finalizada",
            StatusOrdemProducao.Cancelada => "Cancelada",
            _ => "Desconhecido"
        };
        
        public string StatusCssClass => Status switch
        {
            StatusOrdemProducao.Planejada => "badge bg-secondary",
            StatusOrdemProducao.Liberada => "badge bg-primary",
            StatusOrdemProducao.EmAndamento => "badge bg-warning",
            StatusOrdemProducao.Finalizada => "badge bg-success",
            StatusOrdemProducao.Cancelada => "badge bg-dark",
            _ => "badge bg-light"
        };
        
        public bool PodeEditar => Status == StatusOrdemProducao.Planejada;
        public bool PodeLiberarOrdem => Status == StatusOrdemProducao.Planejada;
        public bool PodeIniciarProducao => Status == StatusOrdemProducao.Liberada;
        public bool PodeFinalizarProducao => Status == StatusOrdemProducao.EmAndamento;
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
    
}