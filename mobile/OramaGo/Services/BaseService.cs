using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OramaGo.Data;
using OramaGo.Models;
using System.Linq.Expressions;

namespace OramaGo.Services;

public abstract class BaseService<T> : IBaseService<T> where T : BaseLocalModel
{
    protected readonly OramaGoDbContext _context;
    protected readonly ILogger _logger;
    protected readonly IUserContextService _userContext;
    protected readonly DbSet<T> _dbSet;

    protected BaseService(OramaGoDbContext context, ILogger logger, IUserContextService userContext)
    {
        _context = context;
        _logger = logger;
        _userContext = userContext;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        return await GetAllAsync(empresaId);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(int empresaId)
    {
        return await _dbSet
            .Where(x => x.EmpresaId == empresaId && x.Ativo)
            .OrderByDescending(x => x.DataModificacao)
            .ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        return await GetByIdAsync(id, empresaId);
    }

    public virtual async Task<T?> GetByIdAsync(int id, int empresaId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId && x.Ativo);
    }

    public virtual async Task<IEnumerable<T>> SearchAsync(Expression<Func<T, bool>> predicate)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        return await SearchAsync(empresaId, predicate);
    }

    public virtual async Task<IEnumerable<T>> SearchAsync(int empresaId, Expression<Func<T, bool>> predicate)
    {
        return await _dbSet
            .Where(x => x.EmpresaId == empresaId && x.Ativo)
            .Where(predicate)
            .OrderByDescending(x => x.DataModificacao)
            .ToListAsync();
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        try
        {
            // Garantir isolamento multi-tenant
            var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
            if (empresaId == 0)
                throw new UnauthorizedAccessException("Usuário não autenticado");

            entity.EmpresaId = empresaId;
            entity.DataCriacao = DateTime.Now;
            entity.DataModificacao = DateTime.Now;
            entity.Ativo = true;
            entity.StatusSync = Models.SyncStatus.Pending;

            _dbSet.Add(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Entidade {typeof(T).Name} criada com ID {entity.Id} para empresa {empresaId}");
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao criar entidade {typeof(T).Name}");
            throw;
        }
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        try
        {
            var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
            if (empresaId == 0)
                throw new UnauthorizedAccessException("Usuário não autenticado");

            var existing = await _dbSet.FindAsync(entity.Id);
            if (existing == null)
                throw new InvalidOperationException($"Entidade {typeof(T).Name} com ID {entity.Id} não encontrada");

            // Validar isolamento multi-tenant
            if (existing.EmpresaId != empresaId)
                throw new UnauthorizedAccessException("Não é possível alterar entidade de outra empresa");

            // Atualizar propriedades
            _context.Entry(existing).CurrentValues.SetValues(entity);
            existing.DataModificacao = DateTime.Now;
            existing.EmpresaId = empresaId; // Garantir que não mude

            // Marcar como modificado se estava sincronizado
            if (existing.StatusSync == Models.SyncStatus.Synced)
                existing.StatusSync = Models.SyncStatus.Modified;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Entidade {typeof(T).Name} com ID {entity.Id} atualizada para empresa {empresaId}");
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao atualizar entidade {typeof(T).Name} com ID {entity.Id}");
            throw;
        }
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        return await DeleteAsync(id, empresaId);
    }

    public virtual async Task<bool> DeleteAsync(int id, int empresaId)
    {
        try
        {
            var entity = await GetByIdAsync(id, empresaId);
            if (entity == null)
                return false;

            // Soft delete
            entity.Ativo = false;
            entity.DataModificacao = DateTime.Now;

            // Marcar como modificado se estava sincronizado
            if (entity.StatusSync == Models.SyncStatus.Synced)
                entity.StatusSync = Models.SyncStatus.Modified;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Entidade {typeof(T).Name} com ID {id} marcada como inativa para empresa {empresaId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao deletar entidade {typeof(T).Name} com ID {id}");
            throw;
        }
    }

    public virtual async Task<int> CountAsync()
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        return await CountAsync(empresaId);
    }

    public virtual async Task<int> CountAsync(int empresaId)
    {
        return await _dbSet.CountAsync(x => x.EmpresaId == empresaId && x.Ativo);
    }

    public virtual async Task<IEnumerable<T>> GetPendingSyncAsync()
    {
        var empresaId = await _userContext.GetCurrentEmpresaIdAsync();
        return await GetPendingSyncAsync(empresaId);
    }

    public virtual async Task<IEnumerable<T>> GetPendingSyncAsync(int empresaId)
    {
        return await _dbSet
            .Where(x => x.EmpresaId == empresaId && 
                       (x.StatusSync == Models.SyncStatus.Pending || x.StatusSync == Models.SyncStatus.Modified))
            .OrderBy(x => x.DataCriacao)
            .ToListAsync();
    }

    public virtual async Task MarkAsSyncedAsync(int id, int? servidorId = null)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            entity.StatusSync = Models.SyncStatus.Synced;
            entity.DataUltimaSync = DateTime.Now;
            if (servidorId.HasValue)
                entity.ServidorId = servidorId.Value;

            await _context.SaveChangesAsync();
        }
    }

    public virtual async Task MarkAsConflictAsync(int id, string? conflictInfo = null)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            entity.StatusSync = Models.SyncStatus.Conflict;
            // Poderia adicionar campo para informações do conflito
            await _context.SaveChangesAsync();
        }
    }

    protected virtual IQueryable<T> ApplyIncludes(IQueryable<T> query)
    {
        // Override em classes derivadas para incluir relacionamentos
        return query;
    }
}