using OramaGo.ViewModels;

namespace OramaGo.Views;

public partial class VendaCreatePage : ContentPage
{
    public VendaCreatePage(VendaCreateViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is VendaCreateViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}