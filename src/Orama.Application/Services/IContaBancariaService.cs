using Orama.Domain.Entities;

namespace Orama.Application.Services;

public interface IContaBancariaService
{
    Task<IEnumerable<ContaBancaria>> ObterTodosAsync(int empresaId);
    Task<ContaBancaria?> ObterPorIdAsync(int id, int empresaId);
    Task<ContaBancaria> IncluirAsync(ContaBancaria conta);
    Task<ContaBancaria> AlterarAsync(ContaBancaria conta);
    Task<bool> ExcluirAsync(int id, int empresaId);
    Task<decimal> ObterSaldoTotalAsync(int empresaId);
    
    // Novos métodos para movimentações e conciliação
    Task<MovimentacaoFinanceira> RegistrarMovimentacaoAsync(MovimentacaoFinanceira movimentacao);
    Task<IEnumerable<MovimentacaoFinanceira>> ObterMovimentacoesAsync(int contaBancariaId, int empresaId, DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<ContaBancaria> AtualizarSaldoAsync(int contaBancariaId, int empresaId);
    Task<decimal> CalcularSaldoRealAsync(int contaBancariaId, int empresaId);
    Task<IEnumerable<MovimentacaoFinanceira>> ObterMovimentacoesPendentesAsync(int contaBancariaId, int empresaId);
    Task<MovimentacaoFinanceira> ConciliarMovimentacaoAsync(int movimentacaoId, int empresaId, bool conciliada);
    Task<ContaBancaria> TransferirEntreContasAsync(int contaOrigemId, int contaDestinoId, decimal valor, string descricao, int empresaId);
}
