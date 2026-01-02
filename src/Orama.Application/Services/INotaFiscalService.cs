using Orama.Domain.Entities;

namespace Orama.Application.Services
{
    public interface INotaFiscalService
    {
        // CRUD Básico
        Task<IEnumerable<NotaFiscal>> ObterTodosAsync(int empresaId);
        Task<NotaFiscal?> ObterPorIdAsync(int id, int empresaId);
        Task<NotaFiscal> CriarAsync(NotaFiscal notaFiscal);
        Task<NotaFiscal> AtualizarAsync(NotaFiscal notaFiscal);
        Task<bool> ExcluirAsync(int id, int empresaId);

        // Operações Específicas
        Task<NotaFiscal> CriarDeVendaAsync(int vendaId, int empresaId);
        Task<NotaFiscal> CriarDeCompraAsync(int compraId, int empresaId);
        Task<string> GerarProximoNumeroAsync(string serie, string tipo, int empresaId);
        
        // Autorização/Cancelamento
        Task<bool> AutorizarAsync(int id, int empresaId);
        Task<bool> CancelarAsync(int id, string justificativa, int empresaId);
        
        // Consultas
        Task<IEnumerable<NotaFiscal>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, int empresaId);
        Task<IEnumerable<NotaFiscal>> ObterPorStatusAsync(string status, int empresaId);
        Task<IEnumerable<NotaFiscal>> ObterPorClienteAsync(int clienteId, int empresaId);
        Task<IEnumerable<NotaFiscal>> ObterPorFornecedorAsync(int fornecedorId, int empresaId);
        
        // Relatórios
        Task<decimal> ObterTotalVendasPeriodoAsync(DateTime dataInicio, DateTime dataFim, int empresaId);
        Task<decimal> ObterTotalComprasPeriodoAsync(DateTime dataInicio, DateTime dataFim, int empresaId);
        Task<IEnumerable<dynamic>> ObterResumoImpostosAsync(DateTime dataInicio, DateTime dataFim, int empresaId);
        
        // Validações
        Task<bool> ValidarNotaFiscalAsync(NotaFiscal notaFiscal);
        Task<List<string>> ObterErrosValidacaoAsync(NotaFiscal notaFiscal);
        
        // Utilitários
        Task<bool> ExisteNumeroAsync(string numero, string serie, string tipo, int empresaId, int? excludeId = null);
        Task<NotaFiscal?> ObterPorChaveAcessoAsync(string chaveAcesso, int empresaId);
    }
}