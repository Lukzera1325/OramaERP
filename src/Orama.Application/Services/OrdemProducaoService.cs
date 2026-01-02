using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services
{
    public class OrdemProducaoService : IOrdemProducaoService
    {
        private readonly OramaDbContext _context;

        public OrdemProducaoService(OramaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrdemProducao>> ObterTodosAsync(int empresaId)
        {
            return await _context.OrdensProducao
                .Include(op => op.Produto)
                .Include(op => op.UsuarioCriacao)
                .Include(op => op.Itens).ThenInclude(i => i.Produto)
                .Include(op => op.Etapas)
                .Where(op => op.EmpresaId == empresaId)
                .OrderByDescending(op => op.DataCriacao)
                .ToListAsync();
        }

        public async Task<OrdemProducao?> ObterPorIdAsync(int id, int empresaId)
        {
            return await _context.OrdensProducao
                .Include(op => op.Produto)
                .Include(op => op.UsuarioCriacao)
                .Include(op => op.Itens).ThenInclude(i => i.Produto)
                .Include(op => op.Etapas).ThenInclude(e => e.Responsavel)
                .Include(op => op.ApontamentosHoras).ThenInclude(ah => ah.Funcionario)
                .Include(op => op.InspecoesQualidade).ThenInclude(iq => iq.Inspetor)
                .FirstOrDefaultAsync(op => op.Id == id && op.EmpresaId == empresaId);
        }

        public async Task<OrdemProducao> CriarAsync(OrdemProducao ordemProducao)
        {
            ordemProducao.Numero = await GerarProximoNumeroAsync(ordemProducao.EmpresaId);
            ordemProducao.Status = StatusOrdemProducao.Planejada;
            ordemProducao.DataCriacao = DateTime.UtcNow;

            _context.OrdensProducao.Add(ordemProducao);
            await _context.SaveChangesAsync();

            // Criar itens baseados na lista de materiais
            await CriarItensBaseadoEmBOMAsync(ordemProducao);

            return ordemProducao;
        }

        public async Task<OrdemProducao> AtualizarAsync(OrdemProducao ordemProducao)
        {
            ordemProducao.DataAtualizacao = DateTime.UtcNow;
            _context.OrdensProducao.Update(ordemProducao);
            await _context.SaveChangesAsync();
            return ordemProducao;
        }

        public async Task<bool> ExcluirAsync(int id, int empresaId)
        {
            var ordem = await _context.OrdensProducao
                .FirstOrDefaultAsync(op => op.Id == id && op.EmpresaId == empresaId);

            if (ordem == null || ordem.Status != StatusOrdemProducao.Planejada)
                return false;

            _context.OrdensProducao.Remove(ordem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GerarProximoNumeroAsync(int empresaId)
        {
            var ano = DateTime.Now.Year;
            var prefixo = $"OP{ano:0000}";
            
            var ultimaOrdem = await _context.OrdensProducao
                .Where(op => op.EmpresaId == empresaId && op.Numero.StartsWith(prefixo))
                .OrderByDescending(op => op.Numero)
                .FirstOrDefaultAsync();

            if (ultimaOrdem == null)
                return $"{prefixo}0001";

            var ultimoNumero = ultimaOrdem.Numero.Substring(prefixo.Length);
            if (int.TryParse(ultimoNumero, out var numero))
                return $"{prefixo}{(numero + 1):0000}";

            return $"{prefixo}0001";
        }

        public async Task<bool> LiberarOrdemAsync(int id, int empresaId, int usuarioId)
        {
            var ordem = await _context.OrdensProducao
                .FirstOrDefaultAsync(op => op.Id == id && op.EmpresaId == empresaId);

            if (ordem == null || ordem.Status != StatusOrdemProducao.Planejada)
                return false;

            // Verificar disponibilidade de materiais
            var materiaisInsuficientes = await VerificarDisponibilidadeMateriaisAsync(ordem);
            if (materiaisInsuficientes.Any())
                return false;

            ordem.Status = StatusOrdemProducao.Liberada;
            ordem.DataAtualizacao = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IniciarProducaoAsync(int id, int empresaId, int usuarioId)
        {
            var ordem = await _context.OrdensProducao
                .FirstOrDefaultAsync(op => op.Id == id && op.EmpresaId == empresaId);

            if (ordem == null || ordem.Status != StatusOrdemProducao.Liberada)
                return false;

            ordem.Status = StatusOrdemProducao.EmAndamento;
            ordem.DataInicio = DateTime.UtcNow;
            ordem.DataAtualizacao = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PausarProducaoAsync(int id, int empresaId, int usuarioId, string motivo)
        {
            var ordem = await _context.OrdensProducao
                .FirstOrDefaultAsync(op => op.Id == id && op.EmpresaId == empresaId);

            if (ordem == null || ordem.Status != StatusOrdemProducao.EmAndamento)
                return false;

            ordem.Status = StatusOrdemProducao.Pausada;
            ordem.Observacoes = $"{ordem.Observacoes}\nPausada em {DateTime.Now:dd/MM/yyyy HH:mm}: {motivo}";
            ordem.DataAtualizacao = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FinalizarProducaoAsync(int id, int empresaId, int usuarioId)
        {
            var ordem = await _context.OrdensProducao
                .Include(op => op.Itens)
                .FirstOrDefaultAsync(op => op.Id == id && op.EmpresaId == empresaId);

            if (ordem == null || (ordem.Status != StatusOrdemProducao.EmAndamento && ordem.Status != StatusOrdemProducao.Pausada))
                return false;

            ordem.Status = StatusOrdemProducao.Finalizada;
            ordem.DataFim = DateTime.UtcNow;
            ordem.DataAtualizacao = DateTime.UtcNow;
            
            // Dar entrada no estoque do produto produzido
            await DarEntradaEstoqueProdutoAsync(ordem);
            
            // Consumir materiais do estoque
            await ConsumirMateriaisEstoqueAsync(ordem);
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelarOrdemAsync(int id, int empresaId, int usuarioId, string motivo)
        {
            var ordem = await _context.OrdensProducao
                .FirstOrDefaultAsync(op => op.Id == id && op.EmpresaId == empresaId);

            if (ordem == null || ordem.Status == StatusOrdemProducao.Finalizada)
                return false;

            ordem.Status = StatusOrdemProducao.Cancelada;
            ordem.Observacoes = $"{ordem.Observacoes}\nCancelada em {DateTime.Now:dd/MM/yyyy HH:mm}: {motivo}";
            ordem.DataAtualizacao = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<OrdemProducao>> ObterPorStatusAsync(int empresaId, StatusOrdemProducao status)
        {
            return await _context.OrdensProducao
                .Include(op => op.Produto)
                .Where(op => op.EmpresaId == empresaId && op.Status == status)
                .OrderBy(op => op.DataPlanejada)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrdemProducao>> ObterPorPeriodoAsync(int empresaId, DateTime dataInicio, DateTime dataFim)
        {
            return await _context.OrdensProducao
                .Include(op => op.Produto)
                .Where(op => op.EmpresaId == empresaId && 
                           op.DataPlanejada >= dataInicio && 
                           op.DataPlanejada <= dataFim)
                .OrderBy(op => op.DataPlanejada)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrdemProducao>> ObterPorProdutoAsync(int empresaId, int produtoId)
        {
            return await _context.OrdensProducao
                .Include(op => op.Produto)
                .Where(op => op.EmpresaId == empresaId && op.ProdutoId == produtoId)
                .OrderByDescending(op => op.DataCriacao)
                .ToListAsync();
        }

        public async Task<decimal> CalcularCustoTotalAsync(int id, int empresaId)
        {
            var ordem = await _context.OrdensProducao
                .Include(op => op.Itens)
                .Include(op => op.ApontamentosHoras)
                .FirstOrDefaultAsync(op => op.Id == id && op.EmpresaId == empresaId);

            if (ordem == null) return 0;

            var custoMaterial = ordem.Itens.Sum(i => i.CustoTotal);
            var custoMaoObra = ordem.ApontamentosHoras.Sum(ah => ah.CustoTotal);

            return custoMaterial + custoMaoObra;
        }

        public async Task<int> ObterQuantidadeOrdensAtivasAsync(int empresaId)
        {
            return await _context.OrdensProducao
                .CountAsync(op => op.EmpresaId == empresaId && 
                                (op.Status == StatusOrdemProducao.Liberada || 
                                 op.Status == StatusOrdemProducao.EmAndamento || 
                                 op.Status == StatusOrdemProducao.Pausada));
        }

        public async Task<int> ObterQuantidadeOrdensAtrasadasAsync(int empresaId)
        {
            var hoje = DateTime.Today;
            return await _context.OrdensProducao
                .CountAsync(op => op.EmpresaId == empresaId && 
                                op.DataPlanejada < hoje &&
                                op.Status != StatusOrdemProducao.Finalizada &&
                                op.Status != StatusOrdemProducao.Cancelada);
        }

        public async Task<decimal> ObterCustoProducaoMesAsync(int empresaId, int mes, int ano)
        {
            var dataInicio = new DateTime(ano, mes, 1);
            var dataFim = dataInicio.AddMonths(1).AddDays(-1);

            var ordens = await _context.OrdensProducao
                .Include(op => op.Itens)
                .Include(op => op.ApontamentosHoras)
                .Where(op => op.EmpresaId == empresaId && 
                           op.Status == StatusOrdemProducao.Finalizada &&
                           op.DataFim >= dataInicio && 
                           op.DataFim <= dataFim)
                .ToListAsync();

            return ordens.Sum(op => op.Itens.Sum(i => i.CustoTotal) + op.ApontamentosHoras.Sum(ah => ah.CustoTotal));
        }

        private async Task CriarItensBaseadoEmBOMAsync(OrdemProducao ordem)
        {
            var listaMateriais = await _context.ListasMateriais
                .Include(lm => lm.Itens).ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(lm => lm.ProdutoId == ordem.ProdutoId && lm.Ativo);

            if (listaMateriais == null) return;

            foreach (var itemBOM in listaMateriais.Itens)
            {
                var quantidadeNecessaria = (itemBOM.QuantidadeLiquida * ordem.QuantidadePlanejada) / listaMateriais.QuantidadeBase;
                
                var itemOrdem = new OrdemProducaoItem
                {
                    OrdemProducaoId = ordem.Id,
                    ProdutoId = itemBOM.ProdutoId,
                    QuantidadeNecessaria = quantidadeNecessaria,
                    CustoUnitario = itemBOM.CustoUnitario,
                    Tipo = (TipoItemProducao)itemBOM.Tipo,
                    DataCriacao = DateTime.UtcNow
                };

                _context.OrdemProducaoItens.Add(itemOrdem);
            }

            await _context.SaveChangesAsync();
        }

        private async Task<IEnumerable<string>> VerificarDisponibilidadeMateriaisAsync(OrdemProducao ordem)
        {
            var materiaisInsuficientes = new List<string>();
            
            var itens = await _context.OrdemProducaoItens
                .Include(i => i.Produto)
                .Where(i => i.OrdemProducaoId == ordem.Id)
                .ToListAsync();

            foreach (var item in itens)
            {
                if (item.Produto.ControlaEstoque && item.Produto.EstoqueAtual < item.QuantidadeNecessaria)
                {
                    materiaisInsuficientes.Add($"{item.Produto.Descricao} - Necessário: {item.QuantidadeNecessaria}, Disponível: {item.Produto.EstoqueAtual}");
                }
            }

            return materiaisInsuficientes;
        }

        private async Task DarEntradaEstoqueProdutoAsync(OrdemProducao ordem)
        {
            var produto = await _context.Produtos.FindAsync(ordem.ProdutoId);
            if (produto != null && produto.ControlaEstoque)
            {
                produto.EstoqueAtual += ordem.QuantidadeProduzida;

                var movimentacao = new MovimentacaoEstoque
                {
                    EmpresaId = ordem.EmpresaId,
                    ProdutoId = ordem.ProdutoId,
                    Tipo = TipoMovimentacaoEstoque.EntradaCompra, // Entrada por produção
                    Quantidade = ordem.QuantidadeProduzida,
                    CustoUnitario = produto.PrecoCusto,
                    Observacoes = $"Produção - OP {ordem.Numero}",
                    DataMovimentacao = DateTime.UtcNow,
                    UsuarioId = ordem.UsuarioCriacaoId
                };

                _context.MovimentacoesEstoque.Add(movimentacao);
            }
        }

        private async Task ConsumirMateriaisEstoqueAsync(OrdemProducao ordem)
        {
            var itens = await _context.OrdemProducaoItens
                .Include(i => i.Produto)
                .Where(i => i.OrdemProducaoId == ordem.Id)
                .ToListAsync();

            foreach (var item in itens)
            {
                if (item.Produto.ControlaEstoque)
                {
                    item.Produto.EstoqueAtual -= item.QuantidadeConsumida > 0 ? item.QuantidadeConsumida : item.QuantidadeNecessaria;

                    var movimentacao = new MovimentacaoEstoque
                    {
                        EmpresaId = ordem.EmpresaId,
                        ProdutoId = item.ProdutoId,
                        Tipo = TipoMovimentacaoEstoque.SaidaVenda, // Saída por consumo
                        Quantidade = item.QuantidadeConsumida > 0 ? item.QuantidadeConsumida : item.QuantidadeNecessaria,
                        CustoUnitario = item.CustoUnitario,
                        Observacoes = $"Consumo Produção - OP {ordem.Numero}",
                        DataMovimentacao = DateTime.UtcNow,
                        UsuarioId = ordem.UsuarioCriacaoId
                    };

                    _context.MovimentacoesEstoque.Add(movimentacao);
                }
            }
        }
    }
}