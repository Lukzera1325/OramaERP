using Orama.Domain.Entities;

namespace Orama.Domain.Services;

/// <summary>
/// Domain Service para processamento de vendas
/// Contém lógica de negócio complexa que envolve múltiplas entidades
/// </summary>
public class VendaProcessingService
{
    /// <summary>
    /// Processa o faturamento de uma venda
    /// Valida regras de negócio e prepara dados para persistência
    /// </summary>
    public VendaFaturamentoResult ProcessarFaturamento(Venda venda, IEnumerable<Produto> produtos)
    {
        // Validar se pode ser faturada
        if (!venda.PodeSerFaturada)
            throw new InvalidOperationException("Venda não pode ser faturada no status atual");

        // Validar estoque disponível
        var itensComEstoqueInsuficiente = new List<string>();
        
        foreach (var item in venda.Itens)
        {
            var produto = produtos.FirstOrDefault(p => p.Id == item.ProdutoId);
            if (produto == null)
                throw new InvalidOperationException($"Produto {item.ProdutoId} não encontrado");

            if (produto.ControlaEstoque && produto.EstoqueAtual < item.Quantidade)
            {
                itensComEstoqueInsuficiente.Add($"{produto.Descricao} (Disponível: {produto.EstoqueAtual}, Necessário: {item.Quantidade})");
            }
        }

        if (itensComEstoqueInsuficiente.Any())
        {
            throw new InvalidOperationException($"Estoque insuficiente para os itens: {string.Join(", ", itensComEstoqueInsuficiente)}");
        }

        // Faturar a venda
        venda.Faturar();

        // Preparar movimentações de estoque
        var movimentacoesEstoque = PrepararMovimentacoesEstoque(venda, produtos);

        // Preparar contas a receber
        var contasReceber = PrepararContasReceber(venda);

        return new VendaFaturamentoResult
        {
            VendaFaturada = venda,
            MovimentacoesEstoque = movimentacoesEstoque,
            ContasReceber = contasReceber
        };
    }

    /// <summary>
    /// Processa o cancelamento de uma venda
    /// </summary>
    public VendaCancelamentoResult ProcessarCancelamento(Venda venda, string motivo, bool estornarEstoque = true)
    {
        // Validar se pode ser cancelada
        if (!venda.PodeSerCancelada)
            throw new InvalidOperationException("Venda não pode ser cancelada no status atual");

        // Cancelar a venda
        venda.Cancelar(motivo);

        var result = new VendaCancelamentoResult
        {
            VendaCancelada = venda,
            MovimentacoesEstoque = new List<MovimentacaoEstoque>(),
            ContasReceberParaCancelar = new List<int>()
        };

        // Se estava faturada, preparar estorno de estoque
        if (venda.Status == StatusVenda.Faturada && estornarEstoque)
        {
            result.MovimentacoesEstoque = PrepararEstornoEstoque(venda);
        }

        // Se tinha contas a receber, marcar para cancelamento
        if (venda.ContasReceber.Any())
        {
            result.ContasReceberParaCancelar = venda.ContasReceber.Select(c => c.Id).ToList();
        }

        return result;
    }

    /// <summary>
    /// Valida se uma venda pode ser processada
    /// </summary>
    public VendaValidationResult ValidarVenda(Venda venda, IEnumerable<Produto> produtos)
    {
        var result = new VendaValidationResult { IsValid = true, Errors = new List<string>() };

        // Validar dados básicos
        if (venda.ClienteId <= 0)
            result.Errors.Add("Cliente é obrigatório");

        if (!venda.Itens.Any())
            result.Errors.Add("Venda deve ter pelo menos um item");

        if (venda.ValorTotal <= 0)
            result.Errors.Add("Valor total deve ser maior que zero");

        // Validar itens
        foreach (var item in venda.Itens)
        {
            var produto = produtos.FirstOrDefault(p => p.Id == item.ProdutoId);
            if (produto == null)
            {
                result.Errors.Add($"Produto {item.ProdutoId} não encontrado");
                continue;
            }

            if (item.Quantidade <= 0)
                result.Errors.Add($"Quantidade do produto {produto.Descricao} deve ser maior que zero");

            if (item.PrecoUnitario <= 0)
                result.Errors.Add($"Preço do produto {produto.Descricao} deve ser maior que zero");
        }

        result.IsValid = !result.Errors.Any();
        return result;
    }

