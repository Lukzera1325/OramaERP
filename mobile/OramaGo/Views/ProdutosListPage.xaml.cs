using OramaGo.ViewModels;

namespace OramaGo.Views;

public partial class ProdutosListPage : ContentPage
{
	public ProdutosListPage(ProdutosListViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		
		if (BindingContext is ProdutosListViewModel viewModel)
		{
			await viewModel.InitializeAsync();
		}
	}
}