using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

public class ContaPagarService : IContaPagarService
{
    private readonly OramaDbContext _context;

    public ContaPagarService(OramaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ContaPagar>> ObterTodosAsync(int empresaId)
    {
        return await _context.ContasPagar
            .Include(c => c.Fornecedor)
            .Where(c => c.EmpresaId == empresaId && c.Ativo)
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();
    }

    public async Task<IEnumerable<ContaPagar>> ObterPorStatusAsync(int empresaId, StatusConta status)
    {
        return await _context.ContasPagar
            .Include(c => c.Fornecedor)
            .Where(c => c.EmpresaId == empresaId && c.Status == status && c.Ativo)
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();
    }

    public async Task<IEnumerable<ContaPagar>> ObterVencidasAsync(int empresaId)
    {
        var hoje = DateTime.Today;
        return await _context.ContasPagar
            .Include(c => c.Fornecedor)
            .Where(c => c.EmpresaId == empresaId && c.Status == StatusConta.Aberta && c.DataVencimento < hoje && c.Ativo)
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();
    }

    public async Task<IEnumerable<ContaPagar>> ObterAVencerAsync(int empresaId, int dias = 7)
    {
        var hoje = DateTime.Today;
        var dataLimite = hoje.AddDays(dias);
        return await _context.ContasPagar
            .Include(c => c.Fornecedor)
            .Where(c => c.EmpresaId == empresaId && c.Status == StatusConta.Aberta && 
                        c.DataVencimento >= hoje && c.DataVencimento <= dataLimite && c.Ativo)
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();
    }

    public async Task<ContaPagar?> ObterPorIdAsync(int id, int empresaId)
    {
        return await _context.ContasPagar
            .Include(c => c.Fornecedor)
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId && c.Ativo);
    }

    public async Task<ContaPagar> IncluirAsync(ContaPagar conta)
    {
        _context.ContasPagar.Add(conta);
        await _context.SaveChangesAsync();
        return conta;
    }

    public async Task<ContaPagar> AlterarAsync(ContaPagar conta)
    {
        _context.ContasPagar.Update(conta);
        await _context.SaveChangesAsync();
        return conta;
    }

