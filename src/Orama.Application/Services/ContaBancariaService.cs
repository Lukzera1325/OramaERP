using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

public class ContaBancariaService : IContaBancariaService
{
    private readonly OramaDbContext _context;

    public ContaBancariaService(OramaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ContaBancaria>> ObterTodosAsync(int empresaId)
    {
        return await _context.ContasBancarias
            .Where(c => c.EmpresaId == empresaId && c.Ativo)
            .OrderByDescending(c => c.ContaPadrao)
            .ThenBy(c => c.Descricao)
            .ToListAsync();
    }

    public async Task<ContaBancaria?> ObterPorIdAsync(int id, int empresaId)
    {
        return await _context.ContasBancarias
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId && c.Ativo);
    }

    public async Task<ContaBancaria> IncluirAsync(ContaBancaria conta)
    {
        // Se for conta padrão, desmarcar outras
        if (conta.ContaPadrao)
        {
            var outras = await _context.ContasBancarias
                .Where(c => c.EmpresaId == conta.EmpresaId && c.ContaPadrao)
                .ToListAsync();
            foreach (var c in outras) c.ContaPadrao = false;
        }

        conta.SaldoAtual = conta.SaldoInicial;
        _context.ContasBancarias.Add(conta);
        await _context.SaveChangesAsync();
        return conta;
    }

    public async Task<ContaBancaria> AlterarAsync(ContaBancaria conta)
    {
        if (conta.ContaPadrao)
        {
            var outras = await _context.ContasBancarias
                .Where(c => c.EmpresaId == conta.EmpresaId && c.ContaPadrao && c.Id != conta.Id)
                .ToListAsync();
            foreach (var c in outras) c.ContaPadrao = false;
        }

        _context.ContasBancarias.Update(conta);
        await _context.SaveChangesAsync();
        return conta;
    }

