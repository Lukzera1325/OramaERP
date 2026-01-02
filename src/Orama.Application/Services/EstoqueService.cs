using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

/// <summary>
/// Serviço simplificado para controle de estoque
/// Um caso de uso = um método
/// </summary>
public class EstoqueService : IEstoqueService
{
    private readonly OramaDbContext _context;

    public EstoqueService(OramaDbContext context)
    {
        _context = context;
    }

    // Caso de uso: Consultar posição de estoque
    public async Task<IEnumerable<Produto>> ObterPosicaoEstoqueAsync(int empresaId)
    {
        return await _context.Produtos
            .Where(p => p.EmpresaId == empresaId && p.ControlaEstoque)
            .OrderBy(p => p.Descricao)
            .ToListAsync();
    }

    // Caso de uso: Identificar produtos com estoque baixo
    public async Task<IEnumerable<Produto>> ObterProdutosEstoqueBaixoAsync(int empresaId)
    {
        return await _context.Produtos
            .Where(p => p.EmpresaId == empresaId && p.ControlaEstoque && p.EstoqueAtual <= p.EstoqueMinimo)
            .OrderBy(p => p.Descricao)
            .ToListAsync();
    }

    // Caso de uso: Ajustar estoque de um produto
    public async Task<bool> AjustarEstoqueAsync(int produtoId, decimal novoEstoque, string motivo, int empresaId, int usuarioId)
    {
        var produto = await _context.Produtos
            .FirstOrDefaultAsync(p => p.Id == produtoId && p.EmpresaId == empresaId);

        if (produto == null) return false;

        var estoqueAnterior = produto.EstoqueAtual;
        
        // Usar método de negócio da entidade
        produto.AjustarEstoque(novoEstoque, motivo);

        // Registrar movimentação
        var movimentacao = new MovimentacaoEstoque
        {
            EmpresaId = empresaId,
            ProdutoId = produtoId,
            Tipo = TipoMovimentacaoEstoque.AjustePositivo,
            Quantidade = novoEstoque - estoqueAnterior,
            EstoqueAnterior = estoqueAnterior,
            EstoquePosterior = novoEstoque,
            DataMovimentacao = DateTime.Now,
            Motivo = motivo,
            UsuarioId = usuarioId
        };

        _context.MovimentacoesEstoque.Add(movimentacao);
        await _context.SaveChangesAsync();

        return true;
    }

    // Caso de uso: Dar entrada no estoque
    public async Task<bool> EntradaEstoqueAsync(int produtoId, decimal quantidade, string motivo, int empresaId, int usuarioId)
    {
        var produto = await _context.Produtos
            .FirstOrDefaultAsync(p => p.Id == produtoId && p.EmpresaId == empresaId);

        if (produto == null) return false;

        var estoqueAnterior = produto.EstoqueAtual;
        
        // Usar método de negócio da entidade
        produto.AdicionarEstoque(quantidade, motivo);

        // Registrar movimentação
        var movimentacao = new MovimentacaoEstoque
        {
            EmpresaId = empresaId,
            ProdutoId = produtoId,
            Tipo = TipoMovimentacaoEstoque.EntradaCompra,
            Quantidade = quantidade,
            EstoqueAnterior = estoqueAnterior,
            EstoquePosterior = produto.EstoqueAtual,
            DataMovimentacao = DateTime.Now,
            Motivo = motivo,
            UsuarioId = usuarioId
        };

        _context.MovimentacoesEstoque.Add(movimentacao);
        await _context.SaveChangesAsync();

        return true;
    }

    // Caso de uso: Dar saída no estoque
    public async Task<bool> SaidaEstoqueAsync(int produtoId, decimal quantidade, string motivo, int empresaId, int usuarioId)
    {
        var produto = await _context.Produtos
            .FirstOrDefaultAsync(p => p.Id == produtoId && p.EmpresaId == empresaId);

        if (produto == null) return false;

        var estoqueAnterior = produto.EstoqueAtual;
        
        // Usar método de negócio da entidade
        produto.RemoverEstoque(quantidade, motivo);

        // Registrar movimentação
        var movimentacao = new MovimentacaoEstoque
        {
            EmpresaId = empresaId,
            ProdutoId = produtoId,
            Tipo = TipoMovimentacaoEstoque.SaidaVenda,
            Quantidade = -quantidade,
            EstoqueAnterior = estoqueAnterior,
            EstoquePosterior = produto.EstoqueAtual,
            DataMovimentacao = DateTime.Now,
            Motivo = motivo,
            UsuarioId = usuarioId
        };

        _context.MovimentacoesEstoque.Add(movimentacao);
        await _context.SaveChangesAsync();

        return true;
    }

    // Caso de uso: Consultar histórico de movimentações
    public async Task<IEnumerable<MovimentacaoEstoque>> ObterHistoricoAsync(int empresaId, int? produtoId = null)
    {
        var query = _context.MovimentacoesEstoque
            .Include(m => m.Produto)
            .Where(m => m.EmpresaId == empresaId);

        if (produtoId.HasValue)
            query = query.Where(m => m.ProdutoId == produtoId.Value);

        return await query
            .OrderByDescending(m => m.DataMovimentacao)
            .Take(100) // Limitar para performance
            .ToListAsync();
    }

    // Caso de uso: Calcular valor total do estoque
    public async Task<decimal> CalcularValorTotalEstoqueAsync(int empresaId)
    {
        return await _context.Produtos
            .Where(p => p.EmpresaId == empresaId && p.ControlaEstoque)
            .SumAsync(p => p.EstoqueAtual * p.PrecoCusto);
    }

    // Caso de uso: Fazer inventário
    public async Task<IEnumerable<Produto>> ObterProdutosParaInventarioAsync(int empresaId)
    {
        return await _context.Produtos
            .Where(p => p.EmpresaId == empresaId && p.ControlaEstoque)
            .OrderBy(p => p.Codigo)
            .ToListAsync();
    }

    // Caso de uso: Processar inventário
    public async Task<bool> ProcessarInventarioAsync(Dictionary<int, decimal> contagemInventario, int empresaId, int usuarioId)
    {
        foreach (var item in contagemInventario)
        {
            var produtoId = item.Key;
            var estoqueContado = item.Value;

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == produtoId && p.EmpresaId == empresaId);

            if (produto == null) continue;

            var diferenca = estoqueContado - produto.EstoqueAtual;
            
            if (diferenca != 0)
            {
                await AjustarEstoqueAsync(produtoId, estoqueContado, "Inventário", empresaId, usuarioId);
            }
        }

        return true;
    }
}