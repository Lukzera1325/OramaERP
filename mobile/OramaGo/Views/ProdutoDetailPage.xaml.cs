using OramaGo.ViewModels;

namespace OramaGo.Views;

public partial class ProdutoDetailPage : ContentPage
{
    public ProdutoDetailPage(ProdutoDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is ProdutoDetailViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}