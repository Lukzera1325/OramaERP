using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

/// <summary>
/// Serviço para controle de estoque
/// </summary>
public class EstoqueService : IEstoqueService
{
    private readonly OramaDbContext _context;

    public EstoqueService(OramaDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém todas as movimentações de estoque por empresa
    /// </summary>
    public async Task<IEnumerable<MovimentacaoEstoque>> ObterMovimentacoesAsync(int empresaId)
    {
        return await _context.MovimentacoesEstoque
            .Include(m => m.Produto)
            .Include(m => m.Usuario)
            .Where(m => m.EmpresaId == empresaId && m.Ativo)
            .OrderByDescending(m => m.DataMovimentacao)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém movimentações de estoque por produto
    /// </summary>
    public async Task<IEnumerable<MovimentacaoEstoque>> ObterMovimentacoesPorProdutoAsync(int empresaId, int produtoId)
    {
        return await _context.MovimentacoesEstoque
            .Include(m => m.Produto)
            .Include(m => m.Usuario)
            .Where(m => m.EmpresaId == empresaId && m.ProdutoId == produtoId && m.Ativo)
            .OrderByDescending(m => m.DataMovimentacao)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém movimentações de estoque por período
    /// </summary>
    public async Task<IEnumerable<MovimentacaoEstoque>> ObterMovimentacoesPorPeriodoAsync(int empresaId, DateTime inicio, DateTime fim)
    {
        return await _context.MovimentacoesEstoque
            .Include(m => m.Produto)
            .Include(m => m.Usuario)
            .Where(m => m.EmpresaId == empresaId && 
                       m.DataMovimentacao >= inicio && 
                       m.DataMovimentacao <= fim && 
                       m.Ativo)
            .OrderByDescending(m => m.DataMovimentacao)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém movimentações de estoque por tipo
    /// </summary>
    public async Task<IEnumerable<MovimentacaoEstoque>> ObterMovimentacoesPorTipoAsync(int empresaId, TipoMovimentacaoEstoque tipo)
    {
        return await _context.MovimentacoesEstoque
            .Include(m => m.Produto)
            .Include(m => m.Usuario)
            .Where(m => m.EmpresaId == empresaId && m.Tipo == tipo && m.Ativo)
            .OrderByDescending(m => m.DataMovimentacao)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém posição atual do estoque por empresa
    /// </summary>
    public async Task<IEnumerable<Produto>> ObterPosicaoEstoqueAsync(int empresaId)
    {
        return await _context.Produtos
            .Include(p => p.CategoriaNavigation)
            .Where(p => p.EmpresaId == empresaId && p.ControlaEstoque && p.Ativo)
            .OrderBy(p => p.Descricao)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém produtos com estoque baixo
    /// </summary>
    public async Task<IEnumerable<Produto>> ObterProdutosEstoqueBaixoAsync(int empresaId)
    {
        return await _context.Produtos
            .Include(p => p.CategoriaNavigation)
            .Where(p => p.EmpresaId == empresaId && 
                       p.ControlaEstoque && 
                       p.EstoqueAtual <= p.EstoqueMinimo && 
                       p.Ativo)
            .OrderBy(p => p.Descricao)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém produtos sem movimentação
    /// </summary>
    public async Task<IEnumerable<Produto>> ObterProdutosSemMovimentacaoAsync(int empresaId, int dias = 30)
    {
        var dataLimite = DateTime.Now.AddDays(-dias);
        
        var produtosComMovimentacao = await _context.MovimentacoesEstoque
            .Where(m => m.EmpresaId == empresaId && m.DataMovimentacao >= dataLimite && m.Ativo)
            .Select(m => m.ProdutoId)
            .Distinct()
            .ToListAsync();

        return await _context.Produtos
            .Include(p => p.CategoriaNavigation)
            .Where(p => p.EmpresaId == empresaId && 
                       p.ControlaEstoque && 
                       !produtosComMovimentacao.Contains(p.Id) && 
                       p.Ativo)
            .OrderBy(p => p.Descricao)
            .ToListAsync();
    }

    /// <summary>
    /// Registra uma movimentação de estoque
    /// </summary>
    public async Task<MovimentacaoEstoque> RegistrarMovimentacaoAsync(MovimentacaoEstoque movimentacao)
    {
        // Obter produto para validações
        var produto = await _context.Produtos.FindAsync(movimentacao.ProdutoId);
        if (produto == null || !produto.ControlaEstoque)
            throw new InvalidOperationException("Produto não encontrado ou não controla estoque");

        // Registrar estoque anterior
        movimentacao.EstoqueAnterior = produto.EstoqueAtual;

        // Calcular novo estoque
        var novoEstoque = produto.EstoqueAtual;
        if (IsEntrada(movimentacao.Tipo))
            novoEstoque += movimentacao.Quantidade;
        else
            novoEstoque -= movimentacao.Quantidade;

        // Validar estoque negativo
        if (novoEstoque < 0)
            throw new InvalidOperationException($"Operação resultaria em estoque negativo. Estoque atual: {produto.EstoqueAtual}");

        // Atualizar estoque do produto
        produto.EstoqueAtual = novoEstoque;
        movimentacao.EstoquePosterior = novoEstoque;

        // Registrar movimentação
        movimentacao.Ativo = true;
        _context.MovimentacoesEstoque.Add(movimentacao);
        
        await _context.SaveChangesAsync();
        return movimentacao;
    }

    /// <summary>
    /// Registra entrada de estoque
    /// </summary>
    public async Task<MovimentacaoEstoque> RegistrarEntradaAsync(int empresaId, int produtoId, decimal quantidade, 
        TipoMovimentacaoEstoque tipo, string motivo, decimal? custoUnitario = null, 
        int? referenciaId = null, int? usuarioId = null)
    {
        if (!IsEntrada(tipo))
            throw new ArgumentException("Tipo de movimentação deve ser de entrada");

        var movimentacao = new MovimentacaoEstoque
        {
            EmpresaId = empresaId,
            ProdutoId = produtoId,
            Tipo = tipo,
            Quantidade = quantidade,
            CustoUnitario = custoUnitario,
            Motivo = motivo,
            UsuarioId = usuarioId,
            DataMovimentacao = DateTime.Now
        };

        // Definir referência baseada no tipo
        switch (tipo)
        {
            case TipoMovimentacaoEstoque.EntradaCompra:
                movimentacao.CompraId = referenciaId;
                break;
        }

        return await RegistrarMovimentacaoAsync(movimentacao);
    }

    /// <summary>
    /// Registra saída de estoque
    /// </summary>
    public async Task<MovimentacaoEstoque> RegistrarSaidaAsync(int empresaId, int produtoId, decimal quantidade, 
        TipoMovimentacaoEstoque tipo, string motivo, int? referenciaId = null, int? usuarioId = null)
    {
        if (IsEntrada(tipo))
            throw new ArgumentException("Tipo de movimentação deve ser de saída");

        var movimentacao = new MovimentacaoEstoque
        {
            EmpresaId = empresaId,
            ProdutoId = produtoId,
            Tipo = tipo,
            Quantidade = quantidade,
            Motivo = motivo,
            UsuarioId = usuarioId,
            DataMovimentacao = DateTime.Now
        };

        // Definir referência baseada no tipo
        switch (tipo)
        {
            case TipoMovimentacaoEstoque.SaidaVenda:
                movimentacao.VendaId = referenciaId;
                break;
        }

        return await RegistrarMovimentacaoAsync(movimentacao);
    }

    /// <summary>
    /// Realiza ajuste de estoque (inventário)
    /// </summary>
    public async Task<MovimentacaoEstoque?> AjustarEstoqueAsync(int empresaId, int produtoId, decimal quantidadeReal, 
        string motivo, int usuarioId)
    {
        var produto = await _context.Produtos.FindAsync(produtoId);
        if (produto == null || produto.EmpresaId != empresaId)
            throw new InvalidOperationException("Produto não encontrado");

        var diferenca = quantidadeReal - produto.EstoqueAtual;
        if (diferenca == 0)
            return null; // Sem diferença, não precisa ajustar

        var tipo = diferenca > 0 ? TipoMovimentacaoEstoque.AjustePositivo : TipoMovimentacaoEstoque.AjusteNegativo;
        
        return await RegistrarMovimentacaoAsync(new MovimentacaoEstoque
        {
            EmpresaId = empresaId,
            ProdutoId = produtoId,
            Tipo = tipo,
            Quantidade = Math.Abs(diferenca),
            Motivo = motivo,
            UsuarioId = usuarioId,
            Observacoes = $"Ajuste de inventário. Estoque anterior: {produto.EstoqueAtual}, Contagem física: {quantidadeReal}",
            DataMovimentacao = DateTime.Now
        });
    }

    /// <summary>
    /// Realiza transferência entre produtos
    /// </summary>
    public async Task<IEnumerable<MovimentacaoEstoque>> TransferirEstoqueAsync(int empresaId, int produtoOrigemId, 
        int produtoDestinoId, decimal quantidade, string motivo, int usuarioId)
    {
        var movimentacoes = new List<MovimentacaoEstoque>();

        // Saída do produto origem
        var saida = await RegistrarSaidaAsync(empresaId, produtoOrigemId, quantidade, 
            TipoMovimentacaoEstoque.Transferencia, $"Transferência para produto {produtoDestinoId}: {motivo}", 
            null, usuarioId);
        movimentacoes.Add(saida);

        // Entrada no produto destino
        var entrada = await RegistrarEntradaAsync(empresaId, produtoDestinoId, quantidade, 
            TipoMovimentacaoEstoque.Transferencia, $"Transferência do produto {produtoOrigemId}: {motivo}", 
            null, null, usuarioId);
        movimentacoes.Add(entrada);

        return movimentacoes;
    }

    /// <summary>
    /// Obtém histórico de um produto específico
    /// </summary>
    public async Task<IEnumerable<MovimentacaoEstoque>> ObterHistoricoProdutoAsync(int empresaId, int produtoId, 
        DateTime? inicio = null, DateTime? fim = null)
    {
        var query = _context.MovimentacoesEstoque
            .Include(m => m.Usuario)
            .Where(m => m.EmpresaId == empresaId && m.ProdutoId == produtoId && m.Ativo);

        if (inicio.HasValue)
            query = query.Where(m => m.DataMovimentacao >= inicio.Value);
        if (fim.HasValue)
            query = query.Where(m => m.DataMovimentacao <= fim.Value);

        return await query
            .OrderByDescending(m => m.DataMovimentacao)
            .ToListAsync();
    }

    /// <summary>
    /// Calcula valor total do estoque
    /// </summary>
    public async Task<decimal> CalcularValorTotalEstoqueAsync(int empresaId)
    {
        var produtos = await _context.Produtos
            .Where(p => p.EmpresaId == empresaId && p.ControlaEstoque && p.Ativo)
            .ToListAsync();

        return produtos.Sum(p => p.EstoqueAtual * p.PrecoCusto);
    }

    /// <summary>
    /// Obtém relatório de movimentações por período
    /// </summary>
    public async Task<object> ObterRelatorioMovimentacoesAsync(int empresaId, DateTime inicio, DateTime fim)
    {
        var movimentacoes = await ObterMovimentacoesPorPeriodoAsync(empresaId, inicio, fim);
        
        return new
        {
            Periodo = new { Inicio = inicio, Fim = fim },
            TotalMovimentacoes = movimentacoes.Count(),
            Entradas = movimentacoes.Where(m => IsEntrada(m.Tipo)).Sum(m => m.Quantidade),
            Saidas = movimentacoes.Where(m => !IsEntrada(m.Tipo)).Sum(m => m.Quantidade),
            PorTipo = movimentacoes.GroupBy(m => m.Tipo)
                .Select(g => new { Tipo = g.Key.ToString(), Quantidade = g.Sum(m => m.Quantidade) })
                .ToList(),
            PorProduto = movimentacoes.GroupBy(m => new { m.ProdutoId, m.Produto.Descricao })
                .Select(g => new { 
                    ProdutoId = g.Key.ProdutoId, 
                    Produto = g.Key.Descricao, 
                    Movimentacoes = g.Count(),
                    Quantidade = g.Sum(m => IsEntrada(m.Tipo) ? m.Quantidade : -m.Quantidade)
                })
                .OrderByDescending(x => x.Movimentacoes)
                .ToList()
        };
    }

    /// <summary>
    /// Obtém relatório de posição de estoque
    /// </summary>
    public async Task<object> ObterRelatorioPosicaoEstoqueAsync(int empresaId)
    {
        var produtos = await ObterPosicaoEstoqueAsync(empresaId);
        var valorTotal = await CalcularValorTotalEstoqueAsync(empresaId);
        
        return new
        {
            DataRelatorio = DateTime.Now,
            TotalProdutos = produtos.Count(),
            ValorTotalEstoque = valorTotal,
            ProdutosEstoqueBaixo = produtos.Count(p => p.EstoqueAtual <= p.EstoqueMinimo),
            ProdutosSemEstoque = produtos.Count(p => p.EstoqueAtual <= 0),
            Produtos = produtos.Select(p => new
            {
                p.Id,
                p.Codigo,
                Nome = p.Descricao,
                Categoria = p.CategoriaNavigation?.Nome,
                EstoqueAtual = p.EstoqueAtual,
                EstoqueMinimo = p.EstoqueMinimo,
                EstoqueMaximo = p.EstoqueMaximo,
                PrecoCusto = p.PrecoCusto,
                ValorEstoque = p.EstoqueAtual * p.PrecoCusto,
                Status = p.EstoqueAtual <= 0 ? "Sem Estoque" : 
                        p.EstoqueAtual <= p.EstoqueMinimo ? "Estoque Baixo" : "Normal"
            }).OrderBy(p => p.Nome).ToList()
        };
    }

    /// <summary>
    /// Valida se há estoque suficiente para uma operação
    /// </summary>
    public async Task<bool> ValidarEstoqueDisponivelAsync(int empresaId, int produtoId, decimal quantidade)
    {
        var produto = await _context.Produtos
            .FirstOrDefaultAsync(p => p.Id == produtoId && p.EmpresaId == empresaId && p.Ativo);
        
        if (produto == null || !produto.ControlaEstoque)
            return true; // Se não controla estoque, sempre disponível

        return produto.EstoqueAtual >= quantidade;
    }

    /// <summary>
    /// Obtém produtos para inventário (contagem física)
    /// </summary>
    public async Task<IEnumerable<Produto>> ObterProdutosParaInventarioAsync(int empresaId)
    {
        return await _context.Produtos
            .Include(p => p.CategoriaNavigation)
            .Where(p => p.EmpresaId == empresaId && p.ControlaEstoque && p.Ativo)
            .OrderBy(p => p.CategoriaNavigation.Nome)
            .ThenBy(p => p.Descricao)
            .ToListAsync();
    }

    /// <summary>
    /// Processa inventário físico
    /// </summary>
    public async Task<IEnumerable<MovimentacaoEstoque>> ProcessarInventarioAsync(int empresaId, 
        Dictionary<int, decimal> contagemFisica, int usuarioId, string? observacoes = null)
    {
        var movimentacoes = new List<MovimentacaoEstoque>();

        foreach (var item in contagemFisica)
        {
            var produtoId = item.Key;
            var quantidadeContada = item.Value;

            var movimentacao = await AjustarEstoqueAsync(empresaId, produtoId, quantidadeContada, 
                "Inventário físico", usuarioId);
            
            if (movimentacao != null)
            {
                if (!string.IsNullOrEmpty(observacoes))
                    movimentacao.Observacoes += $" | {observacoes}";
                
                movimentacoes.Add(movimentacao);
            }
        }

        await _context.SaveChangesAsync();
        return movimentacoes;
    }

    /// <summary>
    /// Verifica se o tipo de movimentação é entrada
    /// </summary>
    private static bool IsEntrada(TipoMovimentacaoEstoque tipo)
    {
        return tipo == TipoMovimentacaoEstoque.EntradaCompra ||
               tipo == TipoMovimentacaoEstoque.AjustePositivo ||
               tipo == TipoMovimentacaoEstoque.Devolucao ||
               tipo == TipoMovimentacaoEstoque.Transferencia; // Transferência pode ser entrada ou saída
    }
}