    public async Task<bool> ExcluirAsync(int id, int empresaId)
    {
        var conta = await ObterPorIdAsync(id, empresaId);
        if (conta == null) return false;

        if (conta.Status == StatusConta.Paga)
            throw new InvalidOperationException("Não é possível excluir conta já paga.");

        conta.Ativo = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ContaPagar> PagarAsync(int id, int empresaId, decimal valorPago, int contaBancariaId, DateTime dataPagamento)
    {
        var conta = await ObterPorIdAsync(id, empresaId);
        if (conta == null)
            throw new InvalidOperationException("Conta não encontrada.");

        conta.ValorPago += valorPago;
        conta.DataPagamento = dataPagamento;
        conta.ContaBancariaId = contaBancariaId;

        if (conta.ValorPago >= conta.ValorTotal)
            conta.Status = StatusConta.Paga;
        else if (conta.ValorPago > 0)
            conta.Status = StatusConta.Parcial;

        // Atualizar saldo da conta bancária
        var contaBancaria = await _context.ContasBancarias.FindAsync(contaBancariaId);
        if (contaBancaria != null)
        {
            contaBancaria.SaldoAtual -= valorPago;

            // Registrar movimentação
            var movimentacao = new MovimentacaoFinanceira
            {
                EmpresaId = empresaId,
                ContaBancariaId = contaBancariaId,
                Tipo = TipoMovimentacao.Saida,
                Descricao = $"Pagamento: {conta.Descricao}",
                DataMovimentacao = dataPagamento,
                Valor = valorPago,
                SaldoAnterior = contaBancaria.SaldoAtual + valorPago,
                SaldoPosterior = contaBancaria.SaldoAtual,
                ContaPagarId = conta.Id
            };
            _context.MovimentacoesFinanceiras.Add(movimentacao);
        }

        await _context.SaveChangesAsync();
        return conta;
    }

    public async Task<decimal> ObterTotalAPagarAsync(int empresaId)
    {
        var contas = await _context.ContasPagar
            .Where(c => c.EmpresaId == empresaId && c.Status != StatusConta.Paga && c.Status != StatusConta.Cancelada && c.Ativo)
            .ToListAsync();
        
        return contas.Sum(c => c.ValorTotal - c.ValorPago);
    }

    public async Task<decimal> ObterTotalVencidoAsync(int empresaId)
    {
        var hoje = DateTime.Today;
        var contas = await _context.ContasPagar
            .Where(c => c.EmpresaId == empresaId && c.Status == StatusConta.Aberta && c.DataVencimento < hoje && c.Ativo)
            .ToListAsync();
        
        return contas.Sum(c => c.ValorTotal - c.ValorPago);
    }

    /// <summary>
    /// Gera contas a pagar automaticamente a partir de uma compra
    /// </summary>
    public async Task<IEnumerable<ContaPagar>> GerarContasDeCompraAsync(int compraId, int empresaId)
    {
        var compra = await _context.Compras
            .Include(c => c.Fornecedor)
            .FirstOrDefaultAsync(c => c.Id == compraId && c.EmpresaId == empresaId);

        if (compra == null)
            throw new InvalidOperationException("Compra não encontrada.");

        if (compra.Status != StatusCompra.Recebida)
            throw new InvalidOperationException("Apenas compras recebidas podem gerar contas a pagar.");

        var contasExistentes = await _context.ContasPagar
            .Where(c => c.CompraId == compraId && c.EmpresaId == empresaId)
            .ToListAsync();

        if (contasExistentes.Any())
            throw new InvalidOperationException("Esta compra já possui contas a pagar geradas.");

        var contas = new List<ContaPagar>();

        // Gerar parcelas baseado na forma de pagamento
        var numeroParcelas = compra.FormaPagamento switch
        {
            FormaPagamento.Dinheiro => 1,
            FormaPagamento.CartaoCredito => compra.Parcelas,
            FormaPagamento.CartaoDebito => 1,
            FormaPagamento.Pix => 1,
            FormaPagamento.Boleto => compra.Parcelas,
            FormaPagamento.Transferencia => 1,
            FormaPagamento.Cheque => compra.Parcelas,
            _ => 1
        };

        var valorParcela = compra.ValorTotal / numeroParcelas;
        var dataVencimento = compra.DataCompra.AddDays(30); // Primeira parcela em 30 dias

        for (int i = 1; i <= numeroParcelas; i++)
        {
            var conta = new ContaPagar
            {
                EmpresaId = empresaId,
                FornecedorId = compra.FornecedorId,
                CompraId = compraId,
                NumeroDocumento = $"{compra.NumeroCompra}/{i:D2}",
                Descricao = $"Compra {compra.NumeroCompra} - Parcela {i}/{numeroParcelas}",
                DataEmissao = compra.DataCompra,
                DataVencimento = dataVencimento.AddMonths(i - 1),
                ValorOriginal = valorParcela,
                Status = StatusConta.Aberta,
                StatusAprovacao = StatusAprovacao.Pendente,
                Ativo = true
            };

            contas.Add(conta);
            _context.ContasPagar.Add(conta);
        }

        await _context.SaveChangesAsync();
        return contas;
    }

    /// <summary>
    /// Agenda um pagamento para data futura
    /// </summary>
    public async Task<ContaPagar> AgendarPagamentoAsync(int contaId, int empresaId, DateTime dataAgendamento, int contaBancariaId)
    {
        var conta = await ObterPorIdAsync(contaId, empresaId);
        if (conta == null)
            throw new InvalidOperationException("Conta não encontrada.");

        if (conta.Status != StatusConta.Aberta)
            throw new InvalidOperationException("Apenas contas em aberto podem ser agendadas.");

        if (conta.StatusAprovacao != StatusAprovacao.Aprovada)
            throw new InvalidOperationException("Apenas contas aprovadas podem ser agendadas.");

        conta.DataAgendamento = dataAgendamento;
        conta.ContaBancariaId = contaBancariaId;
        conta.Status = StatusConta.Agendada;

        await _context.SaveChangesAsync();
        return conta;
    }

    /// <summary>
    /// Aprova uma conta a pagar
    /// </summary>
    public async Task<ContaPagar> AprovarContaAsync(int contaId, int empresaId, int usuarioAprovadorId)
    {
        var conta = await ObterPorIdAsync(contaId, empresaId);
        if (conta == null)
            throw new InvalidOperationException("Conta não encontrada.");

        if (conta.StatusAprovacao != StatusAprovacao.Pendente)
            throw new InvalidOperationException("Apenas contas pendentes podem ser aprovadas.");

        conta.StatusAprovacao = StatusAprovacao.Aprovada;
        conta.UsuarioAprovadorId = usuarioAprovadorId;
        conta.DataAprovacao = DateTime.Now;

        await _context.SaveChangesAsync();
        return conta;
    }

    /// <summary>
    /// Reprova uma conta a pagar
    /// </summary>
    public async Task<ContaPagar> ReprovarContaAsync(int contaId, int empresaId, int usuarioAprovadorId, string motivo)
    {
        var conta = await ObterPorIdAsync(contaId, empresaId);
        if (conta == null)
            throw new InvalidOperationException("Conta não encontrada.");

        if (conta.StatusAprovacao != StatusAprovacao.Pendente)
            throw new InvalidOperationException("Apenas contas pendentes podem ser reprovadas.");

        conta.StatusAprovacao = StatusAprovacao.Reprovada;
        conta.UsuarioAprovadorId = usuarioAprovadorId;
        conta.DataAprovacao = DateTime.Now;
        conta.MotivoReprovacao = motivo;

        await _context.SaveChangesAsync();
        return conta;
    }

    /// <summary>
    /// Obtém contas pendentes de aprovação
    /// </summary>
    public async Task<IEnumerable<ContaPagar>> ObterContasParaAprovacaoAsync(int empresaId)
    {
        return await _context.ContasPagar
            .Include(c => c.Fornecedor)
            .Where(c => c.EmpresaId == empresaId && c.StatusAprovacao == StatusAprovacao.Pendente && c.Ativo)
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém contas a pagar de um fornecedor específico
    /// </summary>
    public async Task<IEnumerable<ContaPagar>> ObterPorFornecedorAsync(int fornecedorId, int empresaId)
    {
        return await _context.ContasPagar
            .Include(c => c.Fornecedor)
            .Where(c => c.FornecedorId == fornecedorId && c.EmpresaId == empresaId && c.Ativo)
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém contas a pagar por período
    /// </summary>
    public async Task<IEnumerable<ContaPagar>> ObterPorPeriodoAsync(int empresaId, DateTime dataInicio, DateTime dataFim)
    {
        return await _context.ContasPagar
            .Include(c => c.Fornecedor)
            .Where(c => c.EmpresaId == empresaId && 
                       c.DataVencimento >= dataInicio && 
                       c.DataVencimento <= dataFim && 
                       c.Ativo)
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém contas agendadas para uma data específica
    /// </summary>
    public async Task<IEnumerable<ContaPagar>> ObterAgendadasAsync(int empresaId, DateTime data)
    {
        return await _context.ContasPagar
            .Include(c => c.Fornecedor)
            .Where(c => c.EmpresaId == empresaId && 
                       c.Status == StatusConta.Agendada && 
                       c.DataAgendamento.HasValue &&
                       c.DataAgendamento.Value.Date == data.Date && 
                       c.Ativo)
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();
    }
}
