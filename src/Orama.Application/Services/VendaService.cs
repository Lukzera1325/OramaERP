using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Domain.Services;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

/// <summary>
/// Application Service para Vendas - Focado em CRUD e coordenação
/// Lógica de negócio complexa está no VendaProcessingService (Domain)
/// </summary>
public class VendaService : IVendaService
{
    private readonly OramaDbContext _context;
    private readonly VendaProcessingService _vendaProcessingService;

    public VendaService(OramaDbContext context, VendaProcessingService vendaProcessingService)
    {
        _context = context;
        _vendaProcessingService = vendaProcessingService;
    }

    // CRUD Básico - Simples e Direto

    public async Task<IEnumerable<Venda>> ObterTodosAsync(int empresaId)
    {
        return await _context.Vendas
            .Include(v => v.Cliente)
            .Include(v => v.Vendedor)
            .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
            .Where(v => v.EmpresaId == empresaId && v.Ativo)
            .OrderByDescending(v => v.DataVenda)
            .ToListAsync();
    }

    public async Task<Venda?> ObterPorIdAsync(int id, int empresaId)
    {
        return await _context.Vendas
            .Include(v => v.Cliente)
            .Include(v => v.Vendedor)
            .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(v => v.Id == id && v.EmpresaId == empresaId);
    }

    public async Task<IEnumerable<Venda>> ObterPorStatusAsync(int empresaId, StatusVenda status)
    {
        return await _context.Vendas
            .Include(v => v.Cliente)
            .Where(v => v.EmpresaId == empresaId && v.Status == status && v.Ativo)
            .OrderByDescending(v => v.DataVenda)
            .ToListAsync();
    }

    public async Task<IEnumerable<Venda>> ObterPorClienteAsync(int empresaId, int clienteId)
    {
        return await _context.Vendas
            .Include(v => v.Cliente)
            .Where(v => v.EmpresaId == empresaId && v.ClienteId == clienteId && v.Ativo)
            .OrderByDescending(v => v.DataVenda)
            .ToListAsync();
    }

    public async Task<IEnumerable<Venda>> ObterPorPeriodoAsync(int empresaId, DateTime inicio, DateTime fim)
    {
        return await _context.Vendas
            .Include(v => v.Cliente)
            .Where(v => v.EmpresaId == empresaId && v.DataVenda >= inicio && v.DataVenda <= fim && v.Ativo)
            .OrderByDescending(v => v.DataVenda)
            .ToListAsync();
    }

    // Operações Simples

    public async Task<string> GerarNumeroAsync(int empresaId)
    {
        var ultimaVenda = await _context.Vendas
            .Where(v => v.EmpresaId == empresaId)
            .OrderByDescending(v => v.Id)
            .FirstOrDefaultAsync();

        int proximoNumero = 1;
        if (ultimaVenda != null && int.TryParse(ultimaVenda.Numero, out int numero))
        {
            proximoNumero = numero + 1;
        }
        return proximoNumero.ToString("D6");
    }

    public async Task<Venda> CriarAsync(Venda venda)
    {
        // Gerar número da venda
        venda.Numero = await GerarNumeroAsync(venda.EmpresaId);
        venda.DataCriacao = DateTime.Now;
        venda.Ativo = true;

        // Usar método do Domain para calcular totais
        venda.CalcularTotais();

        _context.Vendas.Add(venda);
        await _context.SaveChangesAsync();
        return venda;
    }

    public async Task<Venda> AtualizarAsync(Venda venda)
    {
        var vendaExistente = await _context.Vendas
            .Include(v => v.Itens)
            .FirstOrDefaultAsync(v => v.Id == venda.Id && v.EmpresaId == venda.EmpresaId);

        if (vendaExistente == null)
            throw new InvalidOperationException("Venda não encontrada");

        // Usar método do Domain para validar se pode ser editada
        if (!vendaExistente.PodeSerEditada)
            throw new InvalidOperationException("Venda não pode ser editada no status atual");

        // Atualizar dados básicos
        vendaExistente.ClienteId = venda.ClienteId;
        vendaExistente.VendedorId = venda.VendedorId;
        vendaExistente.DataVenda = venda.DataVenda;
        vendaExistente.DataEntrega = venda.DataEntrega;
        vendaExistente.FormaPagamento = venda.FormaPagamento;
        vendaExistente.Parcelas = venda.Parcelas;
        vendaExistente.PercentualDesconto = venda.PercentualDesconto;
        vendaExistente.ValorFrete = venda.ValorFrete;
        vendaExistente.Observacoes = venda.Observacoes;
        vendaExistente.DataAtualizacao = DateTime.Now;

        // Atualizar itens (remover antigos e adicionar novos)
        _context.VendaItens.RemoveRange(vendaExistente.Itens);
        
        foreach (var item in venda.Itens)
        {
            item.VendaId = vendaExistente.Id;
            item.DataCriacao = DateTime.Now;
            item.Ativo = true;
            item.CalcularTotal(); // Usar método do Domain
            vendaExistente.Itens.Add(item);
        }

        // Recalcular totais usando método do Domain
        vendaExistente.CalcularTotais();

        await _context.SaveChangesAsync();
        return vendaExistente;
    }

