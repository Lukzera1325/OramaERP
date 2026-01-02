using OramaGo.Models;
using System.Linq.Expressions;

namespace OramaGo.Services;

public interface IBaseService<T> where T : BaseLocalModel
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetAllAsync(int empresaId);
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetByIdAsync(int id, int empresaId);
    Task<IEnumerable<T>> SearchAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> SearchAsync(int empresaId, Expression<Func<T, bool>> predicate);
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteAsync(int id, int empresaId);
    Task<int> CountAsync();
    Task<int> CountAsync(int empresaId);
    Task<IEnumerable<T>> GetPendingSyncAsync();
    Task<IEnumerable<T>> GetPendingSyncAsync(int empresaId);
    Task MarkAsSyncedAsync(int id, int? servidorId = null);
    Task MarkAsConflictAsync(int id, string? conflictInfo = null);
}