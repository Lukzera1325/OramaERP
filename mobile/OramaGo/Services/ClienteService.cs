using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OramaGo.Data;
using OramaGo.Models;

namespace OramaGo.Services;

public class ClienteService : BaseService<ClienteLocal>, IClienteService
{
    public ClienteService(OramaGoDbContext context, ILogger<ClienteService> logger, IUserContextService userContext) 
        : base(context, logger, userContext)
    {
    }

    public async Task<IEnumerable<ClienteLocal>> SearchAsync(int empresaId, string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return await GetAllAsync(empresaId);

        termo = termo.ToLower().Trim();

        return await _dbSet
            .Where(x => x.EmpresaId == empresaId && x.Ativo)
            .Where(x => x.Nome.ToLower().Contains(termo) ||
                       x.CpfCnpj!.Contains(termo) ||
                       x.Email!.ToLower().Contains(termo) ||
                       x.Telefone!.Contains(termo) ||
                       x.Celular!.Contains(termo))
            .OrderBy(x => x.Nome)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClienteLocal>> SearchAsync(string termo)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        return await SearchAsync(empresaId, termo);
    }

    public async Task<bool> ExistsByCpfCnpjAsync(string cpfCnpj, int? excludeId = null)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        return await ExistsByCpfCnpjAsync(cpfCnpj, empresaId, excludeId);
    }

    public async Task<bool> ExistsByCpfCnpjAsync(string cpfCnpj, int empresaId, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(cpfCnpj))
            return false;

        var query = _dbSet.Where(x => x.EmpresaId == empresaId && 
                                     x.Ativo && 
                                     x.CpfCnpj == cpfCnpj);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<ClienteLocal>> GetByVendedorAsync(int vendedorId)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        return await GetByVendedorAsync(empresaId, vendedorId);
    }

    public async Task<IEnumerable<ClienteLocal>> GetByVendedorAsync(int empresaId, int vendedorId)
    {
        return await _dbSet
            .Where(x => x.EmpresaId == empresaId && 
                       x.Ativo && 
                       x.VendedorId == vendedorId)
            .OrderBy(x => x.Nome)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalCreditoDisponivelAsync()
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        return await GetTotalCreditoDisponivelAsync(empresaId);
    }

    public async Task<decimal> GetTotalCreditoDisponivelAsync(int empresaId)
    {
        return await _dbSet
            .Where(x => x.EmpresaId == empresaId && x.Ativo)
            .SumAsync(x => x.LimiteCredito - x.SaldoAtual);
    }

    public async Task<int> GetCountAsync()
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        return await _dbSet
            .Where(x => x.EmpresaId == empresaId && x.Ativo)
            .CountAsync();
    }

    public override async Task<IEnumerable<ClienteLocal>> GetPendingSyncAsync()
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        return await _dbSet
            .Where(x => x.EmpresaId == empresaId && 
                       x.Ativo && 
                       (x.StatusSync == Models.SyncStatus.Pending || 
                        x.StatusSync == Models.SyncStatus.Modified ||
                        x.StatusSync == Models.SyncStatus.Error))
            .OrderBy(x => x.DataModificacao)
            .ToListAsync();
    }

    public override async Task<ClienteLocal> CreateAsync(ClienteLocal cliente)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        
        // Validações específicas
        if (await ExistsByCpfCnpjAsync(cliente.CpfCnpj!, empresaId))
            throw new InvalidOperationException("Já existe um cliente com este CPF/CNPJ");

        // Definir status de sincronização para novo registro
        cliente.StatusSync = Models.SyncStatus.Pending;
        cliente.DataUltimaSync = null;

        return await base.CreateAsync(cliente);
    }

    public override async Task<ClienteLocal> UpdateAsync(ClienteLocal cliente)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        
        // Validações específicas
        if (await ExistsByCpfCnpjAsync(cliente.CpfCnpj!, empresaId, cliente.Id))
            throw new InvalidOperationException("Já existe um cliente com este CPF/CNPJ");

        // Se já foi sincronizado, marcar como modificado
        if (cliente.StatusSync == Models.SyncStatus.Synced)
        {
            cliente.StatusSync = Models.SyncStatus.Modified;
        }

        return await base.UpdateAsync(cliente);
    }
}