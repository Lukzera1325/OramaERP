using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

public class VendaService : IVendaService
{
    private readonly OramaDbContext _context;
    private readonly IContaReceberService _contaReceberService;
    private readonly IProdutoService _produtoService;

    public VendaService(OramaDbContext context, IContaReceberService contaReceberService, IProdutoService produtoService)
    {
        _context = context;
        _contaReceberService = contaReceberService;
        _produtoService = produtoService;
    }

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

    public async Task<Venda> IncluirAsync(Venda venda)
    {
        venda.Numero = await GerarNumeroAsync(venda.EmpresaId);
        venda.DataCriacao = DateTime.Now;
        venda.Ativo = true;

        // Calcular totais
        CalcularTotais(venda);

        // Atualizar estoque se faturada
        if (venda.Status == StatusVenda.Faturada)
        {
            await AtualizarEstoque(venda, false);
        }

        _context.Vendas.Add(venda);
        await _context.SaveChangesAsync();
        return venda;
    }

    public async Task<Venda> CriarAsync(Venda venda)
    {
        return await IncluirAsync(venda);
    }

    public async Task AlterarAsync(Venda venda)
    {
        var vendaExistente = await _context.Vendas
            .Include(v => v.Itens)
            .FirstOrDefaultAsync(v => v.Id == venda.Id && v.EmpresaId == venda.EmpresaId);

        if (vendaExistente == null)
            throw new Exception("Venda não encontrada");

        if (vendaExistente.Status == StatusVenda.Faturada || vendaExistente.Status == StatusVenda.Cancelado)
            throw new Exception("Não é possível alterar uma venda faturada ou cancelada");

        // Remover itens antigos
        _context.VendaItens.RemoveRange(vendaExistente.Itens);

        // Atualizar dados
        vendaExistente.ClienteId = venda.ClienteId;
        vendaExistente.VendedorId = venda.VendedorId;
        vendaExistente.DataVenda = venda.DataVenda;
        vendaExistente.DataEntrega = venda.DataEntrega;
        vendaExistente.FormaPagamento = venda.FormaPagamento;
        vendaExistente.Parcelas = venda.Parcelas;
        vendaExistente.PercentualDesconto = venda.PercentualDesconto;
        vendaExistente.ValorFrete = venda.ValorFrete;
        vendaExistente.Observacoes = venda.Observacoes;

        // Calcular totais com os novos itens
        vendaExistente.SubTotal = venda.Itens.Sum(i => i.ValorTotal);
        if (vendaExistente.PercentualDesconto > 0)
            vendaExistente.ValorDesconto = vendaExistente.SubTotal * (vendaExistente.PercentualDesconto / 100);
        else
            vendaExistente.ValorDesconto = venda.ValorDesconto;
        vendaExistente.ValorTotal = vendaExistente.SubTotal - vendaExistente.ValorDesconto + vendaExistente.ValorFrete;

        // Adicionar novos itens
        foreach (var item in venda.Itens)
        {
            item.VendaId = vendaExistente.Id;
            item.DataCriacao = DateTime.Now;
            item.Ativo = true;
            _context.VendaItens.Add(item);
        }

        await _context.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id, int empresaId)
    {
        var venda = await _context.Vendas.FirstOrDefaultAsync(v => v.Id == id && v.EmpresaId == empresaId);
        if (venda == null)
            throw new Exception("Venda não encontrada");

        if (venda.Status == StatusVenda.Faturada)
            throw new Exception("Não é possível excluir uma venda faturada");

        venda.Ativo = false;
        await _context.SaveChangesAsync();
    }

    public async Task<Venda> AprovarAsync(int id, int empresaId)
    {
        var venda = await ObterPorIdAsync(id, empresaId);
        if (venda == null)
            throw new Exception("Venda não encontrada");

        if (venda.Status != StatusVenda.Orcamento)
            throw new Exception("Apenas orçamentos podem ser aprovados");

        venda.Status = StatusVenda.Aprovado;
        await _context.SaveChangesAsync();
        return venda;
    }