    public async Task<bool> ExcluirAsync(int id, int empresaId)
    {
        var conta = await ObterPorIdAsync(id, empresaId);
        if (conta == null) return false;

        // Verificar se tem movimentações
        var temMovimentacoes = await _context.MovimentacoesFinanceiras
            .AnyAsync(m => m.ContaBancariaId == id);
        if (temMovimentacoes)
            throw new InvalidOperationException("Não é possível excluir conta com movimentações.");

        conta.Ativo = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> ObterSaldoTotalAsync(int empresaId)
    {
        var contas = await _context.ContasBancarias
            .Where(c => c.EmpresaId == empresaId && c.Ativo)
            .ToListAsync();
        
        return contas.Sum(c => c.SaldoAtual);
    }

    /// <summary>
    /// Registra uma movimentação financeira
    /// </summary>
    public async Task<MovimentacaoFinanceira> RegistrarMovimentacaoAsync(MovimentacaoFinanceira movimentacao)
    {
        var conta = await ObterPorIdAsync(movimentacao.ContaBancariaId, movimentacao.EmpresaId);
        if (conta == null)
            throw new InvalidOperationException("Conta bancária não encontrada.");

        // Calcular saldos
        movimentacao.SaldoAnterior = conta.SaldoAtual;
        
        if (movimentacao.Tipo == TipoMovimentacao.Entrada)
            conta.SaldoAtual += movimentacao.Valor;
        else
            conta.SaldoAtual -= movimentacao.Valor;

        movimentacao.SaldoPosterior = conta.SaldoAtual;
        movimentacao.Conciliada = false; // Por padrão não conciliada

        _context.MovimentacoesFinanceiras.Add(movimentacao);
        await _context.SaveChangesAsync();

        return movimentacao;
    }

    /// <summary>
    /// Obtém movimentações de uma conta por período
    /// </summary>
    public async Task<IEnumerable<MovimentacaoFinanceira>> ObterMovimentacoesAsync(int contaBancariaId, int empresaId, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var query = _context.MovimentacoesFinanceiras
            .Where(m => m.ContaBancariaId == contaBancariaId && m.EmpresaId == empresaId);

        if (dataInicio.HasValue)
            query = query.Where(m => m.DataMovimentacao >= dataInicio.Value);

        if (dataFim.HasValue)
            query = query.Where(m => m.DataMovimentacao <= dataFim.Value);

        return await query
            .OrderByDescending(m => m.DataMovimentacao)
            .ThenByDescending(m => m.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Atualiza o saldo da conta baseado nas movimentações
    /// </summary>
    public async Task<ContaBancaria> AtualizarSaldoAsync(int contaBancariaId, int empresaId)
    {
        var conta = await ObterPorIdAsync(contaBancariaId, empresaId);
        if (conta == null)
            throw new InvalidOperationException("Conta bancária não encontrada.");

        var saldoCalculado = await CalcularSaldoRealAsync(contaBancariaId, empresaId);
        conta.SaldoAtual = saldoCalculado;

        await _context.SaveChangesAsync();
        return conta;
    }

    /// <summary>
    /// Calcula o saldo real baseado no saldo inicial + movimentações
    /// </summary>
    public async Task<decimal> CalcularSaldoRealAsync(int contaBancariaId, int empresaId)
    {
        var conta = await ObterPorIdAsync(contaBancariaId, empresaId);
        if (conta == null)
            return 0;

        var movimentacoesEntrada = await _context.MovimentacoesFinanceiras
            .Where(m => m.ContaBancariaId == contaBancariaId && m.EmpresaId == empresaId && m.Tipo == TipoMovimentacao.Entrada)
            .ToListAsync();

        var movimentacoesSaida = await _context.MovimentacoesFinanceiras
            .Where(m => m.ContaBancariaId == contaBancariaId && m.EmpresaId == empresaId && m.Tipo == TipoMovimentacao.Saida)
            .ToListAsync();

        var totalEntradas = movimentacoesEntrada.Sum(m => m.Valor);
        var totalSaidas = movimentacoesSaida.Sum(m => m.Valor);

        return conta.SaldoInicial + totalEntradas - totalSaidas;
    }

    /// <summary>
    /// Obtém movimentações pendentes de conciliação
    /// </summary>
    public async Task<IEnumerable<MovimentacaoFinanceira>> ObterMovimentacoesPendentesAsync(int contaBancariaId, int empresaId)
    {
        return await _context.MovimentacoesFinanceiras
            .Where(m => m.ContaBancariaId == contaBancariaId && m.EmpresaId == empresaId && !m.Conciliada)
            .OrderByDescending(m => m.DataMovimentacao)
            .ToListAsync();
    }

    /// <summary>
    /// Marca uma movimentação como conciliada ou não
    /// </summary>
    public async Task<MovimentacaoFinanceira> ConciliarMovimentacaoAsync(int movimentacaoId, int empresaId, bool conciliada)
    {
        var movimentacao = await _context.MovimentacoesFinanceiras
            .FirstOrDefaultAsync(m => m.Id == movimentacaoId && m.EmpresaId == empresaId);

        if (movimentacao == null)
            throw new InvalidOperationException("Movimentação não encontrada.");

        movimentacao.Conciliada = conciliada;
        movimentacao.DataConciliacao = conciliada ? DateTime.Now : null;

        await _context.SaveChangesAsync();
        return movimentacao;
    }

    /// <summary>
    /// Transfere valor entre contas bancárias
    /// </summary>
    public async Task<ContaBancaria> TransferirEntreContasAsync(int contaOrigemId, int contaDestinoId, decimal valor, string descricao, int empresaId)
    {
        if (valor <= 0)
            throw new InvalidOperationException("Valor da transferência deve ser maior que zero.");

        var contaOrigem = await ObterPorIdAsync(contaOrigemId, empresaId);
        var contaDestino = await ObterPorIdAsync(contaDestinoId, empresaId);

        if (contaOrigem == null || contaDestino == null)
            throw new InvalidOperationException("Uma das contas não foi encontrada.");

        if (contaOrigem.SaldoAtual < valor)
            throw new InvalidOperationException("Saldo insuficiente na conta de origem.");

        // Registrar saída na conta origem
        var movimentacaoSaida = new MovimentacaoFinanceira
        {
            EmpresaId = empresaId,
            ContaBancariaId = contaOrigemId,
            Tipo = TipoMovimentacao.Saida,
            Descricao = $"Transferência para {contaDestino.Descricao}: {descricao}",
            DataMovimentacao = DateTime.Now,
            Valor = valor,
            SaldoAnterior = contaOrigem.SaldoAtual,
            SaldoPosterior = contaOrigem.SaldoAtual - valor,
            Conciliada = true // Transferências internas são automaticamente conciliadas
        };

        // Registrar entrada na conta destino
        var movimentacaoEntrada = new MovimentacaoFinanceira
        {
            EmpresaId = empresaId,
            ContaBancariaId = contaDestinoId,
            Tipo = TipoMovimentacao.Entrada,
            Descricao = $"Transferência de {contaOrigem.Descricao}: {descricao}",
            DataMovimentacao = DateTime.Now,
            Valor = valor,
            SaldoAnterior = contaDestino.SaldoAtual,
            SaldoPosterior = contaDestino.SaldoAtual + valor,
            Conciliada = true
        };

        // Atualizar saldos
        contaOrigem.SaldoAtual -= valor;
        contaDestino.SaldoAtual += valor;

        _context.MovimentacoesFinanceiras.AddRange(movimentacaoSaida, movimentacaoEntrada);
        await _context.SaveChangesAsync();

        return contaOrigem;
    }
}
