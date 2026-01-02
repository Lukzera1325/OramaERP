using System.ComponentModel;

namespace OramaGo.Services;

/// <summary>
/// Interface para serviço de conectividade
/// </summary>
public interface IConnectivityService : INotifyPropertyChanged
{
    /// <summary>
    /// Indica se há conectividade com a internet
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Indica se a API do ERP está acessível
    /// </summary>
    bool IsApiReachable { get; }

    /// <summary>
    /// Status detalhado da conectividade
    /// </summary>
    ConnectivityStatus Status { get; }

    /// <summary>
    /// Evento disparado quando a conectividade muda
    /// </summary>
    event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged;

    /// <summary>
    /// Inicia o monitoramento de conectividade
    /// </summary>
    Task StartMonitoringAsync();

    /// <summary>
    /// Para o monitoramento de conectividade
    /// </summary>
    void StopMonitoring();

    /// <summary>
    /// Testa a conectividade com a API manualmente
    /// </summary>
    Task<bool> TestApiConnectivityAsync();
}

/// <summary>
/// Status da conectividade
/// </summary>
public enum ConnectivityStatus
{
    Unknown,
    Disconnected,
    ConnectedNoInternet,
    ConnectedNoApi,
    Connected
}

/// <summary>
/// Argumentos do evento de mudança de conectividade
/// </summary>
public class ConnectivityChangedEventArgs : EventArgs
{
    public ConnectivityStatus PreviousStatus { get; set; }
    public ConnectivityStatus CurrentStatus { get; set; }
    public bool IsConnected { get; set; }
    public bool IsApiReachable { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}