using Orama.Domain.Entities;

namespace Orama.Application.Services;

public interface IContaPagarService
{
    Task<IEnumerable<ContaPagar>> ObterTodosAsync(int empresaId);
    Task<IEnumerable<ContaPagar>> ObterPorStatusAsync(int empresaId, StatusConta status);
    Task<IEnumerable<ContaPagar>> ObterVencidasAsync(int empresaId);
    Task<IEnumerable<ContaPagar>> ObterAVencerAsync(int empresaId, int dias = 7);
    Task<ContaPagar?> ObterPorIdAsync(int id, int empresaId);
    Task<ContaPagar> IncluirAsync(ContaPagar conta);
    Task<ContaPagar> AlterarAsync(ContaPagar conta);
    Task<bool> ExcluirAsync(int id, int empresaId);
    Task<ContaPagar> PagarAsync(int id, int empresaId, decimal valorPago, int contaBancariaId, DateTime dataPagamento);
    Task<decimal> ObterTotalAPagarAsync(int empresaId);
    Task<decimal> ObterTotalVencidoAsync(int empresaId);
    
    // Novos métodos para integração com compras e funcionalidades avançadas
    Task<IEnumerable<ContaPagar>> GerarContasDeCompraAsync(int compraId, int empresaId);
    Task<ContaPagar> AgendarPagamentoAsync(int contaId, int empresaId, DateTime dataAgendamento, int contaBancariaId);
    Task<ContaPagar> AprovarContaAsync(int contaId, int empresaId, int usuarioAprovadorId);
    Task<ContaPagar> ReprovarContaAsync(int contaId, int empresaId, int usuarioAprovadorId, string motivo);
    Task<IEnumerable<ContaPagar>> ObterContasParaAprovacaoAsync(int empresaId);
    Task<IEnumerable<ContaPagar>> ObterPorFornecedorAsync(int fornecedorId, int empresaId);
    Task<IEnumerable<ContaPagar>> ObterPorPeriodoAsync(int empresaId, DateTime dataInicio, DateTime dataFim);
    Task<IEnumerable<ContaPagar>> ObterAgendadasAsync(int empresaId, DateTime data);
}
