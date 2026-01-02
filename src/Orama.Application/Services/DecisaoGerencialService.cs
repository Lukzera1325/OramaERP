using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services
{
    public class DecisaoGerencialService : IDecisaoGerencialService
    {
        private readonly OramaDbContext _context;
        
        public DecisaoGerencialService(OramaDbContext context)
        {
            _context = context;
        }
        
        public async Task<bool> RegistrarDecisaoProdutoAsync(int produtoId, string problema, string acao, string observacoes = "")
        {
            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == produtoId);
                
            if (produto == null) return false;
            
            var decisao = DecisaoGerencial.CriarDecisaoProduto(
                produto.EmpresaId, 1, produtoId, produto.Descricao, problema, acao, observacoes); // TODO: Pegar usuário real
                
            _context.DecisoesGerenciais.Add(decisao);
            return await _context.SaveChangesAsync() > 0;
        }
        
        public async Task<bool> RegistrarDecisaoVendaAsync(int vendaId, string problema, string acao, string observacoes = "")
        {
            var venda = await _context.Vendas
                .FirstOrDefaultAsync(v => v.Id == vendaId);
                
            if (venda == null) return false;
            
            var decisao = DecisaoGerencial.CriarDecisaoVenda(
                venda.EmpresaId, 1, vendaId, venda.Numero, problema, acao, observacoes); // TODO: Pegar usuário real
                
            _context.DecisoesGerenciais.Add(decisao);
            return await _context.SaveChangesAsync() > 0;
        }
        
        public async Task<bool> RegistrarDecisaoSugestaoAsync(int sugestaoId, string acao, string observacoes = "")
        {
            var sugestao = await _context.SugestoesAcao
                .FirstOrDefaultAsync(s => s.Id == sugestaoId);
                
            if (sugestao == null) return false;
            
            var decisao = DecisaoGerencial.CriarDecisaoSugestao(
                sugestao.EmpresaId, 1, sugestaoId, sugestao.TextoSugestao, acao, observacoes); // TODO: Pegar usuário real
                
            _context.DecisoesGerenciais.Add(decisao);
            return await _context.SaveChangesAsync() > 0;
        }
        
        public async Task<List<DecisaoGerencial>> ObterDecisoesPorProdutoAsync(int produtoId)
        {
            return await _context.DecisoesGerenciais
                .Include(d => d.Usuario)
                .Where(d => d.TipoReferencia == TipoReferenciaDecisao.Produto &&
                           d.ReferenciaId == produtoId)
                .OrderByDescending(d => d.DataDecisao)
                .ToListAsync();
        }
        
        public async Task<List<DecisaoGerencial>> ObterDecisoesPorPeriodoAsync(DateTime inicio, DateTime fim)
        {
            return await _context.DecisoesGerenciais
                .Include(d => d.Usuario)
                .Where(d => d.DataDecisao >= inicio &&
                           d.DataDecisao <= fim)
                .OrderByDescending(d => d.DataDecisao)
                .ToListAsync();
        }
        
        public async Task<List<DecisaoGerencial>> ObterDecisoesRecentesAsync(int quantidade = 20)
        {
            return await _context.DecisoesGerenciais
                .Include(d => d.Usuario)
                .OrderByDescending(d => d.DataDecisao)
                .Take(quantidade)
                .ToListAsync();
        }
        
        public async Task<DecisaoGerencial?> ObterPorIdAsync(int id)
        {
            return await _context.DecisoesGerenciais
                .Include(d => d.Usuario)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
        
        public async Task<List<DecisaoGerencial>> BuscarDecisoesSemelhanteAsync(string problema)
        {
            return await _context.DecisoesGerenciais
                .Include(d => d.Usuario)
                .Where(d => d.ProblemaIdentificado.Contains(problema))
                .OrderByDescending(d => d.DataDecisao)
                .Take(10)
                .ToListAsync();
        }
        
        public async Task<Dictionary<string, int>> ObterEstatisticasDecisoesAsync()
        {
            var estatisticas = new Dictionary<string, int>();
            
            var decisoes = await _context.DecisoesGerenciais.ToListAsync();
                
            estatisticas["Total"] = decisoes.Count;
            estatisticas["Este Mês"] = decisoes.Count(d => d.DataDecisao.Month == DateTime.Now.Month && d.DataDecisao.Year == DateTime.Now.Year);
            estatisticas["Produtos"] = decisoes.Count(d => d.TipoReferencia == TipoReferenciaDecisao.Produto);
            estatisticas["Vendas"] = decisoes.Count(d => d.TipoReferencia == TipoReferenciaDecisao.Venda);
            estatisticas["Sugestões"] = decisoes.Count(d => d.TipoReferencia == TipoReferenciaDecisao.Sugestao);
            estatisticas["Efetivas"] = decisoes.Count(d => d.FoiEfetiva == true);
            estatisticas["Não Efetivas"] = decisoes.Count(d => d.FoiEfetiva == false);
            estatisticas["Não Avaliadas"] = decisoes.Count(d => d.FoiEfetiva == null);
            
            return estatisticas;
        }
        
        public async Task<List<DecisaoGerencial>> ObterDecisoesPorUsuarioAsync(int usuarioId)
        {
            return await _context.DecisoesGerenciais
                .Include(d => d.Usuario)
                .Where(d => d.UsuarioId == usuarioId)
                .OrderByDescending(d => d.DataDecisao)
                .ToListAsync();
        }
        
        public async Task<bool> AvaliarResultadoDecisaoAsync(int decisaoId, string resultado, bool foiEfetiva)
        {
            var decisao = await _context.DecisoesGerenciais
                .FirstOrDefaultAsync(d => d.Id == decisaoId);
                
            if (decisao == null) return false;
            
            decisao.RegistrarResultado(resultado, foiEfetiva);
            
            return await _context.SaveChangesAsync() > 0;
        }
        
        public async Task<List<DecisaoGerencial>> ObterDecisoesParaAvaliacaoAsync()
        {
            // Decisões com mais de 7 dias que ainda não foram avaliadas
            var dataLimite = DateTime.Now.AddDays(-7);
            
            return await _context.DecisoesGerenciais
                .Include(d => d.Usuario)
                .Where(d => d.DataDecisao <= dataLimite &&
                           d.FoiEfetiva == null)
                .OrderBy(d => d.DataDecisao)
                .ToListAsync();
        }
    }
}