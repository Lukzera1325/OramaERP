using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OramaGo.Data;
using OramaGo.Models;

namespace OramaGo.Services;

public class ProdutoService : BaseService<ProdutoLocal>, IProdutoService
{
    public ProdutoService(OramaGoDbContext context, ILogger<ProdutoService> logger, IUserContextService userContext) 
        : base(context, logger, userContext)
    {
    }

    public async Task<IEnumerable<ProdutoLocal>> SearchAsync(string termo)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        return await SearchAsync(empresaId, termo);
    }

    public async Task<IEnumerable<ProdutoLocal>> SearchAsync(int empresaId, string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return await GetAtivosParaVendaAsync(empresaId);

        termo = termo.ToLower().Trim();

        return await _dbSet
            .Where(x => x.EmpresaId == empresaId && x.Ativo && x.AtivoVenda)
            .Where(x => x.Codigo.ToLower().Contains(termo) ||
                       x.Descricao.ToLower().Contains(termo) ||
                       x.DescricaoDetalhada!.ToLower().Contains(termo) ||
                       x.Categoria!.ToLower().Contains(termo) ||
                       x.Marca!.ToLower().Contains(termo) ||
                       x.CodigoBarras!.Contains(termo))
            .OrderBy(x => x.Descricao)
            .ToListAsync();
    }

    public async Task<bool> ExistsByCodigoAsync(string codigo, int? excludeId = null)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        return await ExistsByCodigoAsync(codigo, empresaId, excludeId);
    }

    public async Task<bool> ExistsByCodigoAsync(string codigo, int empresaId, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return false;

        var query = _dbSet.Where(x => x.EmpresaId == empresaId && 
                                     x.Ativo && 
                                     x.Codigo == codigo);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<ProdutoLocal>> GetByCategoriaAsync(string categoria)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        return await GetByCategoriaAsync(empresaId, categoria);
    }

    public async Task<IEnumerable<ProdutoLocal>> GetByCategoriaAsync(int empresaId, string categoria)
    {
        return await _dbSet
            .Where(x => x.EmpresaId == empresaId && 
                       x.Ativo && 
                       x.AtivoVenda &&
                       x.Categoria == categoria)
            .OrderBy(x => x.Descricao)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProdutoLocal>> GetComEstoqueBaixoAsync()
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        return await GetComEstoqueBaixoAsync(empresaId);
    }

    public async Task<IEnumerable<ProdutoLocal>> GetComEstoqueBaixoAsync(int empresaId)
    {
        return await _dbSet
            .Where(x => x.EmpresaId == empresaId && 
                       x.Ativo && 
                       x.ControlaEstoque &&
                       x.EstoqueAtual <= x.EstoqueMinimo)
            .OrderBy(x => x.EstoqueAtual)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProdutoLocal>> GetAtivosParaVendaAsync()
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        return await GetAtivosParaVendaAsync(empresaId);
    }

    public async Task<IEnumerable<ProdutoLocal>> GetAtivosParaVendaAsync(int empresaId)
    {
        return await _dbSet
            .Where(x => x.EmpresaId == empresaId && x.Ativo && x.AtivoVenda)
            .OrderBy(x => x.Descricao)
            .ToListAsync();
    }

    public async Task<ProdutoLocal?> GetByCodigoBarrasAsync(string codigoBarras)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        return await GetByCodigoBarrasAsync(codigoBarras, empresaId);
    }

    public async Task<ProdutoLocal?> GetByCodigoBarrasAsync(string codigoBarras, int empresaId)
    {
        if (string.IsNullOrWhiteSpace(codigoBarras))
            return null;

        return await _dbSet
            .FirstOrDefaultAsync(x => x.EmpresaId == empresaId && 
                                     x.Ativo && 
                                     x.CodigoBarras == codigoBarras);
    }

    public async Task<IEnumerable<string>> GetCategoriasAsync()
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        return await GetCategoriasAsync(empresaId);
    }

    public async Task<IEnumerable<string>> GetCategoriasAsync(int empresaId)
    {
        return await _dbSet
            .Where(x => x.EmpresaId == empresaId && x.Ativo && !string.IsNullOrEmpty(x.Categoria))
            .Select(x => x.Categoria!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();
    }

    public override async Task<ProdutoLocal> CreateAsync(ProdutoLocal produto)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        
        // Validações específicas
        if (await ExistsByCodigoAsync(produto.Codigo, empresaId))
            throw new InvalidOperationException("Já existe um produto com este código");

        // Calcular margem de lucro se não informada
        if (produto.MargemLucro == 0 && produto.PrecoCusto > 0 && produto.PrecoVenda > 0)
        {
            produto.MargemLucro = ((produto.PrecoVenda - produto.PrecoCusto) / produto.PrecoCusto) * 100;
        }

        return await base.CreateAsync(produto);
    }

    public override async Task<ProdutoLocal> UpdateAsync(ProdutoLocal produto)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        
        // Validações específicas
        if (await ExistsByCodigoAsync(produto.Codigo, empresaId, produto.Id))
            throw new InvalidOperationException("Já existe um produto com este código");

        // Recalcular margem de lucro
        if (produto.PrecoCusto > 0 && produto.PrecoVenda > 0)
        {
            produto.MargemLucro = ((produto.PrecoVenda - produto.PrecoCusto) / produto.PrecoCusto) * 100;
        }

        return await base.UpdateAsync(produto);
    }
}