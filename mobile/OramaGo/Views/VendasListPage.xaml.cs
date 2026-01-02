using OramaGo.ViewModels;

namespace OramaGo.Views;

public partial class VendasListPage : ContentPage
{
    public VendasListPage(VendasListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is VendasListViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}