    private List<MovimentacaoEstoque> PrepararMovimentacoesEstoque(Venda venda, IEnumerable<Produto> produtos)
    {
        var movimentacoes = new List<MovimentacaoEstoque>();

        foreach (var item in venda.Itens)
        {
            var produto = produtos.FirstOrDefault(p => p.Id == item.ProdutoId);
            if (produto?.ControlaEstoque == true)
            {
                movimentacoes.Add(new MovimentacaoEstoque
                {
                    EmpresaId = venda.EmpresaId,
                    ProdutoId = item.ProdutoId,
                    Tipo = TipoMovimentacaoEstoque.SaidaVenda,
                    Quantidade = item.Quantidade,
                    CustoUnitario = item.PrecoUnitario,
                    Motivo = $"Venda #{venda.Numero}",
                    Observacoes = $"Venda #{venda.Numero} - {produto.Descricao}",
                    DataMovimentacao = DateTime.Now,
                    DataCriacao = DateTime.Now,
                    Ativo = true
                });
            }
        }

        return movimentacoes;
    }

    private List<MovimentacaoEstoque> PrepararEstornoEstoque(Venda venda)
    {
        var movimentacoes = new List<MovimentacaoEstoque>();

        foreach (var item in venda.Itens)
        {
            movimentacoes.Add(new MovimentacaoEstoque
            {
                EmpresaId = venda.EmpresaId,
                ProdutoId = item.ProdutoId,
                Tipo = TipoMovimentacaoEstoque.EntradaCompra,
                Quantidade = item.Quantidade,
                CustoUnitario = item.PrecoUnitario,
                Motivo = $"Estorno venda #{venda.Numero}",
                Observacoes = $"Estorno venda #{venda.Numero} - Cancelamento",
                DataMovimentacao = DateTime.Now,
                DataCriacao = DateTime.Now,
                Ativo = true
            });
        }

        return movimentacoes;
    }

    private List<ContaReceber> PrepararContasReceber(Venda venda)
    {
        var contas = new List<ContaReceber>();
        var valorParcela = venda.ValorTotal / venda.Parcelas;

        for (int i = 1; i <= venda.Parcelas; i++)
        {
            var dataVencimento = CalcularDataVencimento(venda.DataVenda, venda.FormaPagamento, i);

            contas.Add(new ContaReceber
            {
                EmpresaId = venda.EmpresaId,
                ClienteId = venda.ClienteId,
                VendaId = venda.Id,
                NumeroDocumento = $"{venda.Numero}/{i:D2}",
                Descricao = $"Venda #{venda.Numero} - Parcela {i}/{venda.Parcelas}",
                DataEmissao = venda.DataVenda,
                DataVencimento = dataVencimento,
                ValorOriginal = valorParcela,
                Status = StatusConta.Aberta,
                FormaPagamento = venda.FormaPagamento,
                DataCriacao = DateTime.Now,
                Ativo = true
            });
        }

        return contas;
    }

    private DateTime CalcularDataVencimento(DateTime dataBase, FormaPagamento formaPagamento, int numeroParcela)
    {
        return formaPagamento switch
        {
            FormaPagamento.Dinheiro => dataBase, // À vista
            FormaPagamento.Pix => dataBase, // À vista
            FormaPagamento.CartaoDebito => dataBase, // À vista
            FormaPagamento.CartaoCredito => dataBase.AddDays(30 * numeroParcela), // 30 dias por parcela
            FormaPagamento.Boleto => dataBase.AddDays(30 * numeroParcela), // 30 dias por parcela
            FormaPagamento.Transferencia => dataBase.AddDays(1), // 1 dia útil
            FormaPagamento.Cheque => dataBase.AddDays(30 * numeroParcela), // 30 dias por parcela
            _ => dataBase.AddDays(30 * numeroParcela)
        };
    }
}

// Result Objects para operações complexas

public class VendaFaturamentoResult
{
    public Venda VendaFaturada { get; set; } = null!;
    public List<MovimentacaoEstoque> MovimentacoesEstoque { get; set; } = new();
    public List<ContaReceber> ContasReceber { get; set; } = new();
}

public class VendaCancelamentoResult
{
    public Venda VendaCancelada { get; set; } = null!;
    public List<MovimentacaoEstoque> MovimentacoesEstoque { get; set; } = new();
    public List<int> ContasReceberParaCancelar { get; set; } = new();
}

public class VendaValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}