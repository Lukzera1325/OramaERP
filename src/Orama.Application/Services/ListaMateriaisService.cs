using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services
{
    public class ListaMateriaisService : IListaMateriaisService
    {
        private readonly OramaDbContext _context;

        public ListaMateriaisService(OramaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ListaMateriais>> ObterTodosAsync(int empresaId)
        {
            return await _context.ListasMateriais
                .Include(lm => lm.Produto)
                .Include(lm => lm.UsuarioCriacao)
                .Include(lm => lm.Itens).ThenInclude(i => i.Produto)
                .Where(lm => lm.EmpresaId == empresaId)
                .OrderByDescending(lm => lm.DataCriacao)
                .ToListAsync();
        }

        public async Task<ListaMateriais?> ObterPorIdAsync(int id, int empresaId)
        {
            return await _context.ListasMateriais
                .Include(lm => lm.Produto)
                .Include(lm => lm.UsuarioCriacao)
                .Include(lm => lm.Itens).ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(lm => lm.Id == id && lm.EmpresaId == empresaId);
        }

        public async Task<ListaMateriais?> ObterPorProdutoAsync(int produtoId, int empresaId)
        {
            return await _context.ListasMateriais
                .Include(lm => lm.Produto)
                .Include(lm => lm.Itens).ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(lm => lm.ProdutoId == produtoId && lm.EmpresaId == empresaId && lm.Ativo);
        }

        public async Task<ListaMateriais> CriarAsync(ListaMateriais listaMateriais)
        {
            listaMateriais.DataCriacao = DateTime.UtcNow;
            listaMateriais.DataVigencia = DateTime.UtcNow;
            
            // Desativar versões anteriores do mesmo produto
            var versaoAnterior = await _context.ListasMateriais
                .FirstOrDefaultAsync(lm => lm.ProdutoId == listaMateriais.ProdutoId && 
                                         lm.EmpresaId == listaMateriais.EmpresaId && 
                                         lm.Ativo);
            
            if (versaoAnterior != null)
            {
                versaoAnterior.Ativo = false;
                versaoAnterior.DataVencimento = DateTime.UtcNow;
                versaoAnterior.DataAtualizacao = DateTime.UtcNow;
            }

            _context.ListasMateriais.Add(listaMateriais);
            await _context.SaveChangesAsync();

            // Calcular custo total
            await AtualizarCustoTotalAsync(listaMateriais.Id);

            return listaMateriais;
        }

        public async Task<ListaMateriais> AtualizarAsync(ListaMateriais listaMateriais)
        {
            listaMateriais.DataAtualizacao = DateTime.UtcNow;
            _context.ListasMateriais.Update(listaMateriais);
            await _context.SaveChangesAsync();

            // Recalcular custo total
            await AtualizarCustoTotalAsync(listaMateriais.Id);

            return listaMateriais;
        }

        public async Task<bool> ExcluirAsync(int id, int empresaId)
        {
            var listaMateriais = await _context.ListasMateriais
                .FirstOrDefaultAsync(lm => lm.Id == id && lm.EmpresaId == empresaId);

            if (listaMateriais == null)
                return false;

            // Verificar se há ordens de produção usando esta lista
            var temOrdens = await _context.OrdensProducao
                .AnyAsync(op => op.ProdutoId == listaMateriais.ProdutoId && 
                              op.EmpresaId == empresaId &&
                              (op.Status == StatusOrdemProducao.Liberada || 
                               op.Status == StatusOrdemProducao.EmAndamento));

            if (temOrdens)
                return false;

            _context.ListasMateriais.Remove(listaMateriais);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ListaMateriaisItem> AdicionarItemAsync(ListaMateriaisItem item)
        {
            item.DataCriacao = DateTime.UtcNow;
            
            // Buscar custo atual do produto
            var produto = await _context.Produtos.FindAsync(item.ProdutoId);
            if (produto != null)
            {
                item.CustoUnitario = produto.PrecoCusto;
            }

            _context.ListaMateriaisItens.Add(item);
            await _context.SaveChangesAsync();

            // Atualizar custo total da lista
            await AtualizarCustoTotalAsync(item.ListaMateriaisId);

            return item;
        }

        public async Task<ListaMateriaisItem> AtualizarItemAsync(ListaMateriaisItem item)
        {
            _context.ListaMateriaisItens.Update(item);
            await _context.SaveChangesAsync();

            // Atualizar custo total da lista
            await AtualizarCustoTotalAsync(item.ListaMateriaisId);

            return item;
        }

        public async Task<bool> RemoverItemAsync(int itemId, int empresaId)
        {
            var item = await _context.ListaMateriaisItens
                .Include(i => i.ListaMateriais)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.ListaMateriais.EmpresaId == empresaId);

            if (item == null)
                return false;

            var listaMateriaisId = item.ListaMateriaisId;
            
            _context.ListaMateriaisItens.Remove(item);
            await _context.SaveChangesAsync();

            // Atualizar custo total da lista
            await AtualizarCustoTotalAsync(listaMateriaisId);

            return true;
        }

        public async Task<decimal> CalcularCustoTotalAsync(int id, int empresaId)
        {
            var listaMateriais = await _context.ListasMateriais
                .Include(lm => lm.Itens)
                .FirstOrDefaultAsync(lm => lm.Id == id && lm.EmpresaId == empresaId);

            if (listaMateriais == null)
                return 0;

            return listaMateriais.Itens.Sum(i => i.CustoTotal);
        }

        public async Task<decimal> CalcularCustoProducaoAsync(int produtoId, decimal quantidade, int empresaId)
        {
            var listaMateriais = await ObterPorProdutoAsync(produtoId, empresaId);
            if (listaMateriais == null)
                return 0;

            var fatorQuantidade = quantidade / listaMateriais.QuantidadeBase;
            return listaMateriais.Itens.Sum(i => i.CustoTotal * fatorQuantidade);
        }

        public async Task<bool> ValidarDisponibilidadeMateriaisAsync(int produtoId, decimal quantidade, int empresaId)
        {
            var materiaisInsuficientes = await ObterMateriaisInsuficientesAsync(produtoId, quantidade, empresaId);
            return !materiaisInsuficientes.Any();
        }

        public async Task<IEnumerable<(int ProdutoId, string Descricao, decimal QuantidadeNecessaria, decimal EstoqueAtual)>> 
            ObterMateriaisInsuficientesAsync(int produtoId, decimal quantidade, int empresaId)
        {
            var listaMateriais = await ObterPorProdutoAsync(produtoId, empresaId);
            if (listaMateriais == null)
                return Enumerable.Empty<(int, string, decimal, decimal)>();

            var materiaisInsuficientes = new List<(int, string, decimal, decimal)>();
            var fatorQuantidade = quantidade / listaMateriais.QuantidadeBase;

            foreach (var item in listaMateriais.Itens)
            {
                var quantidadeNecessaria = item.QuantidadeLiquida * fatorQuantidade;
                
                if (item.Produto.ControlaEstoque && item.Produto.EstoqueAtual < quantidadeNecessaria)
                {
                    materiaisInsuficientes.Add((
                        item.ProdutoId,
                        item.Produto.Descricao,
                        quantidadeNecessaria,
                        item.Produto.EstoqueAtual
                    ));
                }
            }

            return materiaisInsuficientes;
        }

        public async Task<IEnumerable<(int ProdutoId, string Descricao, decimal QuantidadeTotal, string Unidade)>> 
            ExplodirMateriaisAsync(int produtoId, decimal quantidade, int empresaId)
        {
            var materiais = new Dictionary<int, (string Descricao, decimal QuantidadeTotal, string Unidade)>();
            
            await ExplodirMateriaisRecursivoAsync(produtoId, quantidade, empresaId, materiais);
            
            return materiais.Select(m => (m.Key, m.Value.Descricao, m.Value.QuantidadeTotal, m.Value.Unidade));
        }

        private async Task ExplodirMateriaisRecursivoAsync(int produtoId, decimal quantidade, int empresaId, 
            Dictionary<int, (string Descricao, decimal QuantidadeTotal, string Unidade)> materiais)
        {
            var listaMateriais = await ObterPorProdutoAsync(produtoId, empresaId);
            if (listaMateriais == null)
                return;

            var fatorQuantidade = quantidade / listaMateriais.QuantidadeBase;

            foreach (var item in listaMateriais.Itens)
            {
                var quantidadeNecessaria = item.QuantidadeLiquida * fatorQuantidade;

                // Verificar se o item também tem lista de materiais (subconjunto)
                var temSubLista = await _context.ListasMateriais
                    .AnyAsync(lm => lm.ProdutoId == item.ProdutoId && lm.EmpresaId == empresaId && lm.Ativo);

                if (temSubLista)
                {
                    // Explodir recursivamente
                    await ExplodirMateriaisRecursivoAsync(item.ProdutoId, quantidadeNecessaria, empresaId, materiais);
                }
                else
                {
                    // É um material final, adicionar à lista
                    if (materiais.ContainsKey(item.ProdutoId))
                    {
                        var materialExistente = materiais[item.ProdutoId];
                        materiais[item.ProdutoId] = (
                            materialExistente.Descricao,
                            materialExistente.QuantidadeTotal + quantidadeNecessaria,
                            materialExistente.Unidade
                        );
                    }
                    else
                    {
                        materiais[item.ProdutoId] = (
                            item.Produto.Descricao,
                            quantidadeNecessaria,
                            item.UnidadeMedida
                        );
                    }
                }
            }
        }

        private async Task AtualizarCustoTotalAsync(int listaMateriaisId)
        {
            var listaMateriais = await _context.ListasMateriais
                .Include(lm => lm.Itens)
                .FirstOrDefaultAsync(lm => lm.Id == listaMateriaisId);

            if (listaMateriais != null)
            {
                listaMateriais.CustoTotal = listaMateriais.Itens.Sum(i => i.CustoTotal);
                listaMateriais.DataAtualizacao = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}