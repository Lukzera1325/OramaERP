using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services
{
    public class NaoConformidadeService : INaoConformidadeService
    {
        private readonly OramaDbContext _context;

        public NaoConformidadeService(OramaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NaoConformidade>> ObterTodosAsync(int empresaId)
        {
            return await _context.NaoConformidades
                .Include(n => n.DetectadoPor)
                .Include(n => n.Responsavel)
                .Where(n => n.EmpresaId == empresaId)
                .OrderByDescending(n => n.DataCriacao)
                .ToListAsync();
        }

        public async Task<NaoConformidade?> ObterPorIdAsync(int id, int empresaId)
        {
            return await _context.NaoConformidades
                .Include(n => n.DetectadoPor)
                .Include(n => n.Responsavel)
                .Include(n => n.OrdemProducao)
                .Include(n => n.Produto)
                .Where(n => n.Id == id && n.EmpresaId == empresaId)
                .FirstOrDefaultAsync();
        }

        public async Task<NaoConformidade> CriarAsync(NaoConformidade naoConformidade)
        {
            naoConformidade.Numero = await GerarProximoNumeroAsync(naoConformidade.EmpresaId);
            naoConformidade.Status = StatusNaoConformidade.Aberta;
            naoConformidade.DataDeteccao = DateTime.Now;
            naoConformidade.DataCriacao = DateTime.Now;
            naoConformidade.DataAtualizacao = DateTime.Now;
            
            _context.NaoConformidades.Add(naoConformidade);
            await _context.SaveChangesAsync();
            
            return naoConformidade;
        }

        public async Task<NaoConformidade> AtualizarAsync(NaoConformidade naoConformidade)
        {
            var naoConformidadeExistente = await _context.NaoConformidades
                .FirstOrDefaultAsync(n => n.Id == naoConformidade.Id && n.EmpresaId == naoConformidade.EmpresaId);

            if (naoConformidadeExistente == null)
                throw new ArgumentException("Não conformidade não encontrada");

            naoConformidadeExistente.Titulo = naoConformidade.Titulo;
            naoConformidadeExistente.Descricao = naoConformidade.Descricao;
            naoConformidadeExistente.Tipo = naoConformidade.Tipo;
            naoConformidadeExistente.Severidade = naoConformidade.Severidade;
            naoConformidadeExistente.ResponsavelId = naoConformidade.ResponsavelId;
            naoConformidadeExistente.CausaRaiz = naoConformidade.CausaRaiz;
            naoConformidadeExistente.AcaoCorretiva = naoConformidade.AcaoCorretiva;
            naoConformidadeExistente.AcaoPreventiva = naoConformidade.AcaoPreventiva;
            naoConformidadeExistente.DataAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();
            return naoConformidadeExistente;
        }

        public async Task<bool> ExcluirAsync(int id, int empresaId)
        {
            var naoConformidade = await _context.NaoConformidades
                .FirstOrDefaultAsync(n => n.Id == id && n.EmpresaId == empresaId);

            if (naoConformidade == null)
                return false;

            _context.NaoConformidades.Remove(naoConformidade);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GerarProximoNumeroAsync(int empresaId)
        {
            var ano = DateTime.Now.Year;
            var ultimaNC = await _context.NaoConformidades
                .Where(n => n.EmpresaId == empresaId && n.Numero.StartsWith($"NC{ano}"))
                .OrderByDescending(n => n.Numero)
                .FirstOrDefaultAsync();

            int proximoNumero = 1;
            if (ultimaNC != null)
            {
                var numeroStr = ultimaNC.Numero.Substring(6); // Remove "NC2024"
                if (int.TryParse(numeroStr, out int numero))
                {
                    proximoNumero = numero + 1;
                }
            }

            return $"NC{ano}{proximoNumero:D4}";
        }

        public async Task<bool> AtribuirResponsavelAsync(int id, int responsavelId, int empresaId)
        {
            var naoConformidade = await _context.NaoConformidades
                .FirstOrDefaultAsync(n => n.Id == id && n.EmpresaId == empresaId);

            if (naoConformidade == null)
                return false;

            naoConformidade.ResponsavelId = responsavelId;
            naoConformidade.Status = StatusNaoConformidade.EmAnalise;
            naoConformidade.DataAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DefinirAcaoCorretivaAsync(int id, string acaoCorretiva, DateTime dataPrazo, int empresaId)
        {
            var naoConformidade = await _context.NaoConformidades
                .FirstOrDefaultAsync(n => n.Id == id && n.EmpresaId == empresaId);

            if (naoConformidade == null)
                return false;

            naoConformidade.AcaoCorretiva = acaoCorretiva;
            naoConformidade.DataPrazo = dataPrazo;
            naoConformidade.Status = StatusNaoConformidade.EmAndamento;
            naoConformidade.DataAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResolverNaoConformidadeAsync(int id, string observacoesResolucao, int empresaId)
        {
            var naoConformidade = await _context.NaoConformidades
                .FirstOrDefaultAsync(n => n.Id == id && n.EmpresaId == empresaId);

            if (naoConformidade == null)
                return false;

            naoConformidade.ObservacoesResolucao = observacoesResolucao;
            naoConformidade.DataResolucao = DateTime.Now;
            naoConformidade.Status = StatusNaoConformidade.Resolvida;
            naoConformidade.DataAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FecharNaoConformidadeAsync(int id, int empresaId)
        {
            var naoConformidade = await _context.NaoConformidades
                .FirstOrDefaultAsync(n => n.Id == id && n.EmpresaId == empresaId);

            if (naoConformidade == null)
                return false;

            if (naoConformidade.Status != StatusNaoConformidade.Resolvida)
                throw new InvalidOperationException("Não conformidade deve estar resolvida para ser fechada");

            naoConformidade.Status = StatusNaoConformidade.Fechada;
            naoConformidade.DataAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<NaoConformidade>> ObterPorStatusAsync(int empresaId, StatusNaoConformidade status)
        {
            return await _context.NaoConformidades
                .Include(n => n.DetectadoPor)
                .Include(n => n.Responsavel)
                .Where(n => n.EmpresaId == empresaId && n.Status == status)
                .OrderByDescending(n => n.DataCriacao)
                .ToListAsync();
        }

        public async Task<IEnumerable<NaoConformidade>> ObterPorTipoAsync(int empresaId, TipoNaoConformidade tipo)
        {
            return await _context.NaoConformidades
                .Include(n => n.DetectadoPor)
                .Include(n => n.Responsavel)
                .Where(n => n.EmpresaId == empresaId && n.Tipo == tipo)
                .OrderByDescending(n => n.DataCriacao)
                .ToListAsync();
        }

        public async Task<IEnumerable<NaoConformidade>> ObterPorSeveridadeAsync(int empresaId, SeveridadeNaoConformidade severidade)
        {
            return await _context.NaoConformidades
                .Include(n => n.DetectadoPor)
                .Include(n => n.Responsavel)
                .Where(n => n.EmpresaId == empresaId && n.Severidade == severidade)
                .OrderByDescending(n => n.DataCriacao)
                .ToListAsync();
        }

        public async Task<IEnumerable<NaoConformidade>> ObterPorResponsavelAsync(int responsavelId, int empresaId)
        {
            return await _context.NaoConformidades
                .Include(n => n.DetectadoPor)
                .Where(n => n.ResponsavelId == responsavelId && n.EmpresaId == empresaId)
                .OrderByDescending(n => n.DataCriacao)
                .ToListAsync();
        }

        public async Task<IEnumerable<NaoConformidade>> ObterVencidasAsync(int empresaId)
        {
            var hoje = DateTime.Today;
            return await _context.NaoConformidades
                .Include(n => n.DetectadoPor)
                .Include(n => n.Responsavel)
                .Where(n => n.EmpresaId == empresaId &&
                           n.DataPrazo.HasValue && n.DataPrazo.Value < hoje &&
                           n.Status != StatusNaoConformidade.Resolvida &&
                           n.Status != StatusNaoConformidade.Fechada)
                .OrderBy(n => n.DataPrazo)
                .ToListAsync();
        }

        public async Task<IEnumerable<NaoConformidade>> ObterPorPeriodoAsync(int empresaId, DateTime dataInicio, DateTime dataFim)
        {
            return await _context.NaoConformidades
                .Include(n => n.DetectadoPor)
                .Include(n => n.Responsavel)
                .Where(n => n.EmpresaId == empresaId &&
                           n.DataDeteccao >= dataInicio && n.DataDeteccao <= dataFim)
                .OrderBy(n => n.DataDeteccao)
                .ToListAsync();
        }

        public async Task<IEnumerable<(string Tipo, int Quantidade)>> ObterEstatisticasPorTipoAsync(int empresaId, 
            DateTime dataInicio, DateTime dataFim)
        {
            var naoConformidades = await _context.NaoConformidades
                .Where(n => n.EmpresaId == empresaId &&
                           n.DataDeteccao >= dataInicio && n.DataDeteccao <= dataFim)
                .ToListAsync();

            return naoConformidades
                .GroupBy(n => n.Tipo)
                .Select(g => (g.Key.ToString(), g.Count()))
                .OrderByDescending(x => x.Item2)
                .ToList();
        }

        public async Task<IEnumerable<(string Severidade, int Quantidade)>> ObterEstatisticasPorSeveridadeAsync(int empresaId, 
            DateTime dataInicio, DateTime dataFim)
        {
            var naoConformidades = await _context.NaoConformidades
                .Where(n => n.EmpresaId == empresaId &&
                           n.DataDeteccao >= dataInicio && n.DataDeteccao <= dataFim)
                .ToListAsync();

            return naoConformidades
                .GroupBy(n => n.Severidade)
                .Select(g => (g.Key.ToString(), g.Count()))
                .OrderByDescending(x => x.Item2)
                .ToList();
        }

        public async Task<decimal> CalcularTempoMedioResolucaoAsync(int empresaId, DateTime dataInicio, DateTime dataFim)
        {
            var naoConformidades = await _context.NaoConformidades
                .Where(n => n.EmpresaId == empresaId &&
                           n.DataDeteccao >= dataInicio && n.DataDeteccao <= dataFim &&
                           n.DataResolucao.HasValue)
                .ToListAsync();

            if (!naoConformidades.Any())
                return 0;

            var temposTotalDias = naoConformidades
                .Select(n => (n.DataResolucao!.Value - n.DataDeteccao).TotalDays)
                .ToList();

            return (decimal)temposTotalDias.Average();
        }

        public async Task<int> ObterQuantidadeAbertasAsync(int empresaId)
        {
            return await _context.NaoConformidades
                .CountAsync(n => n.EmpresaId == empresaId &&
                               (n.Status == StatusNaoConformidade.Aberta ||
                                n.Status == StatusNaoConformidade.EmAnalise ||
                                n.Status == StatusNaoConformidade.EmAndamento));
        }

        public async Task<int> ObterQuantidadeVencidasAsync(int empresaId)
        {
            var hoje = DateTime.Today;
            return await _context.NaoConformidades
                .CountAsync(n => n.EmpresaId == empresaId &&
                               n.DataPrazo.HasValue && n.DataPrazo.Value < hoje &&
                               n.Status != StatusNaoConformidade.Resolvida &&
                               n.Status != StatusNaoConformidade.Fechada);
        }

        public async Task<int> ObterQuantidadeCriticasAsync(int empresaId)
        {
            return await _context.NaoConformidades
                .CountAsync(n => n.EmpresaId == empresaId &&
                               n.Severidade == SeveridadeNaoConformidade.Critica &&
                               n.Status != StatusNaoConformidade.Resolvida &&
                               n.Status != StatusNaoConformidade.Fechada);
        }

        public async Task<IEnumerable<(string Responsavel, int QuantidadeAberta)>> 
            ObterNaoConformidadesPorResponsavelAsync(int empresaId)
        {
            var naoConformidades = await _context.NaoConformidades
                .Include(n => n.Responsavel)
                .Where(n => n.EmpresaId == empresaId &&
                           n.ResponsavelId.HasValue &&
                           (n.Status == StatusNaoConformidade.EmAnalise ||
                            n.Status == StatusNaoConformidade.EmAndamento))
                .ToListAsync();

            return naoConformidades
                .GroupBy(n => new { n.ResponsavelId, n.Responsavel!.Nome })
                .Select(g => (g.Key.Nome, g.Count()))
                .OrderByDescending(x => x.Item2)
                .ToList();
        }
    }
}