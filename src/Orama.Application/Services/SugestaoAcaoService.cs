using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services
{
    public class SugestaoAcaoService : ISugestaoAcaoService
    {
        private readonly OramaDbContext _context;
        
        public SugestaoAcaoService(OramaDbContext context)
        {
            _context = context;
        }
        
        public async Task GerarSugestoesAutomaticasAsync()
        {
            // Buscar todas as empresas ativas
            var empresas = await _context.Empresas.Where(e => e.Ativo).ToListAsync();
            
            foreach (var empresa in empresas)
            {
                await GerarSugestoesPorEmpresaAsync(empresa.Id);
            }
        }
        
        private async Task GerarSugestoesPorEmpresaAsync(int empresaId)
        {
            // Buscar produtos com problemas
            var produtos = await _context.Produtos
                .Where(p => p.EmpresaId == empresaId)
                .ToListAsync();
                
            foreach (var produto in produtos)
            {
                await GerarSugestoesProdutoAsync(produto.Id);
            }
        }
        
        public async Task GerarSugestoesProdutoAsync(int produtoId)
        {
            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == produtoId);
                
            if (produto == null) return;
            
            // Verificar se já existe sugestão pendente para este produto
            var sugestaoExistente = await _context.SugestoesAcao
                .AnyAsync(s => s.EmpresaId == produto.EmpresaId && 
                              s.TipoReferencia == TipoReferenciaSugestao.Produto &&
                              s.ReferenciaId == produtoId &&
                              s.Status == StatusSugestao.Pendente);
                              
            if (sugestaoExistente) return; // Não criar duplicatas
            
            var sugestoes = new List<SugestaoAcao>();
            
            // Regra 1: Preço abaixo do custo
            if (produto.PrecoVenda > 0 && produto.PrecoCusto > 0 && produto.PrecoVenda < produto.PrecoCusto)
            {
                sugestoes.Add(SugestaoAcao.CriarSugestaoPrecoAbaixoCusto(
                    produto.EmpresaId, produto.Id, produto.Descricao, produto.PrecoVenda, produto.PrecoCusto));
            }
            
            // Regra 2: Margem baixa (entre 0% e 10%)
            if (produto.PrecoVenda > 0 && produto.PrecoCusto > 0)
            {
                var margem = ((produto.PrecoVenda - produto.PrecoCusto) / produto.PrecoVenda) * 100;
                if (margem > 0 && margem < 10)
                {
                    sugestoes.Add(SugestaoAcao.CriarSugestaoMargemBaixa(
                        produto.EmpresaId, produto.Id, produto.Descricao, margem));
                }
            }
            
            // Regra 3: Vendas recorrentes com prejuízo (últimos 30 dias)
            var dataLimite = DateTime.Now.AddDays(-30);
            var vendasComPrejuizo = await _context.Vendas
                .Where(v => v.EmpresaId == produto.EmpresaId && 
                           v.DataVenda >= dataLimite &&
                           v.Status == StatusVenda.Faturada &&
                           v.Itens.Any(i => i.ProdutoId == produtoId && i.PrecoUnitario < i.CustoUnitario))
                .CountAsync();
                
            if (vendasComPrejuizo >= 3)
            {
                sugestoes.Add(SugestaoAcao.CriarSugestaoVendaRecorrentePrejuizo(
                    produto.EmpresaId, produto.Id, produto.Descricao, vendasComPrejuizo));
            }
            
            // Salvar sugestões
            if (sugestoes.Any())
            {
                _context.SugestoesAcao.AddRange(sugestoes);
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task<List<SugestaoAcao>> ObterSugestoesPendentesAsync()
        {
            return await _context.SugestoesAcao
                .Where(s => s.Status == StatusSugestao.Pendente)
                .OrderByDescending(s => s.Prioridade)
                .ThenBy(s => s.DataCriacao)
                .ToListAsync();
        }
        
        public async Task<List<SugestaoAcao>> ObterSugestoesPorPrioridadeAsync(PrioridadeSugestao prioridade)
        {
            return await _context.SugestoesAcao
                .Where(s => s.Status == StatusSugestao.Pendente &&
                           s.Prioridade == prioridade)
                .OrderBy(s => s.DataCriacao)
                .ToListAsync();
        }
        
        public async Task<SugestaoAcao?> ObterPorIdAsync(int id)
        {
            return await _context.SugestoesAcao
                .Include(s => s.UsuarioResolucao)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        
        public async Task<List<SugestaoAcao>> ObterPorProdutoAsync(int produtoId)
        {
            return await _context.SugestoesAcao
                .Where(s => s.TipoReferencia == TipoReferenciaSugestao.Produto &&
                           s.ReferenciaId == produtoId)
                .OrderByDescending(s => s.DataCriacao)
                .ToListAsync();
        }
        
        public async Task<bool> ResolverSugestaoAsync(int sugestaoId, int usuarioId, string observacao)
        {
            var sugestao = await _context.SugestoesAcao
                .FirstOrDefaultAsync(s => s.Id == sugestaoId);
                
            if (sugestao == null) return false;
            
            sugestao.Resolver(usuarioId, observacao);
            
            return await _context.SaveChangesAsync() > 0;
        }
        
        public async Task<bool> DescartarSugestaoAsync(int sugestaoId, int usuarioId, string motivo)
        {
            var sugestao = await _context.SugestoesAcao
                .FirstOrDefaultAsync(s => s.Id == sugestaoId);
                
            if (sugestao == null) return false;
            
            sugestao.Descartar(usuarioId, motivo);
            
            return await _context.SaveChangesAsync() > 0;
        }
        
        public async Task<int> ResolverSugestoesEmLoteAsync(List<int> sugestaoIds, int usuarioId, string observacao)
        {
            var sugestoes = await _context.SugestoesAcao
                .Where(s => sugestaoIds.Contains(s.Id))
                .ToListAsync();
                
            foreach (var sugestao in sugestoes)
            {
                sugestao.Resolver(usuarioId, observacao);
            }
            
            await _context.SaveChangesAsync();
            return sugestoes.Count;
        }
        
        public async Task<Dictionary<string, int>> ObterEstatisticasSugestoesAsync()
        {
            var estatisticas = new Dictionary<string, int>();
            
            var sugestoes = await _context.SugestoesAcao.ToListAsync();
                
            estatisticas["Total"] = sugestoes.Count;
            estatisticas["Pendentes"] = sugestoes.Count(s => s.Status == StatusSugestao.Pendente);
            estatisticas["Resolvidas"] = sugestoes.Count(s => s.Status == StatusSugestao.Resolvida);
            estatisticas["Descartadas"] = sugestoes.Count(s => s.Status == StatusSugestao.Descartada);
            estatisticas["Alta Prioridade"] = sugestoes.Count(s => s.Prioridade == PrioridadeSugestao.Alta && s.Status == StatusSugestao.Pendente);
            estatisticas["Média Prioridade"] = sugestoes.Count(s => s.Prioridade == PrioridadeSugestao.Media && s.Status == StatusSugestao.Pendente);
            estatisticas["Baixa Prioridade"] = sugestoes.Count(s => s.Prioridade == PrioridadeSugestao.Baixa && s.Status == StatusSugestao.Pendente);
            
            return estatisticas;
        }
        
        public async Task<List<SugestaoAcao>> ObterSugestoesRecentesAsync(int quantidade = 10)
        {
            return await _context.SugestoesAcao
                .OrderByDescending(s => s.DataCriacao)
                .Take(quantidade)
                .ToListAsync();
        }
    }
}