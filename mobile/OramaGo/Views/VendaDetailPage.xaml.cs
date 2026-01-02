using OramaGo.ViewModels;

namespace OramaGo.Views;

public partial class VendaDetailPage : ContentPage
{
    public VendaDetailPage(VendaDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is VendaDetailViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}