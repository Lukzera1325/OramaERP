using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

public class ContaReceberService : IContaReceberService
{
    private readonly OramaDbContext _context;

    public ContaReceberService(OramaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ContaReceber>> ObterTodosAsync(int empresaId)
    {
        return await _context.ContasReceber
            .Include(c => c.Cliente)
            .Where(c => c.EmpresaId == empresaId && c.Ativo)
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();
    }

    public async Task<IEnumerable<ContaReceber>> ObterPorStatusAsync(int empresaId, StatusConta status)
    {
        return await _context.ContasReceber
            .Include(c => c.Cliente)
            .Where(c => c.EmpresaId == empresaId && c.Status == status && c.Ativo)
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();
    }

    public async Task<IEnumerable<ContaReceber>> ObterVencidasAsync(int empresaId)
    {
        var hoje = DateTime.Today;
        return await _context.ContasReceber
            .Include(c => c.Cliente)
            .Where(c => c.EmpresaId == empresaId && c.Status == StatusConta.Aberta && c.DataVencimento < hoje && c.Ativo)
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();
    }

    public async Task<IEnumerable<ContaReceber>> ObterAVencerAsync(int empresaId, int dias = 7)
    {
        var hoje = DateTime.Today;
        var dataLimite = hoje.AddDays(dias);
        return await _context.ContasReceber
            .Include(c => c.Cliente)
            .Where(c => c.EmpresaId == empresaId && c.Status == StatusConta.Aberta && 
                        c.DataVencimento >= hoje && c.DataVencimento <= dataLimite && c.Ativo)
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();
    }

    public async Task<ContaReceber?> ObterPorIdAsync(int id, int empresaId)
    {
        return await _context.ContasReceber
            .Include(c => c.Cliente)
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId && c.Ativo);
    }

    public async Task<ContaReceber> IncluirAsync(ContaReceber conta)
    {
        _context.ContasReceber.Add(conta);
        await _context.SaveChangesAsync();
        return conta;
    }

    public async Task<ContaReceber> AlterarAsync(ContaReceber conta)
    {
        _context.ContasReceber.Update(conta);
        await _context.SaveChangesAsync();
        return conta;
    }

