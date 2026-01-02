using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities;

/// <summary>
/// Entidade que representa uma venda/pedido de venda
/// </summary>
public class Venda : BaseEntity
{
    public int EmpresaId { get; set; }
    public virtual Empresa? Empresa { get; set; }

    [StringLength(20)]
    public string Numero { get; set; } = string.Empty;
    
    // Propriedade para compatibilidade
    public string NumeroVenda => Numero;

    public int ClienteId { get; set; }
    public virtual Cliente Cliente { get; set; } = null!;

    public int? VendedorId { get; set; }
    public virtual Usuario? Vendedor { get; set; }

    public DateTime DataVenda { get; set; } = DateTime.Now;
    public DateTime? DataEntrega { get; set; }

    public StatusVenda Status { get; set; } = StatusVenda.Orcamento;

    // Valores
    public decimal SubTotal { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal PercentualDesconto { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal ValorTotal { get; set; }

    // Controle de Margem e Lucro
    /// <summary>
    /// Custo total dos produtos vendidos
    /// Calculado automaticamente ao faturar a venda
    /// </summary>
    [Display(Name = "Custo Total")]
    public decimal CustoTotal { get; set; } = 0;

    /// <summary>
    /// Lucro total da venda (ValorTotal - CustoTotal)
    /// Calculado automaticamente ao faturar a venda
    /// </summary>
    [Display(Name = "Lucro Total")]
    public decimal LucroTotal { get; set; } = 0;

    /// <summary>
    /// Margem percentual da venda (LucroTotal / ValorTotal) × 100
    /// Calculado automaticamente ao faturar a venda
    /// </summary>
    [Display(Name = "Margem %")]
    public decimal MargemPercentual { get; set; } = 0;

    /// <summary>
    /// Data em que a margem foi calculada
    /// Registra quando o cálculo foi realizado
    /// </summary>
    [Display(Name = "Data Cálculo Margem")]
    public DateTime? DataMargemCalculada { get; set; }

    // Pagamento
    public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.Dinheiro;
    public int Parcelas { get; set; } = 1;

    [StringLength(500)]
    public string? Observacoes { get; set; }

    // Relacionamentos
    public virtual ICollection<VendaItem> Itens { get; set; } = new List<VendaItem>();
    public virtual ICollection<ContaReceber> ContasReceber { get; set; } = new List<ContaReceber>();

    // Métodos de Negócio - Domain Rico
    
    /// <summary>
    /// Calcula todos os totais da venda baseado nos itens
    /// </summary>
    public void CalcularTotais()
    {
        if (!Itens.Any())
        {
            SubTotal = ValorDesconto = ValorTotal = 0;
            return;
        }

        // Calcular subtotal dos itens
        SubTotal = Itens.Sum(i => i.Quantidade * i.PrecoUnitario);
        
        // Aplicar desconto percentual se informado
        if (PercentualDesconto > 0)
        {
            ValorDesconto = SubTotal * (PercentualDesconto / 100);
        }
        
        // Calcular total final
        ValorTotal = SubTotal - ValorDesconto + ValorFrete;
        
        // Recalcular totais dos itens
        foreach (var item in Itens)
        {
            item.CalcularTotal();
        }
    }

    /// <summary>
    /// Aprova a venda, mudando status de Orçamento para Aprovado
    /// </summary>
    public void Aprovar()
    {
        if (Status != StatusVenda.Orcamento)
            throw new InvalidOperationException("Apenas orçamentos podem ser aprovados");
            
        if (!Itens.Any())
            throw new InvalidOperationException("Não é possível aprovar venda sem itens");
            
        if (ValorTotal <= 0)
            throw new InvalidOperationException("Não é possível aprovar venda com valor zero");

        Status = StatusVenda.Aprovado;
        DataAtualizacao = DateTime.Now;
    }

    /// <summary>
    /// Fatura a venda, mudando status para Faturada
    /// </summary>
    public void Faturar()
    {
        if (Status != StatusVenda.Aprovado)
            throw new InvalidOperationException("Apenas vendas aprovadas podem ser faturadas");

        Status = StatusVenda.Faturada;
        DataAtualizacao = DateTime.Now;
    }

    /// <summary>
    /// Calcula margem e lucro da venda baseado no custo dos produtos
    /// 
    /// Fórmulas:
    /// - Lucro = ValorTotal - CustoTotal
    /// - Margem (%) = (Lucro / ValorTotal) × 100
    /// 
    /// Deve ser chamado após o faturamento, quando os custos estão definidos
    /// </summary>
    public void CalcularMargemELucro(IEnumerable<Produto> produtos)
    {
        if (Status != StatusVenda.Faturada)
            throw new InvalidOperationException("Margem só pode ser calculada para vendas faturadas");

        if (!Itens.Any())
            throw new InvalidOperationException("Não é possível calcular margem sem itens");

        decimal custoTotalCalculado = 0;

        foreach (var item in Itens)
        {
            var produto = produtos.FirstOrDefault(p => p.Id == item.ProdutoId);
            if (produto == null)
                throw new InvalidOperationException($"Produto ID {item.ProdutoId} não encontrado");

            // Determinar custo baseado no tipo do produto
            decimal custoUnitario = ObterCustoUnitarioProduto(produto);
            
            // Calcular custo total do item
            decimal custoItem = item.Quantidade * custoUnitario;
            custoTotalCalculado += custoItem;

            // Atualizar custo no item para auditoria
            item.CustoUnitario = custoUnitario;
            item.CustoTotal = custoItem;
        }

        // Registrar custos e calcular lucro/margem
        CustoTotal = custoTotalCalculado;
        LucroTotal = ValorTotal - CustoTotal;
        
        // Calcular margem percentual (evitar divisão por zero)
        MargemPercentual = ValorTotal > 0 ? (LucroTotal / ValorTotal) * 100 : 0;
        
        DataMargemCalculada = DateTime.Now;
    }

    /// <summary>
    /// Determina o custo unitário do produto baseado no seu tipo
    /// 
    /// Regras:
    /// - Produto Acabado (produzido): usar custo de produção mais recente
    /// - Outros produtos: usar custo médio do estoque (PrecoCusto)
    /// </summary>
    private decimal ObterCustoUnitarioProduto(Produto produto)
    {
        // Para produtos produzidos, idealmente usaríamos o custo da ordem de produção
        // Por simplicidade, vamos usar o PrecoCusto que já é atualizado pela produção
        return produto.PrecoCusto;
    }

    /// <summary>
    /// Cancela a venda
    /// </summary>
    public void Cancelar(string motivo = "")
    {
        if (Status == StatusVenda.Entregue)
            throw new InvalidOperationException("Não é possível cancelar venda já entregue");

        Status = StatusVenda.Cancelado;
        if (!string.IsNullOrEmpty(motivo))
        {
            Observacoes = string.IsNullOrEmpty(Observacoes) 
                ? $"Cancelado: {motivo}" 
                : $"{Observacoes}\nCancelado: {motivo}";
        }
        DataAtualizacao = DateTime.Now;
    }

    /// <summary>
    /// Marca a venda como entregue
    /// </summary>
    public void MarcarComoEntregue()
    {
        if (Status != StatusVenda.Faturada)
            throw new InvalidOperationException("Apenas vendas faturadas podem ser marcadas como entregues");

        Status = StatusVenda.Entregue;
        DataEntrega = DateTime.Now;
        DataAtualizacao = DateTime.Now;
    }

    /// <summary>
    /// Adiciona um item à venda
    /// </summary>
    public void AdicionarItem(int produtoId, decimal quantidade, decimal precoUnitario, decimal percentualDesconto = 0)
    {
        if (Status != StatusVenda.Orcamento)
            throw new InvalidOperationException("Apenas orçamentos podem ter itens adicionados");

        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero");

        if (precoUnitario <= 0)
            throw new ArgumentException("Preço unitário deve ser maior que zero");

        var item = new VendaItem
        {
            VendaId = Id,
            ProdutoId = produtoId,
            Quantidade = quantidade,
            PrecoUnitario = precoUnitario,
            PercentualDesconto = percentualDesconto,
            DataCriacao = DateTime.Now,
            Ativo = true
        };

        item.CalcularTotal();
        Itens.Add(item);
        CalcularTotais();
    }

    /// <summary>
    /// Remove um item da venda
    /// </summary>
    public void RemoverItem(int itemId)
    {
        if (Status != StatusVenda.Orcamento)
            throw new InvalidOperationException("Apenas orçamentos podem ter itens removidos");

        var item = Itens.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            Itens.Remove(item);
            CalcularTotais();
        }
    }

    /// <summary>
    /// Verifica se a venda pode ser editada
    /// </summary>
    public bool PodeSerEditada => Status == StatusVenda.Orcamento;

    /// <summary>
    /// Verifica se a venda pode ser aprovada
    /// </summary>
    public bool PodeSerAprovada => Status == StatusVenda.Orcamento && Itens.Any() && ValorTotal > 0;

    /// <summary>
    /// Verifica se a venda pode ser faturada
    /// </summary>
    public bool PodeSerFaturada => Status == StatusVenda.Aprovado;

    /// <summary>
    /// Verifica se a venda pode ser cancelada
    /// </summary>
    public bool PodeSerCancelada => Status != StatusVenda.Entregue && Status != StatusVenda.Cancelado;

    /// <summary>
    /// Indica se a margem e lucro já foram calculados
    /// </summary>
    public bool MargemCalculada => DataMargemCalculada.HasValue;

    /// <summary>
    /// Indica se a venda está dando lucro (margem positiva)
    /// </summary>
    public bool EstaDandoLucro => LucroTotal > 0;

    /// <summary>
    /// Indica se a venda está sendo vendida abaixo do custo
    /// </summary>
    public bool VendaAbaixoDoCusto => MargemCalculada && LucroTotal < 0;
}

/// <summary>
/// Item de uma venda
/// </summary>
public class VendaItem : BaseEntity
{
    public int VendaId { get; set; }
    public virtual Venda Venda { get; set; } = null!;

