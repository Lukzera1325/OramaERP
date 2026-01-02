namespace OramaGo.Services;

public interface IDatabaseService
{
    Task InitializeDatabaseAsync();
    Task<bool> DatabaseExistsAsync();
    Task DeleteDatabaseAsync();
    Task SeedDataAsync();
    Task<string> GetDatabasePathAsync();
    Task<long> GetDatabaseSizeAsync();
}