using OramaGo.ViewModels;

namespace OramaGo.Views;

public partial class ClienteDetailPage : ContentPage
{
    public ClienteDetailPage(ClienteDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is ClienteDetailViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}