    public async Task<bool> ExcluirAsync(int id, int empresaId)
    {
        var conta = await ObterPorIdAsync(id, empresaId);
        if (conta == null) return false;

        if (conta.Status == StatusConta.Paga)
            throw new InvalidOperationException("Não é possível excluir conta já recebida.");

        conta.Ativo = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ContaReceber> ReceberAsync(int id, int empresaId, decimal valorRecebido, int contaBancariaId, DateTime dataRecebimento)
    {
        var conta = await ObterPorIdAsync(id, empresaId);
        if (conta == null)
            throw new InvalidOperationException("Conta não encontrada.");

        conta.ValorRecebido += valorRecebido;
        conta.DataRecebimento = dataRecebimento;
        conta.ContaBancariaId = contaBancariaId;

        if (conta.ValorRecebido >= conta.ValorTotal)
            conta.Status = StatusConta.Paga;
        else if (conta.ValorRecebido > 0)
            conta.Status = StatusConta.Parcial;

        // Atualizar saldo da conta bancária
        var contaBancaria = await _context.ContasBancarias.FindAsync(contaBancariaId);
        if (contaBancaria != null)
        {
            contaBancaria.SaldoAtual += valorRecebido;

            // Registrar movimentação
            var movimentacao = new MovimentacaoFinanceira
            {
                EmpresaId = empresaId,
                ContaBancariaId = contaBancariaId,
                Tipo = TipoMovimentacao.Entrada,
                Descricao = $"Recebimento: {conta.Descricao}",
                DataMovimentacao = dataRecebimento,
                Valor = valorRecebido,
                SaldoAnterior = contaBancaria.SaldoAtual - valorRecebido,
                SaldoPosterior = contaBancaria.SaldoAtual,
                ContaReceberId = conta.Id
            };
            _context.MovimentacoesFinanceiras.Add(movimentacao);
        }

        await _context.SaveChangesAsync();
        return conta;
    }

    public async Task<decimal> ObterTotalAReceberAsync(int empresaId)
    {
        var contas = await _context.ContasReceber
            .Where(c => c.EmpresaId == empresaId && c.Status != StatusConta.Paga && c.Status != StatusConta.Cancelada && c.Ativo)
            .ToListAsync();
        
        return contas.Sum(c => c.ValorTotal - c.ValorRecebido);
    }

    public async Task<decimal> ObterTotalVencidoAsync(int empresaId)
    {
        var hoje = DateTime.Today;
        var contas = await _context.ContasReceber
            .Where(c => c.EmpresaId == empresaId && c.Status == StatusConta.Aberta && c.DataVencimento < hoje && c.Ativo)
            .ToListAsync();
        
        return contas.Sum(c => c.ValorTotal - c.ValorRecebido);
    }

    /// <summary>
    /// Gera contas a receber automaticamente a partir de uma venda
    /// </summary>
    public async Task<IEnumerable<ContaReceber>> GerarContasDeVendaAsync(int vendaId, int empresaId)
    {
        var venda = await _context.Vendas
            .Include(v => v.Cliente)
            .FirstOrDefaultAsync(v => v.Id == vendaId && v.EmpresaId == empresaId);

        if (venda == null)
            throw new InvalidOperationException("Venda não encontrada.");

        if (venda.Status != StatusVenda.Faturada)
            throw new InvalidOperationException("Apenas vendas faturadas podem gerar contas a receber.");

        var contasExistentes = await _context.ContasReceber
            .Where(c => c.VendaId == vendaId && c.EmpresaId == empresaId)
            .ToListAsync();

        if (contasExistentes.Any())
            throw new InvalidOperationException("Esta venda já possui contas a receber geradas.");

        var contas = new List<ContaReceber>();

        // Gerar parcelas baseado na forma de pagamento
        var numeroParcelas = venda.FormaPagamento switch
        {
            FormaPagamento.Dinheiro => 1,
            FormaPagamento.CartaoCredito => venda.Parcelas,
            FormaPagamento.CartaoDebito => 1,
            FormaPagamento.Pix => 1,
            FormaPagamento.Boleto => venda.Parcelas,
            FormaPagamento.Transferencia => 1,
            FormaPagamento.Cheque => venda.Parcelas,
            _ => 1
        };

        var valorParcela = venda.ValorTotal / numeroParcelas;
        var dataVencimento = venda.DataVenda.AddDays(30); // Primeira parcela em 30 dias

        for (int i = 1; i <= numeroParcelas; i++)
        {
            var conta = new ContaReceber
            {
                EmpresaId = empresaId,
                ClienteId = venda.ClienteId,
                VendaId = vendaId,
                NumeroDocumento = $"{venda.NumeroVenda}/{i:D2}",
                Descricao = $"Venda {venda.NumeroVenda} - Parcela {i}/{numeroParcelas}",
                DataEmissao = venda.DataVenda,
                DataVencimento = dataVencimento.AddMonths(i - 1),
                ValorOriginal = valorParcela,
                Status = StatusConta.Aberta,
                Ativo = true
            };

            contas.Add(conta);
            _context.ContasReceber.Add(conta);
        }

        await _context.SaveChangesAsync();
        return contas;
    }

    /// <summary>
    /// Calcula juros e multa para conta vencida
    /// </summary>
    public async Task<ContaReceber> CalcularJurosMultaAsync(int contaId, int empresaId, DateTime dataCalculo)
    {
        var conta = await ObterPorIdAsync(contaId, empresaId);
        if (conta == null)
            throw new InvalidOperationException("Conta não encontrada.");

        if (conta.Status != StatusConta.Aberta)
            throw new InvalidOperationException("Apenas contas em aberto podem ter juros/multa calculados.");

        if (dataCalculo <= conta.DataVencimento)
            return conta; // Não há atraso

        var diasAtraso = (dataCalculo - conta.DataVencimento).Days;
        
        // Configurações padrão (podem vir de configuração da empresa)
        var percentualMulta = 2.0m; // 2% de multa
        var percentualJurosDia = 0.033m; // 0.033% ao dia (1% ao mês)

        // Calcular multa (apenas uma vez)
        if (conta.ValorMulta == 0)
        {
            conta.ValorMulta = conta.ValorOriginal * (percentualMulta / 100);
        }

        // Calcular juros proporcionais aos dias de atraso
        conta.ValorJuros = conta.ValorOriginal * (percentualJurosDia / 100) * diasAtraso;

        // ValorTotal é calculado automaticamente pela propriedade

        await _context.SaveChangesAsync();
        return conta;
    }

    /// <summary>
    /// Obtém contas a receber de um cliente específico
    /// </summary>
    public async Task<IEnumerable<ContaReceber>> ObterPorClienteAsync(int clienteId, int empresaId)
    {
        return await _context.ContasReceber
            .Include(c => c.Cliente)
            .Where(c => c.ClienteId == clienteId && c.EmpresaId == empresaId && c.Ativo)
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém contas a receber por período
    /// </summary>
    public async Task<IEnumerable<ContaReceber>> ObterPorPeriodoAsync(int empresaId, DateTime dataInicio, DateTime dataFim)
    {
        return await _context.ContasReceber
            .Include(c => c.Cliente)
            .Where(c => c.EmpresaId == empresaId && 
                       c.DataVencimento >= dataInicio && 
                       c.DataVencimento <= dataFim && 
                       c.Ativo)
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();
    }
}
