using Orama.Domain.Entities;

namespace Orama.Application.Services;

public interface IContaReceberService
{
    Task<IEnumerable<ContaReceber>> ObterTodosAsync(int empresaId);
    Task<IEnumerable<ContaReceber>> ObterPorStatusAsync(int empresaId, StatusConta status);
    Task<IEnumerable<ContaReceber>> ObterVencidasAsync(int empresaId);
    Task<IEnumerable<ContaReceber>> ObterAVencerAsync(int empresaId, int dias = 7);
    Task<ContaReceber?> ObterPorIdAsync(int id, int empresaId);
    Task<ContaReceber> IncluirAsync(ContaReceber conta);
    Task<ContaReceber> AlterarAsync(ContaReceber conta);
    Task<bool> ExcluirAsync(int id, int empresaId);
    Task<ContaReceber> ReceberAsync(int id, int empresaId, decimal valorRecebido, int contaBancariaId, DateTime dataRecebimento);
    Task<decimal> ObterTotalAReceberAsync(int empresaId);
    Task<decimal> ObterTotalVencidoAsync(int empresaId);
    
    // Novos métodos para integração com vendas
    Task<IEnumerable<ContaReceber>> GerarContasDeVendaAsync(int vendaId, int empresaId);
    Task<ContaReceber> CalcularJurosMultaAsync(int contaId, int empresaId, DateTime dataCalculo);
    Task<IEnumerable<ContaReceber>> ObterPorClienteAsync(int clienteId, int empresaId);
    Task<IEnumerable<ContaReceber>> ObterPorPeriodoAsync(int empresaId, DateTime dataInicio, DateTime dataFim);
}
