using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services
{
    public class ApontamentoHorasService : IApontamentoHorasService
    {
        private readonly OramaDbContext _context;

        public ApontamentoHorasService(OramaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ApontamentoHoras>> ObterTodosAsync(int empresaId)
        {
            return await _context.ApontamentosHoras
                .Include(a => a.OrdemProducao)
                .Include(a => a.Etapa)
                .Include(a => a.Funcionario)
                .Where(a => a.OrdemProducao.EmpresaId == empresaId)
                .OrderByDescending(a => a.DataCriacao)
                .ToListAsync();
        }

        public async Task<ApontamentoHoras?> ObterPorIdAsync(int id, int empresaId)
        {
            return await _context.ApontamentosHoras
                .Include(a => a.OrdemProducao)
                .Include(a => a.Etapa)
                .Include(a => a.Funcionario)
                .Where(a => a.Id == id && a.OrdemProducao.EmpresaId == empresaId)
                .FirstOrDefaultAsync();
        }

        public async Task<ApontamentoHoras> CriarAsync(ApontamentoHoras apontamento)
        {
            apontamento.DataCriacao = DateTime.Now;
            
            _context.ApontamentosHoras.Add(apontamento);
            await _context.SaveChangesAsync();
            
            return apontamento;
        }

        public async Task<ApontamentoHoras> AtualizarAsync(ApontamentoHoras apontamento)
        {
            var apontamentoExistente = await _context.ApontamentosHoras
                .Include(a => a.OrdemProducao)
                .FirstOrDefaultAsync(a => a.Id == apontamento.Id && a.OrdemProducao.EmpresaId == apontamento.OrdemProducao.EmpresaId);

            if (apontamentoExistente == null)
                throw new ArgumentException("Apontamento não encontrado");

            apontamentoExistente.DataFim = apontamento.DataFim;
            apontamentoExistente.HorasTrabalhadas = apontamento.HorasTrabalhadas;
            apontamentoExistente.ValorHora = apontamento.ValorHora;
            apontamentoExistente.Observacoes = apontamento.Observacoes;

            await _context.SaveChangesAsync();
            return apontamentoExistente;
        }

        public async Task<bool> ExcluirAsync(int id, int empresaId)
        {
            var apontamento = await _context.ApontamentosHoras
                .Include(a => a.OrdemProducao)
                .FirstOrDefaultAsync(a => a.Id == id && a.OrdemProducao.EmpresaId == empresaId);

            if (apontamento == null)
                return false;

            _context.ApontamentosHoras.Remove(apontamento);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ApontamentoHoras> IniciarApontamentoAsync(int ordemProducaoId, int? etapaId, 
            int funcionarioId, TipoApontamento tipo, int usuarioCriacaoId, int empresaId)
        {
            var apontamento = new ApontamentoHoras
            {
                OrdemProducaoId = ordemProducaoId,
                EtapaId = etapaId,
                FuncionarioId = funcionarioId,
                Tipo = tipo,
                DataInicio = DateTime.Now,
                UsuarioCriacaoId = usuarioCriacaoId,
                DataCriacao = DateTime.Now,
                ValorHora = 25.00m // Valor padrão
            };

            return await CriarAsync(apontamento);
        }

        public async Task<ApontamentoHoras> FinalizarApontamentoAsync(int id, int empresaId, string? observacoes = null)
        {
            var apontamento = await ObterPorIdAsync(id, empresaId);
            
            if (apontamento == null)
                throw new ArgumentException("Apontamento não encontrado");

            if (apontamento.DataFim.HasValue)
                throw new InvalidOperationException("Apontamento já foi finalizado");

            apontamento.DataFim = DateTime.Now;
            apontamento.HorasTrabalhadas = (decimal)(apontamento.DataFim.Value - apontamento.DataInicio).TotalHours;
            apontamento.Observacoes = observacoes;
            
            return await AtualizarAsync(apontamento);
        }

        public async Task<IEnumerable<ApontamentoHoras>> ObterPorOrdemProducaoAsync(int ordemProducaoId, int empresaId)
        {
            return await _context.ApontamentosHoras
                .Include(a => a.Etapa)
                .Include(a => a.Funcionario)
                .Where(a => a.OrdemProducaoId == ordemProducaoId && a.OrdemProducao.EmpresaId == empresaId)
                .OrderBy(a => a.DataInicio)
                .ToListAsync();
        }

        public async Task<IEnumerable<ApontamentoHoras>> ObterPorFuncionarioAsync(int funcionarioId, int empresaId, 
            DateTime? dataInicio = null, DateTime? dataFim = null)
        {
            var query = _context.ApontamentosHoras
                .Include(a => a.OrdemProducao)
                .Include(a => a.Etapa)
                .Where(a => a.FuncionarioId == funcionarioId && a.OrdemProducao.EmpresaId == empresaId);

            if (dataInicio.HasValue)
                query = query.Where(a => a.DataInicio >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(a => a.DataInicio <= dataFim.Value);

            return await query
                .OrderByDescending(a => a.DataInicio)
                .ToListAsync();
        }

        public async Task<IEnumerable<ApontamentoHoras>> ObterPorPeriodoAsync(int empresaId, DateTime dataInicio, DateTime dataFim)
        {
            return await _context.ApontamentosHoras
                .Include(a => a.OrdemProducao)
                .Include(a => a.Etapa)
                .Include(a => a.Funcionario)
                .Where(a => a.OrdemProducao.EmpresaId == empresaId &&
                           a.DataInicio >= dataInicio && a.DataInicio <= dataFim)
                .OrderBy(a => a.DataInicio)
                .ToListAsync();
        }

        public async Task<decimal> CalcularHorasTrabalhadasAsync(int ordemProducaoId, int empresaId)
        {
            var apontamentos = await _context.ApontamentosHoras
                .Include(a => a.OrdemProducao)
                .Where(a => a.OrdemProducaoId == ordemProducaoId && a.OrdemProducao.EmpresaId == empresaId && 
                           a.DataFim.HasValue)
                .ToListAsync();

            return apontamentos.Sum(a => a.HorasTrabalhadas);
        }

        public async Task<decimal> CalcularCustoMaoObraAsync(int ordemProducaoId, int empresaId)
        {
            var apontamentos = await _context.ApontamentosHoras
                .Include(a => a.OrdemProducao)
                .Where(a => a.OrdemProducaoId == ordemProducaoId && a.OrdemProducao.EmpresaId == empresaId && 
                           a.DataFim.HasValue)
                .ToListAsync();

            return apontamentos.Sum(a => a.CustoTotal);
        }

        public async Task<decimal> CalcularHorasFuncionarioAsync(int funcionarioId, int empresaId, 
            DateTime dataInicio, DateTime dataFim)
        {
            var apontamentos = await _context.ApontamentosHoras
                .Include(a => a.OrdemProducao)
                .Where(a => a.FuncionarioId == funcionarioId && a.OrdemProducao.EmpresaId == empresaId && 
                           a.DataFim.HasValue &&
                           a.DataInicio >= dataInicio && a.DataInicio <= dataFim)
                .ToListAsync();

            return apontamentos.Sum(a => a.HorasTrabalhadas);
        }

        public async Task<decimal> ObterHorasTrabalhadasHojeAsync(int empresaId)
        {
            var hoje = DateTime.Today;
            var amanha = hoje.AddDays(1);

            var apontamentos = await _context.ApontamentosHoras
                .Include(a => a.OrdemProducao)
                .Where(a => a.OrdemProducao.EmpresaId == empresaId &&
                           a.DataInicio >= hoje && a.DataInicio < amanha &&
                           a.DataFim.HasValue)
                .ToListAsync();

            return apontamentos.Sum(a => a.HorasTrabalhadas);
        }

        public async Task<decimal> ObterCustoMaoObraMesAsync(int empresaId, int mes, int ano)
        {
            var inicioMes = new DateTime(ano, mes, 1);
            var fimMes = inicioMes.AddMonths(1).AddDays(-1);

            var apontamentos = await _context.ApontamentosHoras
                .Include(a => a.OrdemProducao)
                .Where(a => a.OrdemProducao.EmpresaId == empresaId &&
                           a.DataInicio >= inicioMes && a.DataInicio <= fimMes &&
                           a.DataFim.HasValue)
                .ToListAsync();

            return apontamentos.Sum(a => a.CustoTotal);
        }

        public async Task<IEnumerable<(string NomeFuncionario, decimal HorasTrabalhadas)>> 
            ObterRankingFuncionariosAsync(int empresaId, DateTime dataInicio, DateTime dataFim)
        {
            var apontamentos = await _context.ApontamentosHoras
                .Include(a => a.OrdemProducao)
                .Include(a => a.Funcionario)
                .Where(a => a.OrdemProducao.EmpresaId == empresaId &&
                           a.DataInicio >= dataInicio && a.DataInicio <= dataFim &&
                           a.DataFim.HasValue)
                .ToListAsync();

            return apontamentos
                .GroupBy(a => new { a.FuncionarioId, a.Funcionario.Nome })
                .Select(g => (g.Key.Nome, g.Sum(a => a.HorasTrabalhadas)))
                .OrderByDescending(x => x.Item2)
                .ToList();
        }
    }
}