    public async Task ExcluirAsync(int id, int empresaId)
    {
        var venda = await _context.Vendas.FirstOrDefaultAsync(v => v.Id == id && v.EmpresaId == empresaId);
        if (venda == null)
            throw new InvalidOperationException("Venda não encontrada");

        if (!venda.PodeSerEditada)
            throw new InvalidOperationException("Venda não pode ser excluída no status atual");

        venda.Ativo = false;
        venda.DataAtualizacao = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    // Operações de Negócio - Delegam para o Domain Service

    public async Task<Venda> AprovarAsync(int id, int empresaId)
    {
        var venda = await ObterPorIdAsync(id, empresaId);
        if (venda == null)
            throw new InvalidOperationException("Venda não encontrada");

        // Usar método do Domain
        venda.Aprovar();

        await _context.SaveChangesAsync();
        return venda;
    }

    public async Task<Venda> FaturarAsync(int id, int empresaId)
    {
        var venda = await ObterPorIdAsync(id, empresaId);
        if (venda == null)
            throw new InvalidOperationException("Venda não encontrada");

        // Buscar produtos para validação
        var produtoIds = venda.Itens.Select(i => i.ProdutoId).ToList();
        var produtos = await _context.Produtos
            .Where(p => produtoIds.Contains(p.Id) && p.EmpresaId == empresaId)
            .ToListAsync();

        // Usar Domain Service para processar faturamento
        var resultado = _vendaProcessingService.ProcessarFaturamento(venda, produtos);

        // Persistir mudanças
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Salvar venda faturada
            await _context.SaveChangesAsync();

            // Salvar movimentações de estoque
            if (resultado.MovimentacoesEstoque.Any())
            {
                _context.MovimentacoesEstoque.AddRange(resultado.MovimentacoesEstoque);
                
                // Atualizar estoque dos produtos
                foreach (var movimentacao in resultado.MovimentacoesEstoque)
                {
                    var produto = produtos.First(p => p.Id == movimentacao.ProdutoId);
                    produto.EstoqueAtual -= movimentacao.Quantidade;
                }
            }

            // Salvar contas a receber
            if (resultado.ContasReceber.Any())
            {
                _context.ContasReceber.AddRange(resultado.ContasReceber);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return resultado.VendaFaturada;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Venda> CancelarAsync(int id, int empresaId, string motivo = "")
    {
        var venda = await ObterPorIdAsync(id, empresaId);
        if (venda == null)
            throw new InvalidOperationException("Venda não encontrada");

        // Usar Domain Service para processar cancelamento
        var resultado = _vendaProcessingService.ProcessarCancelamento(venda, motivo);

        // Persistir mudanças
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Salvar venda cancelada
            await _context.SaveChangesAsync();

            // Processar estorno de estoque se necessário
            if (resultado.MovimentacoesEstoque.Any())
            {
                _context.MovimentacoesEstoque.AddRange(resultado.MovimentacoesEstoque);
                
                // Atualizar estoque dos produtos
                var produtoIds = resultado.MovimentacoesEstoque.Select(m => m.ProdutoId).ToList();
                var produtos = await _context.Produtos
                    .Where(p => produtoIds.Contains(p.Id) && p.EmpresaId == empresaId)
                    .ToListAsync();

                foreach (var movimentacao in resultado.MovimentacoesEstoque)
                {
                    var produto = produtos.First(p => p.Id == movimentacao.ProdutoId);
                    produto.EstoqueAtual += movimentacao.Quantidade;
                }
            }

            // Cancelar contas a receber se necessário
            if (resultado.ContasReceberParaCancelar.Any())
            {
                var contas = await _context.ContasReceber
                    .Where(c => resultado.ContasReceberParaCancelar.Contains(c.Id))
                    .ToListAsync();

                foreach (var conta in contas)
                {
                    conta.Status = StatusConta.Cancelada;
                    conta.DataAtualizacao = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return resultado.VendaCancelada;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Consultas e Relatórios

    public async Task<decimal> ObterTotalVendasAsync(int empresaId, DateTime? inicio = null, DateTime? fim = null)
    {
        var query = _context.Vendas
            .Where(v => v.EmpresaId == empresaId && v.Status == StatusVenda.Faturada && v.Ativo);

        if (inicio.HasValue)
            query = query.Where(v => v.DataVenda >= inicio.Value);
        if (fim.HasValue)
            query = query.Where(v => v.DataVenda <= fim.Value);

        return await query.SumAsync(v => v.ValorTotal);
    }

    public async Task<IEnumerable<dynamic>> ObterProdutosMaisVendidosAsync(int empresaId, DateTime inicio, DateTime fim, int limite = 10)
    {
        return await _context.VendaItens
            .Include(vi => vi.Produto)
            .Include(vi => vi.Venda)
            .Where(vi => vi.Venda.EmpresaId == empresaId && 
                        vi.Venda.Status == StatusVenda.Faturada &&
                        vi.Venda.DataVenda >= inicio && 
                        vi.Venda.DataVenda <= fim &&
                        vi.Venda.Ativo)
            .GroupBy(vi => new { vi.ProdutoId, vi.Produto.Descricao })
            .Select(g => new
            {
                ProdutoId = g.Key.ProdutoId,
                Nome = g.Key.Descricao,
                QuantidadeVendida = g.Sum(vi => vi.Quantidade),
                ValorTotal = g.Sum(vi => vi.ValorTotal)
            })
            .OrderByDescending(p => p.QuantidadeVendida)
            .Take(limite)
            .ToListAsync();
    }
}