    public async Task<Venda> FaturarAsync(int id, int empresaId, int? contaBancariaId)
    {
        var venda = await ObterPorIdAsync(id, empresaId);
        if (venda == null)
            throw new InvalidOperationException("Venda não encontrada");

        if (venda.Status != StatusVenda.Aprovado && venda.Status != StatusVenda.Orcamento)
            throw new InvalidOperationException("Apenas orçamentos ou vendas aprovadas podem ser faturadas");

        // Validar estoque antes de faturar
        if (!await ValidarEstoqueAsync(id, empresaId))
            throw new InvalidOperationException("Estoque insuficiente para alguns produtos");

        // Baixar estoque
        await AtualizarEstoque(venda, false);

        // Gerar contas a receber usando o service
        await _contaReceberService.GerarContasDeVendaAsync(venda.Id, empresaId);

        venda.Status = StatusVenda.Faturada;
        await _context.SaveChangesAsync();
        return venda;
    }

    /// <summary>
    /// Fatura uma venda com parcelamento específico
    /// </summary>
    public async Task<Venda> FaturarComParcelasAsync(int id, int empresaId, FormaPagamento formaPagamento, int numeroParcelas)
    {
        var venda = await ObterPorIdAsync(id, empresaId);
        if (venda == null)
            throw new InvalidOperationException("Venda não encontrada");

        // Atualizar forma de pagamento
        venda.FormaPagamento = formaPagamento;
        venda.Parcelas = numeroParcelas;

        return await FaturarAsync(id, empresaId, null);
    }

    public async Task<Venda> CancelarAsync(int id, int empresaId)
    {
        var venda = await ObterPorIdAsync(id, empresaId);
        if (venda == null)
            throw new Exception("Venda não encontrada");

        if (venda.Status == StatusVenda.Cancelado)
            throw new Exception("Venda já está cancelada");

        // Se estava faturada, estornar estoque
        if (venda.Status == StatusVenda.Faturada)
        {
            await AtualizarEstoque(venda, true);
        }

        venda.Status = StatusVenda.Cancelado;
        await _context.SaveChangesAsync();
        return venda;
    }

    public async Task<decimal> ObterTotalVendasAsync(int empresaId, DateTime? inicio = null, DateTime? fim = null)
    {
        var query = _context.Vendas
            .Where(v => v.EmpresaId == empresaId && v.Status == StatusVenda.Faturada && v.Ativo);

        if (inicio.HasValue)
            query = query.Where(v => v.DataVenda >= inicio.Value);
        if (fim.HasValue)
            query = query.Where(v => v.DataVenda <= fim.Value);

        var vendas = await query.Select(v => v.ValorTotal).ToListAsync();
        return vendas.Sum();
    }

    public async Task<IEnumerable<dynamic>> ObterProdutosMaisVendidosAsync(int empresaId, DateTime inicio, DateTime fim, int limite = 10)
    {
        var resultado = await _context.VendaItens
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

        return resultado;
    }

    private void CalcularTotais(Venda venda)
    {
        venda.SubTotal = venda.Itens.Sum(i => i.ValorTotal);
        
        if (venda.PercentualDesconto > 0)
            venda.ValorDesconto = venda.SubTotal * (venda.PercentualDesconto / 100);

        venda.ValorTotal = venda.SubTotal - venda.ValorDesconto + venda.ValorFrete;
    }

    private async Task AtualizarEstoque(Venda venda, bool estornar)
    {
        foreach (var item in venda.Itens)
        {
            var produto = await _context.Produtos.FindAsync(item.ProdutoId);
            if (produto != null && produto.ControlaEstoque)
            {
                if (estornar)
                    produto.EstoqueAtual += item.Quantidade;
                else
                    produto.EstoqueAtual -= item.Quantidade;
            }
        }
    }

