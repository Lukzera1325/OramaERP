using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orama.Domain.Entities;

namespace Orama.Web.Models
{
    public class ListaMateriaisViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O produto é obrigatório")]
        [Display(Name = "Produto")]
        public int ProdutoId { get; set; }
        
        [Display(Name = "Produto")]
        public string? ProdutoDescricao { get; set; }
        
        [Required(ErrorMessage = "A versão é obrigatória")]
        [StringLength(20, ErrorMessage = "A versão não pode exceder 20 caracteres")]
        [Display(Name = "Versão")]
        public string Versao { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "A descrição não pode exceder 200 caracteres")]
        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }
        
        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;
        
        [Required(ErrorMessage = "A data de vigência é obrigatória")]
        [Display(Name = "Data de Vigência")]
        [DataType(DataType.Date)]
        public DateTime DataVigencia { get; set; }
        
        [Display(Name = "Data de Vencimento")]
        [DataType(DataType.Date)]
        public DateTime? DataVencimento { get; set; }
        
        [Required(ErrorMessage = "A quantidade base é obrigatória")]
        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade base deve ser maior que zero")]
        [Display(Name = "Quantidade Base")]
        public decimal QuantidadeBase { get; set; } = 1;
        
        [Required(ErrorMessage = "A unidade de medida é obrigatória")]
        [StringLength(10, ErrorMessage = "A unidade de medida não pode exceder 10 caracteres")]
        [Display(Name = "Unidade de Medida")]
        public string UnidadeMedida { get; set; } = "UN";
        
        [Display(Name = "Custo Total")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal CustoTotal { get; set; }
        
        [Display(Name = "Data de Criação")]
        [DataType(DataType.DateTime)]
        public DateTime DataCriacao { get; set; }
        
        [Display(Name = "Criado por")]
        public string? UsuarioCriacao { get; set; }
        
        // Listas para dropdowns
        public SelectList? Produtos { get; set; }
        
        // Itens da lista de materiais
        public List<ListaMateriaisItemViewModel> Itens { get; set; } = new();
        
        // Propriedades auxiliares
        public string StatusDescricao => Ativo ? "Ativo" : "Inativo";
        public string StatusCssClass => Ativo ? "badge bg-success" : "badge bg-secondary";
        
        public bool PodeEditar => Ativo;
        public bool PodeExcluir => true; // Verificação adicional no service
        
        public decimal CustoUnitario
        {
            get
            {
                if (QuantidadeBase == 0) return 0;
                return CustoTotal / QuantidadeBase;
            }
        }
    }
    
    public class ListaMateriaisItemViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O produto é obrigatório")]
        [Display(Name = "Produto")]
        public int ProdutoId { get; set; }
        
        [Display(Name = "Produto")]
        public string ProdutoDescricao { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "A quantidade é obrigatória")]
        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        [Display(Name = "Quantidade")]
        public decimal Quantidade { get; set; }
        
        [Required(ErrorMessage = "A unidade de medida é obrigatória")]
        [StringLength(10, ErrorMessage = "A unidade de medida não pode exceder 10 caracteres")]
        [Display(Name = "Unidade")]
        public string UnidadeMedida { get; set; } = "UN";
        
        [Required(ErrorMessage = "O custo unitário é obrigatório")]
        [Range(0, double.MaxValue, ErrorMessage = "O custo unitário deve ser maior ou igual a zero")]
        [Display(Name = "Custo Unitário")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal CustoUnitario { get; set; }
        
        [Display(Name = "Custo Total")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal CustoTotal => Quantidade * CustoUnitario;
        
        [Display(Name = "Tipo")]
        public TipoComponente Tipo { get; set; }
        
        [Display(Name = "Crítico")]
        public bool Critico { get; set; }
        
        [Range(0, 100, ErrorMessage = "O percentual de perda deve estar entre 0 e 100")]
        [Display(Name = "% Perda")]
        public decimal PercentualPerda { get; set; }
        
        [Display(Name = "Qtd. Líquida")]
        public decimal QuantidadeLiquida => Quantidade * (1 + PercentualPerda / 100);
        
        [StringLength(200, ErrorMessage = "As observações não podem exceder 200 caracteres")]
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }
        
        // Propriedades auxiliares
        public string TipoDescricao => Tipo switch
        {
            TipoComponente.MateriaPrima => "Matéria Prima",
            TipoComponente.Componente => "Componente",
            TipoComponente.Subconjunto => "Subconjunto",
            TipoComponente.Embalagem => "Embalagem",
            TipoComponente.Insumo => "Insumo",
            TipoComponente.Ferramenta => "Ferramenta",
            _ => "Desconhecido"
        };
        
        public string CriticoCssClass => Critico ? "badge bg-danger" : "badge bg-secondary";
        public string CriticoDescricao => Critico ? "Crítico" : "Normal";
    }
    
    public class ExplosaoMateriaisViewModel
    {
        [Required(ErrorMessage = "O produto é obrigatório")]
        [Display(Name = "Produto")]
        public int ProdutoId { get; set; }
        
        [Display(Name = "Produto")]
        public string? ProdutoDescricao { get; set; }
        
        [Required(ErrorMessage = "A quantidade é obrigatória")]
        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        [Display(Name = "Quantidade")]
        public decimal Quantidade { get; set; } = 1;
        
        // Listas para dropdowns
        public SelectList? Produtos { get; set; }
        
        // Resultado da explosão
        public List<MaterialExplodidoViewModel> Materiais { get; set; } = new();
        
        // Materiais insuficientes
        public List<MaterialInsuficienteViewModel> MateriaisInsuficientes { get; set; } = new();
        
        public bool TemMateriaisInsuficientes => MateriaisInsuficientes.Any();
        public decimal CustoTotalProducao { get; set; }
    }
    
    public class MaterialExplodidoViewModel
    {
        public int ProdutoId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal QuantidadeTotal { get; set; }
        public string Unidade { get; set; } = string.Empty;
        public decimal CustoUnitario { get; set; }
        public decimal CustoTotal => QuantidadeTotal * CustoUnitario;
        public decimal EstoqueAtual { get; set; }
        public bool TemEstoqueSuficiente => EstoqueAtual >= QuantidadeTotal;
        
        public string StatusEstoque => TemEstoqueSuficiente ? "Suficiente" : "Insuficiente";
        public string StatusCssClass => TemEstoqueSuficiente ? "text-success" : "text-danger";
    }
    
    public class MaterialInsuficienteViewModel
    {
        public int ProdutoId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal QuantidadeNecessaria { get; set; }
        public decimal EstoqueAtual { get; set; }
        public decimal QuantidadeFaltante => QuantidadeNecessaria - EstoqueAtual;
        public string Unidade { get; set; } = string.Empty;
    }
}