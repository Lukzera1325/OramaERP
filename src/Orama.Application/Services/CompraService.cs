using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

/// <summary>
/// Serviço para gerenciamento de compras
/// </summary>
public class CompraService : ICompraService
{
    private readonly OramaDbContext _context;
    private readonly IContaPagarService _contaPagarService;
    private readonly IProdutoService _produtoService;

    public CompraService(OramaDbContext context, IContaPagarService contaPagarService, IProdutoService produtoService)
    {
        _context = context;
        _contaPagarService = contaPagarService;
        _produtoService = produtoService;
    }

    /// <summary>
    /// Obtém todas as compras ativas por empresa
    /// </summary>
    public async Task<IEnumerable<Compra>> ObterTodosAsync(int empresaId)
    {
        return await _context.Compras
            .Include(c => c.Fornecedor)
            .Include(c => c.Itens)
                .ThenInclude(i => i.Produto)
            .Where(c => c.EmpresaId == empresaId && c.Ativo)
            .OrderByDescending(c => c.DataCompra)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém uma compra por ID e empresa
    /// </summary>
    public async Task<Compra?> ObterPorIdAsync(int id, int empresaId)
    {
        return await _context.Compras
            .Include(c => c.Fornecedor)
            .Include(c => c.Itens)
                .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId && c.Ativo);
    }