    /// <summary>
    /// Obtém itens de uma venda
    /// </summary>
    public async Task<IEnumerable<VendaItem>> ObterItensVendaAsync(int vendaId, int empresaId)
    {
        return await _context.VendaItens
            .Include(i => i.Produto)
            .Where(i => i.VendaId == vendaId && i.Venda.EmpresaId == empresaId && i.Ativo)
            .OrderBy(i => i.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Adiciona um item à venda
    /// </summary>
    public async Task<VendaItem> AdicionarItemAsync(int vendaId, VendaItem item, int empresaId)
    {
        var venda = await ObterPorIdAsync(vendaId, empresaId);
        if (venda == null)
            throw new InvalidOperationException("Venda não encontrada");

        if (venda.Status == StatusVenda.Faturada || venda.Status == StatusVenda.Cancelado)
            throw new InvalidOperationException("Não é possível adicionar itens a uma venda faturada ou cancelada");

        // Verificar se produto existe e calcular valores
        var produto = await _produtoService.ObterPorIdAsync(item.ProdutoId, empresaId);
        if (produto == null)
            throw new InvalidOperationException("Produto não encontrado");

        item.VendaId = vendaId;
        item.PrecoUnitario = produto.PrecoVenda;
        item.ValorTotal = item.Quantidade * item.PrecoUnitario;
        item.DataCriacao = DateTime.Now;
        item.Ativo = true;

        _context.VendaItens.Add(item);
        await _context.SaveChangesAsync();

        // Recalcular totais da venda
        await RecalcularTotaisAsync(vendaId, empresaId);

        return item;
    }

    /// <summary>
    /// Atualiza um item da venda
    /// </summary>
    public async Task<VendaItem> AtualizarItemAsync(VendaItem item, int empresaId)
    {
        var itemExistente = await _context.VendaItens
            .Include(i => i.Venda)
            .FirstOrDefaultAsync(i => i.Id == item.Id && i.Venda.EmpresaId == empresaId);

        if (itemExistente == null)
            throw new InvalidOperationException("Item não encontrado");

        if (itemExistente.Venda.Status == StatusVenda.Faturada || itemExistente.Venda.Status == StatusVenda.Cancelado)
            throw new InvalidOperationException("Não é possível alterar itens de uma venda faturada ou cancelada");

        itemExistente.Quantidade = item.Quantidade;
        itemExistente.PrecoUnitario = item.PrecoUnitario;
        itemExistente.ValorTotal = item.Quantidade * item.PrecoUnitario;

        await _context.SaveChangesAsync();

        // Recalcular totais da venda
        await RecalcularTotaisAsync(itemExistente.VendaId, empresaId);

        return itemExistente;
    }

    /// <summary>
    /// Remove um item da venda
    /// </summary>
    public async Task RemoverItemAsync(int itemId, int empresaId)
    {
        var item = await _context.VendaItens
            .Include(i => i.Venda)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.Venda.EmpresaId == empresaId);

        if (item == null)
            throw new InvalidOperationException("Item não encontrado");

        if (item.Venda.Status == StatusVenda.Faturada || item.Venda.Status == StatusVenda.Cancelado)
            throw new InvalidOperationException("Não é possível remover itens de uma venda faturada ou cancelada");

        item.Ativo = false;
        await _context.SaveChangesAsync();

        // Recalcular totais da venda
        await RecalcularTotaisAsync(item.VendaId, empresaId);
    }

    /// <summary>
    /// Recalcula os totais de uma venda
    /// </summary>
    public async Task<Venda> RecalcularTotaisAsync(int vendaId, int empresaId)
    {
        var venda = await _context.Vendas
            .Include(v => v.Itens.Where(i => i.Ativo))
            .FirstOrDefaultAsync(v => v.Id == vendaId && v.EmpresaId == empresaId);

        if (venda == null)
            throw new InvalidOperationException("Venda não encontrada");

        CalcularTotais(venda);
        await _context.SaveChangesAsync();

        return venda;
    }

    /// <summary>
    /// Valida se há estoque suficiente para todos os itens da venda
    /// </summary>
    public async Task<bool> ValidarEstoqueAsync(int vendaId, int empresaId)
    {
        var itens = await ObterItensVendaAsync(vendaId, empresaId);
        
        foreach (var item in itens)
        {
            var produto = await _produtoService.ObterPorIdAsync(item.ProdutoId, empresaId);
            if (produto != null && produto.ControlaEstoque)
            {
                if (produto.EstoqueAtual < item.Quantidade)
                    return false;
            }
        }

        return true;
    }
}
