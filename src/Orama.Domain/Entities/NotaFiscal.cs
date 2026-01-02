using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities
{
    public class NotaFiscal
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Numero { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Serie { get; set; } = "1";

        [Required]
        [StringLength(20)]
        public string Tipo { get; set; } = string.Empty; // Entrada, Saida

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Rascunho"; // Rascunho, Autorizada, Cancelada

        public DateTime DataEmissao { get; set; }
        public DateTime? DataSaida { get; set; }

        // Relacionamentos
        public int EmpresaId { get; set; }
        public virtual Empresa Empresa { get; set; } = null!;

        public int? ClienteId { get; set; }
        public virtual Cliente? Cliente { get; set; }

        public int? FornecedorId { get; set; }
        public virtual Fornecedor? Fornecedor { get; set; }

        public int? VendaId { get; set; }
        public virtual Venda? Venda { get; set; }

        public int? CompraId { get; set; }
        public virtual Compra? Compra { get; set; }

        // Valores
        public decimal ValorProdutos { get; set; }
        public decimal ValorFrete { get; set; }
        public decimal ValorSeguro { get; set; }
        public decimal ValorDesconto { get; set; }
        public decimal ValorOutrasDespesas { get; set; }
        public decimal ValorIPI { get; set; }
        public decimal ValorICMS { get; set; }
        public decimal ValorPIS { get; set; }
        public decimal ValorCOFINS { get; set; }
        public decimal ValorTotal { get; set; }

        // Informações Fiscais
        [StringLength(10)]
        public string NaturezaOperacao { get; set; } = string.Empty;

        [StringLength(4)]
        public string CFOP { get; set; } = string.Empty;

        [StringLength(100)]
        public string ChaveAcesso { get; set; } = string.Empty;

        [StringLength(20)]
        public string Protocolo { get; set; } = string.Empty;

        public DateTime? DataAutorizacao { get; set; }

        // Observações
        [StringLength(500)]
        public string? InformacaoComplementar { get; set; }

        [StringLength(500)]
        public string? ObservacaoFisco { get; set; }

        // Auditoria
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataAlteracao { get; set; }
        public int UsuarioCriacaoId { get; set; }
        public int? UsuarioAlteracaoId { get; set; }

        // Itens
        public virtual ICollection<NotaFiscalItem> Itens { get; set; } = new List<NotaFiscalItem>();

        // Métodos
        public void CalcularTotais()
        {
            ValorProdutos = Itens.Sum(i => i.ValorTotal);
            ValorIPI = Itens.Sum(i => i.ValorIPI);
            ValorICMS = Itens.Sum(i => i.ValorICMS);
            ValorPIS = Itens.Sum(i => i.ValorPIS);
            ValorCOFINS = Itens.Sum(i => i.ValorCOFINS);
            
            ValorTotal = ValorProdutos + ValorFrete + ValorSeguro + ValorOutrasDespesas + ValorIPI - ValorDesconto;
        }

        public bool PodeEditar()
        {
            return Status == "Rascunho";
        }

        public bool PodeCancelar()
        {
            return Status == "Autorizada";
        }

        public string ObterDestinatario()
        {
            if (Tipo == "Saida" && Cliente != null)
                return Cliente.Nome;
            
            if (Tipo == "Entrada" && Fornecedor != null)
                return Fornecedor.Nome;
            
            return "Não informado";
        }
    }

    public class NotaFiscalItem
    {
        public int Id { get; set; }

        public int NotaFiscalId { get; set; }
        public virtual NotaFiscal NotaFiscal { get; set; } = null!;

        public int ProdutoId { get; set; }
        public virtual Produto Produto { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Descricao { get; set; } = string.Empty;

        [StringLength(10)]
        public string Unidade { get; set; } = "UN";

        public decimal Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal ValorDesconto { get; set; }

        // Informações Fiscais
        [StringLength(4)]
        public string CFOP { get; set; } = string.Empty;

        [StringLength(10)]
        public string NCM { get; set; } = string.Empty;

        [StringLength(10)]
        public string CST { get; set; } = string.Empty;

        // ICMS
        public decimal BaseCalculoICMS { get; set; }
        public decimal AliquotaICMS { get; set; }
        public decimal ValorICMS { get; set; }

        // IPI
        public decimal BaseCalculoIPI { get; set; }
        public decimal AliquotaIPI { get; set; }
        public decimal ValorIPI { get; set; }

        // PIS
        public decimal BaseCalculoPIS { get; set; }
        public decimal AliquotaPIS { get; set; }
        public decimal ValorPIS { get; set; }

        // COFINS
        public decimal BaseCalculoCOFINS { get; set; }
        public decimal AliquotaCOFINS { get; set; }
        public decimal ValorCOFINS { get; set; }

        // Métodos
        public void CalcularValores()
        {
            ValorTotal = (Quantidade * ValorUnitario) - ValorDesconto;
            
            // Cálculo básico de impostos (simplificado)
            BaseCalculoICMS = ValorTotal;
            ValorICMS = BaseCalculoICMS * (AliquotaICMS / 100);

            BaseCalculoIPI = ValorTotal;
            ValorIPI = BaseCalculoIPI * (AliquotaIPI / 100);

            BaseCalculoPIS = ValorTotal;
            ValorPIS = BaseCalculoPIS * (AliquotaPIS / 100);

            BaseCalculoCOFINS = ValorTotal;
            ValorCOFINS = BaseCalculoCOFINS * (AliquotaCOFINS / 100);
        }
    }
}