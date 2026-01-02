using System.ComponentModel.DataAnnotations;

namespace Orama.Web.Models
{
    public class NotaFiscalViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Número é obrigatório")]
        [Display(Name = "Número")]
        public string Numero { get; set; } = string.Empty;

        [Required(ErrorMessage = "Série é obrigatória")]
        [Display(Name = "Série")]
        public string Serie { get; set; } = "1";

        [Required(ErrorMessage = "Tipo é obrigatório")]
        [Display(Name = "Tipo")]
        public string Tipo { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public string Status { get; set; } = "Rascunho";

        [Required(ErrorMessage = "Data de emissão é obrigatória")]
        [Display(Name = "Data de Emissão")]
        [DataType(DataType.Date)]
        public DateTime DataEmissao { get; set; } = DateTime.Now;

        [Display(Name = "Data de Saída")]
        [DataType(DataType.Date)]
        public DateTime? DataSaida { get; set; }

        // Relacionamentos
        public int EmpresaId { get; set; }

        [Display(Name = "Cliente")]
        public int? ClienteId { get; set; }
        public string? ClienteNome { get; set; }

        [Display(Name = "Fornecedor")]
        public int? FornecedorId { get; set; }
        public string? FornecedorNome { get; set; }

        public int? VendaId { get; set; }
        public string? VendaNumero { get; set; }

        public int? CompraId { get; set; }
        public string? CompraNumero { get; set; }

        // Valores
        [Display(Name = "Valor dos Produtos")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal ValorProdutos { get; set; }

        [Display(Name = "Valor do Frete")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal ValorFrete { get; set; }

        [Display(Name = "Valor do Seguro")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal ValorSeguro { get; set; }

        [Display(Name = "Valor do Desconto")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal ValorDesconto { get; set; }

        [Display(Name = "Outras Despesas")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal ValorOutrasDespesas { get; set; }

        [Display(Name = "Valor IPI")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal ValorIPI { get; set; }

        [Display(Name = "Valor ICMS")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal ValorICMS { get; set; }

        [Display(Name = "Valor PIS")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal ValorPIS { get; set; }

        [Display(Name = "Valor COFINS")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal ValorCOFINS { get; set; }

        [Display(Name = "Valor Total")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal ValorTotal { get; set; }

        // Informações Fiscais
        [Required(ErrorMessage = "Natureza da operação é obrigatória")]
        [Display(Name = "Natureza da Operação")]
        public string NaturezaOperacao { get; set; } = string.Empty;

        [Required(ErrorMessage = "CFOP é obrigatório")]
        [Display(Name = "CFOP")]
        public string CFOP { get; set; } = string.Empty;

        [Display(Name = "Chave de Acesso")]
        public string? ChaveAcesso { get; set; }

        [Display(Name = "Protocolo")]
        public string? Protocolo { get; set; }

        [Display(Name = "Data de Autorização")]
        [DataType(DataType.DateTime)]
        public DateTime? DataAutorizacao { get; set; }

        // Observações
        [Display(Name = "Informação Complementar")]
        [DataType(DataType.MultilineText)]
        public string? InformacaoComplementar { get; set; }

        [Display(Name = "Observação para o Fisco")]
        [DataType(DataType.MultilineText)]
        public string? ObservacaoFisco { get; set; }

        // Auditoria
        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; }

        [Display(Name = "Data de Alteração")]
        public DateTime? DataAlteracao { get; set; }

        // Itens
        public List<NotaFiscalItemViewModel> Itens { get; set; } = new List<NotaFiscalItemViewModel>();

        // Propriedades auxiliares
        public string StatusBadgeClass => Status switch
        {
            "Rascunho" => "badge-secondary",
            "Autorizada" => "badge-success",
            "Cancelada" => "badge-danger",
            _ => "badge-secondary"
        };

        public string TipoBadgeClass => Tipo switch
        {
            "Entrada" => "badge-info",
            "Saida" => "badge-warning",
            _ => "badge-secondary"
        };

        public bool PodeEditar => Status == "Rascunho";
        public bool PodeCancelar => Status == "Autorizada";
        public bool PodeAutorizar => Status == "Rascunho";

        public string ObterDestinatario()
        {
            if (Tipo == "Saida" && !string.IsNullOrEmpty(ClienteNome))
                return ClienteNome;
            
            if (Tipo == "Entrada" && !string.IsNullOrEmpty(FornecedorNome))
                return FornecedorNome;
            
            return "Não informado";
        }
    }

    public class NotaFiscalItemViewModel
    {
        public int Id { get; set; }
        public int NotaFiscalId { get; set; }

        [Required(ErrorMessage = "Produto é obrigatório")]
        [Display(Name = "Produto")]
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public string ProdutoCodigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Descrição é obrigatória")]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; } = string.Empty;

        [Display(Name = "Unidade")]
        public string Unidade { get; set; } = "UN";

        [Required(ErrorMessage = "Quantidade é obrigatória")]
        [Display(Name = "Quantidade")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
        public decimal Quantidade { get; set; }

        [Required(ErrorMessage = "Valor unitário é obrigatório")]
        [Display(Name = "Valor Unitário")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor unitário deve ser maior que zero")]
        public decimal ValorUnitario { get; set; }

        [Display(Name = "Valor Total")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal ValorTotal { get; set; }

        [Display(Name = "Valor Desconto")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal ValorDesconto { get; set; }

        // Informações Fiscais
        [Display(Name = "CFOP")]
        public string CFOP { get; set; } = string.Empty;

        [Display(Name = "NCM")]
        public string NCM { get; set; } = string.Empty;

        [Display(Name = "CST")]
        public string CST { get; set; } = string.Empty;

        // ICMS
        [Display(Name = "Base Cálculo ICMS")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal BaseCalculoICMS { get; set; }

        [Display(Name = "Alíquota ICMS (%)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal AliquotaICMS { get; set; }

        [Display(Name = "Valor ICMS")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal ValorICMS { get; set; }

        // IPI
        [Display(Name = "Base Cálculo IPI")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal BaseCalculoIPI { get; set; }

        [Display(Name = "Alíquota IPI (%)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal AliquotaIPI { get; set; }

        [Display(Name = "Valor IPI")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal ValorIPI { get; set; }

        // PIS
        [Display(Name = "Base Cálculo PIS")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal BaseCalculoPIS { get; set; }

        [Display(Name = "Alíquota PIS (%)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal AliquotaPIS { get; set; }

        [Display(Name = "Valor PIS")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal ValorPIS { get; set; }

        // COFINS
        [Display(Name = "Base Cálculo COFINS")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal BaseCalculoCOFINS { get; set; }

        [Display(Name = "Alíquota COFINS (%)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal AliquotaCOFINS { get; set; }

        [Display(Name = "Valor COFINS")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal ValorCOFINS { get; set; }
    }

    public class NotaFiscalFiltroViewModel
    {
        [Display(Name = "Número")]
        public string? Numero { get; set; }

        [Display(Name = "Tipo")]
        public string? Tipo { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Cliente/Fornecedor")]
        public string? Destinatario { get; set; }

        [Display(Name = "Data Inicial")]
        [DataType(DataType.Date)]
        public DateTime? DataInicial { get; set; }

        [Display(Name = "Data Final")]
        [DataType(DataType.Date)]
        public DateTime? DataFinal { get; set; }

        [Display(Name = "Chave de Acesso")]
        public string? ChaveAcesso { get; set; }
    }
}