    public int ProdutoId { get; set; }
    public virtual Produto Produto { get; set; } = null!;

    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    
    // Propriedade para compatibilidade
    public decimal ValorUnitario => PrecoUnitario;
    public decimal PercentualDesconto { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }

    // Controle de Custo e Margem por Item
    /// <summary>
    /// Custo unitário do produto no momento da venda
    /// Preenchido automaticamente ao calcular margem
    /// </summary>
    [Display(Name = "Custo Unitário")]
    public decimal CustoUnitario { get; set; } = 0;

    /// <summary>
    /// Custo total do item (CustoUnitario × Quantidade)
    /// Calculado automaticamente ao calcular margem
    /// </summary>
    [Display(Name = "Custo Total")]
    public decimal CustoTotal { get; set; } = 0;

    /// <summary>
    /// Lucro do item (ValorTotal - CustoTotal)
    /// </summary>
    public decimal LucroItem => ValorTotal - CustoTotal;

    /// <summary>
    /// Margem percentual do item
    /// </summary>
    public decimal MargemItem => ValorTotal > 0 ? (LucroItem / ValorTotal) * 100 : 0;

    [StringLength(200)]
    public string? Observacoes { get; set; }

    // Métodos de Negócio
    
    /// <summary>
    /// Calcula o total do item baseado na quantidade, preço e desconto
    /// </summary>
    public void CalcularTotal()
    {
        var subtotal = Quantidade * PrecoUnitario;
        ValorDesconto = subtotal * (PercentualDesconto / 100);
        ValorTotal = subtotal - ValorDesconto;
    }

