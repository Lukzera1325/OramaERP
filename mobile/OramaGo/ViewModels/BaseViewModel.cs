using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OramaGo.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private bool isRefreshing;

    public virtual async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    public virtual async Task RefreshAsync()
    {
        IsRefreshing = true;
        try
        {
            await LoadDataAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    protected virtual async Task LoadDataAsync()
    {
        await Task.CompletedTask;
    }

    protected void SetBusy(bool busy, string? loadingMessage = null)
    {
        IsBusy = busy;
        if (!string.IsNullOrEmpty(loadingMessage))
        {
            // TODO: Implementar loading message quando necessário
        }
    }
}