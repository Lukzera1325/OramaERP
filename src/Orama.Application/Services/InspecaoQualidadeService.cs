using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services
{
    public class InspecaoQualidadeService : IInspecaoQualidadeService
    {
        private readonly OramaDbContext _context;

        public InspecaoQualidadeService(OramaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InspecaoQualidade>> ObterTodosAsync(int empresaId)
        {
            return await _context.InspecoesQualidade
                .Include(i => i.OrdemProducao)
                .Include(i => i.Inspetor)
                .Where(i => i.OrdemProducao.EmpresaId == empresaId)
                .OrderByDescending(i => i.DataCriacao)
                .ToListAsync();
        }

        public async Task<InspecaoQualidade?> ObterPorIdAsync(int id, int empresaId)
        {
            return await _context.InspecoesQualidade
                .Include(i => i.OrdemProducao)
                .Include(i => i.Inspetor)
                .Where(i => i.Id == id && i.OrdemProducao.EmpresaId == empresaId)
                .FirstOrDefaultAsync();
        }

        public async Task<InspecaoQualidade> CriarAsync(InspecaoQualidade inspecao)
        {
            inspecao.DataCriacao = DateTime.Now;
            
            _context.InspecoesQualidade.Add(inspecao);
            await _context.SaveChangesAsync();
            
            return inspecao;
        }

        public async Task<InspecaoQualidade> AtualizarAsync(InspecaoQualidade inspecao)
        {
            var inspecaoExistente = await _context.InspecoesQualidade
                .Include(i => i.OrdemProducao)
                .FirstOrDefaultAsync(i => i.Id == inspecao.Id && i.OrdemProducao.EmpresaId == inspecao.OrdemProducao.EmpresaId);

            if (inspecaoExistente == null)
                throw new ArgumentException("Inspeção não encontrada");

            inspecaoExistente.Resultado = inspecao.Resultado;
            inspecaoExistente.QuantidadeInspecionada = inspecao.QuantidadeInspecionada;
            inspecaoExistente.QuantidadeAprovada = inspecao.QuantidadeAprovada;
            inspecaoExistente.QuantidadeRejeitada = inspecao.QuantidadeRejeitada;
            inspecaoExistente.Observacoes = inspecao.Observacoes;
            inspecaoExistente.AcaoCorretiva = inspecao.AcaoCorretiva;

            await _context.SaveChangesAsync();
            return inspecaoExistente;
        }

        public async Task<bool> ExcluirAsync(int id, int empresaId)
        {
            var inspecao = await _context.InspecoesQualidade
                .Include(i => i.OrdemProducao)
                .FirstOrDefaultAsync(i => i.Id == id && i.OrdemProducao.EmpresaId == empresaId);

            if (inspecao == null)
                return false;

            _context.InspecoesQualidade.Remove(inspecao);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<InspecaoQualidade>> ObterPorOrdemProducaoAsync(int ordemProducaoId, int empresaId)
        {
            return await _context.InspecoesQualidade
                .Include(i => i.Inspetor)
                .Where(i => i.OrdemProducaoId == ordemProducaoId && i.OrdemProducao.EmpresaId == empresaId)
                .OrderBy(i => i.DataInspecao)
                .ToListAsync();
        }

        public async Task<IEnumerable<InspecaoQualidade>> ObterPorTipoAsync(int empresaId, TipoInspecao tipo)
        {
            return await _context.InspecoesQualidade
                .Include(i => i.OrdemProducao)
                .Include(i => i.Inspetor)
                .Where(i => i.OrdemProducao.EmpresaId == empresaId && i.Tipo == tipo)
                .OrderByDescending(i => i.DataInspecao)
                .ToListAsync();
        }

        public async Task<IEnumerable<InspecaoQualidade>> ObterPorResultadoAsync(int empresaId, ResultadoInspecao resultado)
        {
            return await _context.InspecoesQualidade
                .Include(i => i.OrdemProducao)
                .Include(i => i.Inspetor)
                .Where(i => i.OrdemProducao.EmpresaId == empresaId && i.Resultado == resultado)
                .OrderByDescending(i => i.DataInspecao)
                .ToListAsync();
        }

        public async Task<IEnumerable<InspecaoQualidade>> ObterPorPeriodoAsync(int empresaId, DateTime dataInicio, DateTime dataFim)
        {
            return await _context.InspecoesQualidade
                .Include(i => i.OrdemProducao)
                .Include(i => i.Inspetor)
                .Where(i => i.OrdemProducao.EmpresaId == empresaId &&
                           i.DataInspecao >= dataInicio && i.DataInspecao <= dataFim)
                .OrderBy(i => i.DataInspecao)
                .ToListAsync();
        }

        public async Task<decimal> CalcularPercentualAprovacaoAsync(int empresaId, DateTime dataInicio, DateTime dataFim)
        {
            var inspecoes = await _context.InspecoesQualidade
                .Include(i => i.OrdemProducao)
                .Where(i => i.OrdemProducao.EmpresaId == empresaId &&
                           i.DataInspecao >= dataInicio && i.DataInspecao <= dataFim)
                .ToListAsync();

            if (!inspecoes.Any())
                return 0;

            var totalInspecionado = inspecoes.Sum(i => i.QuantidadeInspecionada);
            var totalAprovado = inspecoes.Sum(i => i.QuantidadeAprovada);

            return totalInspecionado > 0 ? (totalAprovado / totalInspecionado) * 100 : 0;
        }

        public async Task<decimal> CalcularPercentualRejeicaoAsync(int empresaId, DateTime dataInicio, DateTime dataFim)
        {
            var inspecoes = await _context.InspecoesQualidade
                .Include(i => i.OrdemProducao)
                .Where(i => i.OrdemProducao.EmpresaId == empresaId &&
                           i.DataInspecao >= dataInicio && i.DataInspecao <= dataFim)
                .ToListAsync();

            if (!inspecoes.Any())
                return 0;

            var totalInspecionado = inspecoes.Sum(i => i.QuantidadeInspecionada);
            var totalRejeitado = inspecoes.Sum(i => i.QuantidadeRejeitada);

            return totalInspecionado > 0 ? (totalRejeitado / totalInspecionado) * 100 : 0;
        }

        public async Task<IEnumerable<(string TipoInspecao, int Quantidade, decimal PercentualAprovacao)>> 
            ObterEstatisticasPorTipoAsync(int empresaId, DateTime dataInicio, DateTime dataFim)
        {
            var inspecoes = await _context.InspecoesQualidade
                .Include(i => i.OrdemProducao)
                .Where(i => i.OrdemProducao.EmpresaId == empresaId &&
                           i.DataInspecao >= dataInicio && i.DataInspecao <= dataFim)
                .ToListAsync();

            return inspecoes
                .GroupBy(i => i.Tipo)
                .Select(g => new
                {
                    Tipo = g.Key.ToString(),
                    Quantidade = g.Count(),
                    TotalInspecionado = g.Sum(i => i.QuantidadeInspecionada),
                    TotalAprovado = g.Sum(i => i.QuantidadeAprovada)
                })
                .Select(x => (
                    x.Tipo,
                    x.Quantidade,
                    x.TotalInspecionado > 0 ? (x.TotalAprovado / x.TotalInspecionado) * 100 : 0
                ))
                .ToList();
        }

        public async Task<int> ObterQuantidadeInspecoesPendentesAsync(int empresaId)
        {
            return await _context.InspecoesQualidade
                .Include(i => i.OrdemProducao)
                .CountAsync(i => i.OrdemProducao.EmpresaId == empresaId &&
                               i.Resultado == ResultadoInspecao.EmAnalise);
        }

        public async Task<decimal> ObterPercentualQualidadeMesAsync(int empresaId, int mes, int ano)
        {
            var inicioMes = new DateTime(ano, mes, 1);
            var fimMes = inicioMes.AddMonths(1).AddDays(-1);

            return await CalcularPercentualAprovacaoAsync(empresaId, inicioMes, fimMes);
        }

        public async Task<IEnumerable<(string Produto, int QuantidadeRejeitada)>> 
            ObterProdutosMaisRejeitadosAsync(int empresaId, DateTime dataInicio, DateTime dataFim, int top = 10)
        {
            var inspecoes = await _context.InspecoesQualidade
                .Include(i => i.OrdemProducao)
                .ThenInclude(o => o.Produto)
                .Where(i => i.OrdemProducao.EmpresaId == empresaId &&
                           i.DataInspecao >= dataInicio && i.DataInspecao <= dataFim &&
                           i.QuantidadeRejeitada > 0)
                .ToListAsync();

            return inspecoes
                .GroupBy(i => new { i.OrdemProducao.ProdutoId, i.OrdemProducao.Produto.Descricao })
                .Select(g => new
                {
                    Produto = g.Key.Descricao,
                    QuantidadeRejeitada = (int)g.Sum(i => i.QuantidadeRejeitada)
                })
                .OrderByDescending(x => x.QuantidadeRejeitada)
                .Take(top)
                .Select(x => (x.Produto, x.QuantidadeRejeitada))
                .ToList();
        }
    }
}