using OramaGo.ViewModels;
using OramaGo.Views;

namespace OramaGo;

public partial class AppShell : Shell
{
	public AppShell(AppShellViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
		
		// Registrar rotas para navegação
		RegisterRoutes();
	}

	private void RegisterRoutes()
	{
		// Rota para login
		Routing.RegisterRoute("login", typeof(LoginPage));
		
		// Rotas de clientes
		Routing.RegisterRoute("clientes/detalhes", typeof(ClienteDetailPage));
		Routing.RegisterRoute("clientes/editar", typeof(ClienteEditPage));
		
		// Rotas de produtos
		Routing.RegisterRoute("produtos", typeof(ProdutosListPage));
		Routing.RegisterRoute("produtos/detalhes", typeof(ProdutoDetailPage));
		
		// Rotas de vendas
		Routing.RegisterRoute("vendas", typeof(VendasListPage));
		Routing.RegisterRoute("vendas/criar", typeof(VendaCreatePage));
		Routing.RegisterRoute("vendas/editar", typeof(VendaCreatePage));
		Routing.RegisterRoute("vendas/detalhes", typeof(VendaDetailPage));
		// Routing.RegisterRoute("vendas", typeof(VendasListPage));
		// Routing.RegisterRoute("vendas/criar", typeof(VendaCreatePage));
		// Routing.RegisterRoute("vendas/detalhes", typeof(VendaDetailPage));
		// Routing.RegisterRoute("atividades", typeof(AtividadesListPage));
		// Routing.RegisterRoute("settings", typeof(SettingsPage));
	}
}
