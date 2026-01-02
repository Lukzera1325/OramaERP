using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OramaGo.Data;
using OramaGo.Models;

namespace OramaGo.Services;

public class VendaService : BaseService<VendaLocal>, IVendaService
{
    public VendaService(OramaGoDbContext context, ILogger<VendaService> logger, IUserContextService userContext) 
        : base(context, logger, userContext)
    {
    }

    public async Task<IEnumerable<VendaLocal>> GetByClienteIdAsync(int clienteId)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        
        return await _dbSet
            .Include(v => v.Cliente)
            .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
            .Where(v => v.EmpresaId == empresaId && v.ClienteId == clienteId && v.Ativo)
            .OrderByDescending(v => v.DataVenda)
            .ToListAsync();
    }

    public async Task<IEnumerable<VendaLocal>> GetByStatusAsync(StatusVendaLocal status)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        
        return await _dbSet
            .Include(v => v.Cliente)
            .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
            .Where(v => v.EmpresaId == empresaId && v.Status == status && v.Ativo)
            .OrderByDescending(v => v.DataVenda)
            .ToListAsync();
    }

    public async Task<IEnumerable<VendaLocal>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        
        return await _dbSet
            .Include(v => v.Cliente)
            .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
            .Where(v => v.EmpresaId == empresaId && 
                       v.DataVenda >= dataInicio && 
                       v.DataVenda <= dataFim && 
                       v.Ativo)
            .OrderByDescending(v => v.DataVenda)
            .ToListAsync();
    }

    public async Task<IEnumerable<VendaLocal>> GetPendentesAsync()
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        
        return await _dbSet
            .Include(v => v.Cliente)
            .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
            .Where(v => v.EmpresaId == empresaId && 
                       v.Status == StatusVendaLocal.Orcamento && 
                       v.Ativo)
            .OrderByDescending(v => v.DataVenda)
            .ToListAsync();
    }

    public async Task<string> GerarNumeroVendaAsync()
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        var ano = DateTime.Now.Year;
        var mes = DateTime.Now.Month;
        
        var ultimoNumero = await _dbSet
            .Where(v => v.EmpresaId == empresaId && 
                       v.DataVenda.Year == ano && 
                       v.DataVenda.Month == mes)
            .CountAsync();
        
        return $"VD{ano:0000}{mes:00}{(ultimoNumero + 1):0000}";
    }

    public async Task<decimal> CalcularSubTotalAsync(int vendaId)
    {
        var itens = await _context.Set<VendaItemLocal>()
            .Where(i => i.VendaId == vendaId && i.Ativo)
            .ToListAsync();
        
        return itens.Sum(i => i.Quantidade * i.PrecoUnitario);
    }

    public async Task<decimal> CalcularTotalAsync(int vendaId)
    {
        var venda = await GetByIdAsync(vendaId);
        if (venda == null) return 0;
        
        var subtotal = await CalcularSubTotalAsync(vendaId);
        var desconto = venda.ValorDesconto;
        var frete = venda.ValorFrete;
        
        return subtotal - desconto + frete;
    }

    public async Task RecalcularVendaAsync(int vendaId)
    {
        var venda = await GetByIdAsync(vendaId);
        if (venda == null) return;
        
        var itens = await _context.Set<VendaItemLocal>()
            .Where(i => i.VendaId == vendaId && i.Ativo)
            .ToListAsync();
        
        // Recalcular subtotal
        venda.SubTotal = itens.Sum(i => i.Quantidade * i.PrecoUnitario);
        
        // Aplicar desconto percentual se definido
        if (venda.PercentualDesconto > 0)
        {
            venda.ValorDesconto = venda.SubTotal * venda.PercentualDesconto / 100;
        }
        
        // Calcular total
        venda.ValorTotal = venda.SubTotal - venda.ValorDesconto + venda.ValorFrete;
        
        await UpdateAsync(venda);
    }

    public async Task<VendaLocal> AprovarVendaAsync(int vendaId)
    {
        var venda = await GetByIdAsync(vendaId);
        if (venda == null)
            throw new InvalidOperationException("Venda não encontrada");
        
        if (venda.Status != StatusVendaLocal.Orcamento)
            throw new InvalidOperationException("Apenas orçamentos podem ser aprovados");
        
        // Validar estoque
        if (!await ValidarEstoqueAsync(vendaId))
            throw new InvalidOperationException("Estoque insuficiente para alguns itens");
        
        venda.Status = StatusVendaLocal.Aprovado;
        venda.DataModificacao = DateTime.Now;
        
        return await UpdateAsync(venda);
    }

    public async Task<VendaLocal> CancelarVendaAsync(int vendaId, string motivo)
    {
        var venda = await GetByIdAsync(vendaId);
        if (venda == null)
            throw new InvalidOperationException("Venda não encontrada");
        
        if (venda.Status == StatusVendaLocal.Faturado)
            throw new InvalidOperationException("Vendas faturadas não podem ser canceladas");
        
        venda.Status = StatusVendaLocal.Cancelado;
        venda.Observacoes = string.IsNullOrEmpty(venda.Observacoes) 
            ? $"Cancelado: {motivo}" 
            : $"{venda.Observacoes}\nCancelado: {motivo}";
        venda.DataModificacao = DateTime.Now;
        
        return await UpdateAsync(venda);
    }

    public async Task<VendaLocal> FaturarVendaAsync(int vendaId)
    {
        var venda = await GetByIdAsync(vendaId);
        if (venda == null)
            throw new InvalidOperationException("Venda não encontrada");
        
        if (venda.Status != StatusVendaLocal.Aprovado)
            throw new InvalidOperationException("Apenas vendas aprovadas podem ser faturadas");
        
        venda.Status = StatusVendaLocal.Faturado;
        venda.DataModificacao = DateTime.Now;
        
        return await UpdateAsync(venda);
    }

    public async Task<bool> ValidarEstoqueAsync(int vendaId)
    {
        var itens = await _context.Set<VendaItemLocal>()
            .Include(i => i.Produto)
            .Where(i => i.VendaId == vendaId && i.Ativo)
            .ToListAsync();
        
        foreach (var item in itens)
        {
            if (item.Produto?.ControlaEstoque == true && 
                item.Produto.EstoqueAtual < item.Quantidade)
            {
                return false;
            }
        }
        
        return true;
    }

    public async Task<bool> PodeEditarAsync(int vendaId)
    {
        var venda = await GetByIdAsync(vendaId);
        return venda?.PodeEditar ?? false;
    }

    public async Task<bool> PodeAprovarAsync(int vendaId)
    {
        var venda = await GetByIdAsync(vendaId);
        return venda?.PodeAprovar ?? false;
    }

    public async Task<bool> PodeCancelarAsync(int vendaId)
    {
        var venda = await GetByIdAsync(vendaId);
        return venda?.PodeCancelar ?? false;
    }

    public async Task<decimal> GetTotalVendasMesAsync(int mes, int ano)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        
        var vendas = await _dbSet
            .Where(v => v.EmpresaId == empresaId && 
                       v.DataVenda.Month == mes && 
                       v.DataVenda.Year == ano &&
                       v.Status != StatusVendaLocal.Cancelado &&
                       v.Ativo)
            .ToListAsync();
        
        return vendas.Sum(v => v.ValorTotal);
    }

    public async Task<int> GetQuantidadeVendasMesAsync(int mes, int ano)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        
        return await _dbSet
            .CountAsync(v => v.EmpresaId == empresaId && 
                            v.DataVenda.Month == mes && 
                            v.DataVenda.Year == ano &&
                            v.Status != StatusVendaLocal.Cancelado &&
                            v.Ativo);
    }

    public async Task<IEnumerable<VendaLocal>> GetVendasRecentesAsync(int quantidade = 10)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        
        return await _dbSet
            .Include(v => v.Cliente)
            .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
            .Where(v => v.EmpresaId == empresaId && v.Ativo)
            .OrderByDescending(v => v.DataVenda)
            .Take(quantidade)
            .ToListAsync();
    }

    public override async Task<VendaLocal> CreateAsync(VendaLocal venda)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        
        // Gerar número se não informado
        if (string.IsNullOrEmpty(venda.Numero))
        {
            venda.Numero = await GerarNumeroVendaAsync();
        }
        
        // Definir vendedor se não informado (usar um valor padrão por enquanto)
        if (!venda.VendedorId.HasValue)
        {
            venda.VendedorId = 1; // TODO: Implementar quando tivermos o método GetCurrentUserIdAsync
        }
        
        var vendaCriada = await base.CreateAsync(venda);
        
        // Recalcular totais se há itens
        if (venda.Itens?.Any() == true)
        {
            await RecalcularVendaAsync(vendaCriada.Id);
        }
        
        return vendaCriada;
    }

    public override async Task<VendaLocal> UpdateAsync(VendaLocal venda)
    {
        // Recalcular totais antes de salvar
        await RecalcularVendaAsync(venda.Id);
        
        return await base.UpdateAsync(venda);
    }

    public override async Task<IEnumerable<VendaLocal>> GetAllAsync()
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        
        return await _dbSet
            .Include(v => v.Cliente)
            .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
            .Where(v => v.EmpresaId == empresaId && v.Ativo)
            .OrderByDescending(v => v.DataVenda)
            .ToListAsync();
    }

    public override async Task<VendaLocal?> GetByIdAsync(int id)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        
        return await _dbSet
            .Include(v => v.Cliente)
            .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(v => v.Id == id && v.EmpresaId == empresaId && v.Ativo);
    }
}