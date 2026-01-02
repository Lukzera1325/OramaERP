using OramaGo.ViewModels;

namespace OramaGo.Views;

public partial class ClienteEditPage : ContentPage
{
	public ClienteEditPage(ClienteEditViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		
		if (BindingContext is ClienteEditViewModel viewModel)
		{
			await viewModel.InitializeAsync();
		}
	}
}