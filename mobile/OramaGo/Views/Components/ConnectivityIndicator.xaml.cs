using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using OramaGo.Services;

namespace OramaGo.Views.Components;

/// <summary>
/// Componente visual para indicar status de conectividade
/// </summary>
public partial class ConnectivityIndicator : ContentView, INotifyPropertyChanged
{
    private readonly IConnectivityService _connectivityService;
    private Color _statusColor = Colors.Gray;
    private string _statusIcon = "wifi_off";
    private string _statusText = "Desconhecido";
    private bool _showText = true;

    public ConnectivityIndicator()
    {
        InitializeComponent();
        
        // Obter serviço de conectividade
        _connectivityService = ServiceHelper.GetService<IConnectivityService>();
        
        // Registrar eventos
        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
        _connectivityService.PropertyChanged += OnConnectivityServicePropertyChanged;
        
        // Atualizar status inicial
        UpdateStatus();
        
        // Comando para mostrar detalhes
        ShowDetailsCommand = new Command(async () => await ShowDetailsAsync());
    }

    #region Propriedades Bindáveis

    public Color StatusColor
    {
        get => _statusColor;
        private set
        {
            if (_statusColor != value)
            {
                _statusColor = value;
                OnPropertyChanged();
            }
        }
    }

    public string StatusIcon
    {
        get => _statusIcon;
        private set
        {
            if (_statusIcon != value)
            {
                _statusIcon = value;
                OnPropertyChanged();
            }
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set
        {
            if (_statusText != value)
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }
    }

    public bool ShowText
    {
        get => _showText;
        set
        {
            if (_showText != value)
            {
                _showText = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand ShowDetailsCommand { get; }

    #endregion

    /// <summary>
    /// Atualiza o status visual baseado na conectividade
    /// </summary>
    private void UpdateStatus()
    {
        var status = _connectivityService.Status;
        var isConnected = _connectivityService.IsConnected;
        var isApiReachable = _connectivityService.IsApiReachable;

        switch (status)
        {
            case ConnectivityStatus.Connected:
                StatusColor = Color.FromArgb("#4CAF50"); // Verde
                StatusIcon = "wifi";
                StatusText = "Online";
                break;

            case ConnectivityStatus.ConnectedNoApi:
                StatusColor = Color.FromArgb("#FF9800"); // Laranja
                StatusIcon = "wifi_tethering_error";
                StatusText = "Sem API";
                break;

            case ConnectivityStatus.ConnectedNoInternet:
                StatusColor = Color.FromArgb("#FF9800"); // Laranja
                StatusIcon = "wifi_tethering_error";
                StatusText = "Sem Internet";
                break;

            case ConnectivityStatus.Disconnected:
                StatusColor = Color.FromArgb("#F44336"); // Vermelho
                StatusIcon = "wifi_off";
                StatusText = "Offline";
                break;

            case ConnectivityStatus.Unknown:
            default:
                StatusColor = Color.FromArgb("#9E9E9E"); // Cinza
                StatusIcon = "help_outline";
                StatusText = "Desconhecido";
                break;
        }
    }

    /// <summary>
    /// Mostra detalhes da conectividade
    /// </summary>
    private async Task ShowDetailsAsync()
    {
        var status = _connectivityService.Status;
        var isConnected = _connectivityService.IsConnected;
        var isApiReachable = _connectivityService.IsApiReachable;

        var message = status switch
        {
            ConnectivityStatus.Connected => 
                "✅ Conectado\n\n• Internet: Disponível\n• API do ERP: Acessível\n• Sincronização: Ativa",
            
            ConnectivityStatus.ConnectedNoApi => 
                "⚠️ Conectado (Sem API)\n\n• Internet: Disponível\n• API do ERP: Inacessível\n• Sincronização: Pausada",
            
            ConnectivityStatus.ConnectedNoInternet => 
                "⚠️ Conectado (Sem Internet)\n\n• Rede: Conectada\n• Internet: Indisponível\n• Sincronização: Pausada",
            
            ConnectivityStatus.Disconnected => 
                "❌ Desconectado\n\n• Rede: Desconectada\n• Internet: Indisponível\n• Modo: Offline",
            
            _ => "❓ Status Desconhecido\n\nVerificando conectividade..."
        };

        await Application.Current.MainPage.DisplayAlert("Status de Conectividade", message, "OK");
    }

    #region Event Handlers

    private void OnConnectivityChanged(object sender, OramaGo.Services.ConnectivityChangedEventArgs e)
    {
        // Atualizar na thread principal
        MainThread.BeginInvokeOnMainThread(UpdateStatus);
    }

    private void OnConnectivityServicePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Atualizar quando propriedades do serviço mudarem
        MainThread.BeginInvokeOnMainThread(UpdateStatus);
    }

    #endregion

    #region INotifyPropertyChanged

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        
        if (Handler == null)
        {
            // Cleanup quando o componente for removido
            _connectivityService.ConnectivityChanged -= OnConnectivityChanged;
            _connectivityService.PropertyChanged -= OnConnectivityServicePropertyChanged;
        }
    }
}

/// <summary>
/// Helper para obter serviços
/// </summary>
public static class ServiceHelper
{
    public static T GetService<T>() => Current.GetService<T>();

    public static IServiceProvider Current =>
#if WINDOWS10_0_17763_0_OR_GREATER
        MauiWinUIApplication.Current.Services;
#elif ANDROID
        MauiApplication.Current.Services;
#elif IOS || MACCATALYST
        MauiUIApplicationDelegate.Current.Services;
#else
        null;
#endif
}