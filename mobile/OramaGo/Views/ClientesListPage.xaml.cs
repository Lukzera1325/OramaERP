using OramaGo.ViewModels;

namespace OramaGo.Views;

public partial class ClientesListPage : ContentPage
{
    public ClientesListPage(ClientesListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is ClientesListViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}