    /// <summary>
    /// Atualiza a quantidade do item
    /// </summary>
    public void AtualizarQuantidade(decimal novaQuantidade)
    {
        if (novaQuantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero");

        Quantidade = novaQuantidade;
        CalcularTotal();
        DataAtualizacao = DateTime.Now;
    }

    /// <summary>
    /// Atualiza o preço unitário do item
    /// </summary>
    public void AtualizarPreco(decimal novoPreco)
    {
        if (novoPreco <= 0)
            throw new ArgumentException("Preço deve ser maior que zero");

        PrecoUnitario = novoPreco;
        CalcularTotal();
        DataAtualizacao = DateTime.Now;
    }

    /// <summary>
    /// Aplica desconto percentual ao item
    /// </summary>
    public void AplicarDesconto(decimal percentual)
    {
        if (percentual < 0 || percentual > 100)
            throw new ArgumentException("Percentual de desconto deve estar entre 0 e 100");

        PercentualDesconto = percentual;
        CalcularTotal();
        DataAtualizacao = DateTime.Now;
    }
}

public enum StatusVenda
{
    Orcamento = 1,
    Aprovado = 2,
    Faturada = 3,
    Entregue = 4,
    Cancelado = 5
}

public enum FormaPagamento
{
    Dinheiro = 1,
    CartaoCredito = 2,
    CartaoDebito = 3,
    Pix = 4,
    Boleto = 5,
    Transferencia = 6,
    Cheque = 7
}
