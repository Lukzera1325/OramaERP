using System.ComponentModel;

namespace OramaGo.Services;

/// <summary>
/// Interface para serviço de sincronização
/// </summary>
public interface ISyncService : INotifyPropertyChanged
{
    /// <summary>
    /// Indica se a sincronização está em andamento
    /// </summary>
    bool IsSyncing { get; }

    /// <summary>
    /// Indica se a sincronização automática está habilitada
    /// </summary>
    bool IsAutoSyncEnabled { get; set; }

    /// <summary>
    /// Data/hora da última sincronização bem-sucedida
    /// </summary>
    DateTime? LastSyncTime { get; }

    /// <summary>
    /// Status atual da sincronização
    /// </summary>
    SyncStatus Status { get; }

    /// <summary>
    /// Progresso da sincronização atual (0-100)
    /// </summary>
    int Progress { get; }

    /// <summary>
    /// Mensagem de status atual
    /// </summary>
    string StatusMessage { get; }

    /// <summary>
    /// Evento disparado quando o status da sincronização muda
    /// </summary>
    event EventHandler<SyncStatusChangedEventArgs> SyncStatusChanged;

    /// <summary>
    /// Inicia a sincronização automática em background
    /// </summary>
    Task StartAutoSyncAsync();

    /// <summary>
    /// Para a sincronização automática
    /// </summary>
    void StopAutoSync();

    /// <summary>
    /// Executa sincronização manual completa
    /// </summary>
    Task<SyncResult> SyncAllAsync();

    /// <summary>
    /// Sincroniza apenas clientes
    /// </summary>
    Task<SyncResult> SyncClientesAsync();

    /// <summary>
    /// Sincroniza apenas produtos
    /// </summary>
    Task<SyncResult> SyncProdutosAsync();

    /// <summary>
    /// Sincroniza apenas vendas
    /// </summary>
    Task<SyncResult> SyncVendasAsync();

    /// <summary>
    /// Força sincronização imediata (ignora conectividade)
    /// </summary>
    Task<SyncResult> ForceSyncAsync();
}

/// <summary>
/// Status da sincronização
/// </summary>
public enum SyncStatus
{
    Idle,           // Parado
    Syncing,        // Sincronizando
    Success,        // Sucesso
    Error,          // Erro
    Cancelled,      // Cancelado
    NoConnection    // Sem conexão
}

/// <summary>
/// Resultado da sincronização
/// </summary>
public class SyncResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public int ClientesProcessados { get; set; }
    public int ProdutosProcessados { get; set; }
    public int VendasProcessadas { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Argumentos do evento de mudança de status
/// </summary>
public class SyncStatusChangedEventArgs : EventArgs
{
    public SyncStatus PreviousStatus { get; set; }
    public SyncStatus CurrentStatus { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Progress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}