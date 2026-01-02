using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OramaGo.Data;
using OramaGo.Models;

namespace OramaGo.Services;

public class VendaItemService : BaseService<VendaItemLocal>, IVendaItemService
{
    private readonly IVendaService _vendaService;

    public VendaItemService(
        OramaGoDbContext context, 
        ILogger<VendaItemService> logger, 
        IUserContextService userContext,
        IVendaService vendaService) 
        : base(context, logger, userContext)
    {
        _vendaService = vendaService;
    }

    public async Task<IEnumerable<VendaItemLocal>> GetByVendaIdAsync(int vendaId)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        
        return await _dbSet
            .Include(i => i.Produto)
            .Where(i => i.VendaId == vendaId && i.EmpresaId == empresaId && i.Ativo)
            .OrderBy(i => i.DataCriacao)
            .ToListAsync();
    }

    public async Task<VendaItemLocal> AdicionarItemAsync(int vendaId, int produtoId, decimal quantidade, decimal precoUnitario)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        
        // Verificar se a venda existe e pode ser editada
        var venda = await _vendaService.GetByIdAsync(vendaId);
        if (venda == null)
            throw new InvalidOperationException("Venda não encontrada");
        
        if (!venda.PodeEditar)
            throw new InvalidOperationException("Esta venda não pode ser editada");
        
        // Verificar se o produto existe
        var produto = await _context.Set<ProdutoLocal>()
            .FirstOrDefaultAsync(p => p.Id == produtoId && p.EmpresaId == empresaId && p.Ativo);
        
        if (produto == null)
            throw new InvalidOperationException("Produto não encontrado");
        
        // Verificar se já existe um item com este produto
        var itemExistente = await _dbSet
            .FirstOrDefaultAsync(i => i.VendaId == vendaId && 
                                     i.ProdutoId == produtoId && 
                                     i.EmpresaId == empresaId && 
                                     i.Ativo);
        
        if (itemExistente != null)
        {
            // Atualizar quantidade do item existente
            itemExistente.Quantidade += quantidade;
            await RecalcularItemAsync(itemExistente.Id);
            await UpdateAsync(itemExistente);
            
            // Recalcular venda
            await _vendaService.RecalcularVendaAsync(vendaId);
            
            return itemExistente;
        }
        
        // Criar novo item
        var novoItem = new VendaItemLocal
        {
            VendaId = vendaId,
            ProdutoId = produtoId,
            Quantidade = quantidade,
            PrecoUnitario = precoUnitario,
            EmpresaId = empresaId
        };
        
        await RecalcularItemAsync(novoItem);
        var itemCriado = await CreateAsync(novoItem);
        
        // Recalcular venda
        await _vendaService.RecalcularVendaAsync(vendaId);
        
        return itemCriado;
    }

    public async Task<VendaItemLocal> AtualizarQuantidadeAsync(int itemId, decimal quantidade)
    {
        var item = await GetByIdAsync(itemId);
        if (item == null)
            throw new InvalidOperationException("Item não encontrado");
        
        // Verificar se a venda pode ser editada
        if (!await _vendaService.PodeEditarAsync(item.VendaId))
            throw new InvalidOperationException("Esta venda não pode ser editada");
        
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero");
        
        item.Quantidade = quantidade;
        await RecalcularItemAsync(item.Id);
        var itemAtualizado = await UpdateAsync(item);
        
        // Recalcular venda
        await _vendaService.RecalcularVendaAsync(item.VendaId);
        
        return itemAtualizado;
    }

    public async Task<VendaItemLocal> AtualizarPrecoAsync(int itemId, decimal precoUnitario)
    {
        var item = await GetByIdAsync(itemId);
        if (item == null)
            throw new InvalidOperationException("Item não encontrado");
        
        // Verificar se a venda pode ser editada
        if (!await _vendaService.PodeEditarAsync(item.VendaId))
            throw new InvalidOperationException("Esta venda não pode ser editada");
        
        if (precoUnitario <= 0)
            throw new ArgumentException("Preço unitário deve ser maior que zero");
        
        item.PrecoUnitario = precoUnitario;
        await RecalcularItemAsync(item.Id);
        var itemAtualizado = await UpdateAsync(item);
        
        // Recalcular venda
        await _vendaService.RecalcularVendaAsync(item.VendaId);
        
        return itemAtualizado;
    }

    public async Task<VendaItemLocal> AtualizarDescontoAsync(int itemId, decimal percentualDesconto)
    {
        var item = await GetByIdAsync(itemId);
        if (item == null)
            throw new InvalidOperationException("Item não encontrado");
        
        // Verificar se a venda pode ser editada
        if (!await _vendaService.PodeEditarAsync(item.VendaId))
            throw new InvalidOperationException("Esta venda não pode ser editada");
        
        if (percentualDesconto < 0 || percentualDesconto > 100)
            throw new ArgumentException("Percentual de desconto deve estar entre 0 e 100");
        
        item.PercentualDesconto = percentualDesconto;
        await RecalcularItemAsync(item.Id);
        var itemAtualizado = await UpdateAsync(item);
        
        // Recalcular venda
        await _vendaService.RecalcularVendaAsync(item.VendaId);
        
        return itemAtualizado;
    }

    public async Task RemoverItemAsync(int itemId)
    {
        var item = await GetByIdAsync(itemId);
        if (item == null)
            throw new InvalidOperationException("Item não encontrado");
        
        // Verificar se a venda pode ser editada
        if (!await _vendaService.PodeEditarAsync(item.VendaId))
            throw new InvalidOperationException("Esta venda não pode ser editada");
        
        var vendaId = item.VendaId;
        await DeleteAsync(itemId);
        
        // Recalcular venda
        await _vendaService.RecalcularVendaAsync(vendaId);
    }

    public async Task<bool> ValidarEstoqueItemAsync(int itemId)
    {
        var item = await _dbSet
            .Include(i => i.Produto)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.Ativo);
        
        if (item?.Produto == null) return false;
        
        return !item.Produto.ControlaEstoque || item.Produto.EstoqueAtual >= item.Quantidade;
    }

    public async Task RecalcularItemAsync(int itemId)
    {
        var item = await GetByIdAsync(itemId);
        if (item == null) return;
        
        await RecalcularItemAsync(item);
    }

    private async Task RecalcularItemAsync(VendaItemLocal item)
    {
        // Calcular desconto
        item.ValorDesconto = item.PrecoUnitario * item.PercentualDesconto / 100;
        
        // Calcular total
        var precoComDesconto = item.PrecoUnitario - item.ValorDesconto;
        item.ValorTotal = item.Quantidade * precoComDesconto;
        
        await Task.CompletedTask;
    }

    public override async Task<VendaItemLocal> CreateAsync(VendaItemLocal item)
    {
        await RecalcularItemAsync(item);
        return await base.CreateAsync(item);
    }

    public override async Task<VendaItemLocal> UpdateAsync(VendaItemLocal item)
    {
        await RecalcularItemAsync(item);
        return await base.UpdateAsync(item);
    }

    public override async Task<IEnumerable<VendaItemLocal>> GetAllAsync()
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        
        return await _dbSet
            .Include(i => i.Produto)
            .Include(i => i.Venda)
            .Where(i => i.EmpresaId == empresaId && i.Ativo)
            .OrderByDescending(i => i.DataCriacao)
            .ToListAsync();
    }

    public override async Task<VendaItemLocal?> GetByIdAsync(int id)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        
        return await _dbSet
            .Include(i => i.Produto)
            .Include(i => i.Venda)
            .FirstOrDefaultAsync(i => i.Id == id && i.EmpresaId == empresaId && i.Ativo);
    }
}