    /// <summary>
    /// Obtém compras por status
    /// </summary>
    public async Task<IEnumerable<Compra>> ObterPorStatusAsync(int empresaId, StatusCompra status)
    {
        return await _context.Compras
            .Include(c => c.Fornecedor)
            .Where(c => c.EmpresaId == empresaId && c.Status == status && c.Ativo)
            .OrderByDescending(c => c.DataCompra)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém compras por fornecedor
    /// </summary>
    public async Task<IEnumerable<Compra>> ObterPorFornecedorAsync(int empresaId, int fornecedorId)
    {
        return await _context.Compras
            .Include(c => c.Fornecedor)
            .Where(c => c.EmpresaId == empresaId && c.FornecedorId == fornecedorId && c.Ativo)
            .OrderByDescending(c => c.DataCompra)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém compras por período
    /// </summary>
    public async Task<IEnumerable<Compra>> ObterPorPeriodoAsync(int empresaId, DateTime inicio, DateTime fim)
    {
        return await _context.Compras
            .Include(c => c.Fornecedor)
            .Where(c => c.EmpresaId == empresaId && c.DataCompra >= inicio && c.DataCompra <= fim && c.Ativo)
            .OrderByDescending(c => c.DataCompra)
            .ToListAsync();
    }

    /// <summary>
    /// Gera número sequencial para nova compra
    /// </summary>
    public async Task<string> GerarNumeroAsync(int empresaId)
    {
        var ultimaCompra = await _context.Compras
            .Where(c => c.EmpresaId == empresaId)
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();

        int proximoNumero = 1;
        if (ultimaCompra != null && int.TryParse(ultimaCompra.NumeroCompra, out int numero))
        {
            proximoNumero = numero + 1;
        }
        return proximoNumero.ToString("D6");
    }

    /// <summary>
    /// Cria uma nova compra
    /// </summary>
    public async Task<Compra> CriarAsync(Compra compra)
    {
        compra.NumeroCompra = await GerarNumeroAsync(compra.EmpresaId);
        compra.Status = StatusCompra.Pedido;
        compra.Ativo = true;

        // Calcular totais
        CalcularTotais(compra);

        _context.Compras.Add(compra);
        await _context.SaveChangesAsync();
        return compra;
    }

    /// <summary>
    /// Atualiza uma compra existente
    /// </summary>
    public async Task<Compra> AtualizarAsync(Compra compra)
    {
        var compraExistente = await _context.Compras
            .Include(c => c.Itens)
            .FirstOrDefaultAsync(c => c.Id == compra.Id && c.EmpresaId == compra.EmpresaId);

        if (compraExistente == null)
            throw new InvalidOperationException("Compra não encontrada");

        if (compraExistente.Status == StatusCompra.Recebida || compraExistente.Status == StatusCompra.Cancelada)
            throw new InvalidOperationException("Não é possível alterar uma compra recebida ou cancelada");

        // Remover itens antigos
        _context.CompraItens.RemoveRange(compraExistente.Itens);

        // Atualizar dados
        compraExistente.FornecedorId = compra.FornecedorId;
        compraExistente.DataCompra = compra.DataCompra;
        compraExistente.DataEntrega = compra.DataEntrega;
        compraExistente.FormaPagamento = compra.FormaPagamento;
        compraExistente.Parcelas = compra.Parcelas;
        compraExistente.PercentualDesconto = compra.PercentualDesconto;
        compraExistente.ValorFrete = compra.ValorFrete;
        compraExistente.Observacoes = compra.Observacoes;

        // Adicionar novos itens
        foreach (var item in compra.Itens)
        {
            item.CompraId = compraExistente.Id;
            item.Ativo = true;
            _context.CompraItens.Add(item);
        }

        // Recalcular totais
        CalcularTotais(compraExistente);

        await _context.SaveChangesAsync();
        return compraExistente;
    }

    /// <summary>
    /// Exclui uma compra (soft delete)
    /// </summary>
    public async Task ExcluirAsync(int id, int empresaId)
    {
        var compra = await _context.Compras.FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId);
        if (compra == null)
            throw new InvalidOperationException("Compra não encontrada");

        if (compra.Status == StatusCompra.Recebida)
            throw new InvalidOperationException("Não é possível excluir uma compra recebida");

        compra.Ativo = false;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Aprova uma compra
    /// </summary>
    public async Task<Compra> AprovarAsync(int id, int empresaId)
    {
        var compra = await ObterPorIdAsync(id, empresaId);
        if (compra == null)
            throw new InvalidOperationException("Compra não encontrada");

        if (compra.Status != StatusCompra.Pedido)
            throw new InvalidOperationException("Apenas pedidos podem ser aprovados");

        compra.Status = StatusCompra.Aprovada;
        await _context.SaveChangesAsync();
        return compra;
    }

    /// <summary>
    /// Recebe uma compra (entrada no estoque)
    /// </summary>
    public async Task<Compra> ReceberAsync(int id, int empresaId)
    {
        var compra = await ObterPorIdAsync(id, empresaId);
        if (compra == null)
            throw new InvalidOperationException("Compra não encontrada");

        if (compra.Status != StatusCompra.Aprovada && compra.Status != StatusCompra.Pedido)
            throw new InvalidOperationException("Apenas compras aprovadas ou pedidos podem ser recebidos");

        // Dar entrada no estoque
        await AtualizarEstoque(compra, true);

        // Gerar contas a pagar usando o service
        await _contaPagarService.GerarContasDeCompraAsync(compra.Id, empresaId);

        compra.Status = StatusCompra.Recebida;
        compra.DataRecebimento = DateTime.Now;
        await _context.SaveChangesAsync();
        return compra;
    }

    /// <summary>
    /// Cancela uma compra
    /// </summary>
    public async Task<Compra> CancelarAsync(int id, int empresaId)
    {
        var compra = await ObterPorIdAsync(id, empresaId);
        if (compra == null)
            throw new InvalidOperationException("Compra não encontrada");

        if (compra.Status == StatusCompra.Cancelada)
            throw new InvalidOperationException("Compra já está cancelada");

        // Se estava recebida, estornar estoque
        if (compra.Status == StatusCompra.Recebida)
        {
            await AtualizarEstoque(compra, false);
        }

        compra.Status = StatusCompra.Cancelada;
        await _context.SaveChangesAsync();
        return compra;
    }

    /// <summary>
    /// Obtém itens de uma compra
    /// </summary>
    public async Task<IEnumerable<CompraItem>> ObterItensCompraAsync(int compraId, int empresaId)
    {
        return await _context.CompraItens
            .Include(i => i.Produto)
            .Where(i => i.CompraId == compraId && i.Compra.EmpresaId == empresaId && i.Ativo)
            .OrderBy(i => i.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Adiciona um item à compra
    /// </summary>
    public async Task<CompraItem> AdicionarItemAsync(int compraId, CompraItem item, int empresaId)
    {
        var compra = await ObterPorIdAsync(compraId, empresaId);
        if (compra == null)
            throw new InvalidOperationException("Compra não encontrada");

        if (compra.Status == StatusCompra.Recebida || compra.Status == StatusCompra.Cancelada)
            throw new InvalidOperationException("Não é possível adicionar itens a uma compra recebida ou cancelada");

        // Verificar se produto existe
        var produto = await _produtoService.ObterPorIdAsync(item.ProdutoId, empresaId);
        if (produto == null)
            throw new InvalidOperationException("Produto não encontrado");

        item.CompraId = compraId;
        item.ValorTotal = item.Quantidade * item.ValorUnitario;
        item.Ativo = true;

        _context.CompraItens.Add(item);
        await _context.SaveChangesAsync();

        // Recalcular totais da compra
        await RecalcularTotaisAsync(compraId, empresaId);

        return item;
    }

    /// <summary>
    /// Atualiza um item da compra
    /// </summary>
    public async Task<CompraItem> AtualizarItemAsync(CompraItem item, int empresaId)
    {
        var itemExistente = await _context.CompraItens
            .Include(i => i.Compra)
            .FirstOrDefaultAsync(i => i.Id == item.Id && i.Compra.EmpresaId == empresaId);

        if (itemExistente == null)
            throw new InvalidOperationException("Item não encontrado");

        if (itemExistente.Compra.Status == StatusCompra.Recebida || itemExistente.Compra.Status == StatusCompra.Cancelada)
            throw new InvalidOperationException("Não é possível alterar itens de uma compra recebida ou cancelada");

        itemExistente.Quantidade = item.Quantidade;
        itemExistente.ValorUnitario = item.ValorUnitario;
        itemExistente.ValorTotal = item.Quantidade * item.ValorUnitario;

        await _context.SaveChangesAsync();

        // Recalcular totais da compra
        await RecalcularTotaisAsync(itemExistente.CompraId, empresaId);

        return itemExistente;
    }

    /// <summary>
    /// Remove um item da compra
    /// </summary>
    public async Task RemoverItemAsync(int itemId, int empresaId)
    {
        var item = await _context.CompraItens
            .Include(i => i.Compra)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.Compra.EmpresaId == empresaId);

        if (item == null)
            throw new InvalidOperationException("Item não encontrado");

        if (item.Compra.Status == StatusCompra.Recebida || item.Compra.Status == StatusCompra.Cancelada)
            throw new InvalidOperationException("Não é possível remover itens de uma compra recebida ou cancelada");

        item.Ativo = false;
        await _context.SaveChangesAsync();

        // Recalcular totais da compra
        await RecalcularTotaisAsync(item.CompraId, empresaId);
    }

    /// <summary>
    /// Recalcula os totais de uma compra
    /// </summary>
    public async Task<Compra> RecalcularTotaisAsync(int compraId, int empresaId)
    {
        var compra = await _context.Compras
            .Include(c => c.Itens.Where(i => i.Ativo))
            .FirstOrDefaultAsync(c => c.Id == compraId && c.EmpresaId == empresaId);

        if (compra == null)
            throw new InvalidOperationException("Compra não encontrada");

        CalcularTotais(compra);
        await _context.SaveChangesAsync();

        return compra;
    }

    /// <summary>
    /// Obtém total de compras por período
    /// </summary>
    public async Task<decimal> ObterTotalComprasAsync(int empresaId, DateTime? inicio = null, DateTime? fim = null)
    {
        var query = _context.Compras
            .Where(c => c.EmpresaId == empresaId && c.Status == StatusCompra.Recebida && c.Ativo);

        if (inicio.HasValue)
            query = query.Where(c => c.DataCompra >= inicio.Value);
        if (fim.HasValue)
            query = query.Where(c => c.DataCompra <= fim.Value);

        var compras = await query.Select(c => c.ValorTotal).ToListAsync();
        return compras.Sum();
    }

    /// <summary>
    /// Calcula os totais de uma compra
    /// </summary>
    private void CalcularTotais(Compra compra)
    {
        compra.SubTotal = compra.Itens.Where(i => i.Ativo).Sum(i => i.ValorTotal);
        
        if (compra.PercentualDesconto > 0)
            compra.ValorDesconto = compra.SubTotal * (compra.PercentualDesconto / 100);

        compra.ValorTotal = compra.SubTotal - compra.ValorDesconto + compra.ValorFrete;
    }

    /// <summary>
    /// Atualiza estoque dos produtos da compra
    /// </summary>
    private async Task AtualizarEstoque(Compra compra, bool entrada)
    {
        foreach (var item in compra.Itens.Where(i => i.Ativo))
        {
            var produto = await _context.Produtos.FindAsync(item.ProdutoId);
            if (produto != null && produto.ControlaEstoque)
            {
                if (entrada)
                    produto.EstoqueAtual += item.Quantidade;
                else
                    produto.EstoqueAtual -= item.Quantidade;
            }
        }
    }
}