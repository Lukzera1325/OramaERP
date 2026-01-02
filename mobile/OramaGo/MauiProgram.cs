using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OramaGo.Data;
using OramaGo.Services;
using OramaGo.ViewModels;
using FluentValidation;
using OramaGo.Models;
using OramaGo.Validators;

namespace OramaGo;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Configurar banco de dados SQLite
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "oramago.db");
		builder.Services.AddDbContext<OramaGoDbContext>(options =>
			options.UseSqlite($"Data Source={dbPath}"));

		// Registrar serviços
		builder.Services.AddSingleton<INavigationService, NavigationService>();
		builder.Services.AddScoped<IDatabaseService, DatabaseService>();
		builder.Services.AddScoped<IClienteService, ClienteService>();
		builder.Services.AddScoped<IProdutoService, ProdutoService>();
		builder.Services.AddScoped<IVendaService, VendaService>();
		builder.Services.AddScoped<IVendaItemService, VendaItemService>();
		builder.Services.AddScoped<IClienteSyncService, ClienteSyncService>();
		
		// Serviços de autenticação e segurança
		builder.Services.AddHttpClient();
		builder.Services.AddScoped<IAuthService, AuthService>();
		builder.Services.AddScoped<IPermissionService, PermissionService>();
		builder.Services.AddScoped<IUserContextService, UserContextService>();
		builder.Services.AddScoped<IStartupService, StartupService>();
		
		// Serviços de conectividade e sincronização
		builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
		builder.Services.AddSingleton<ISyncService, SyncService>();
		
		// Registrar validadores
		builder.Services.AddScoped<IValidator<VendaLocal>, VendaLocalValidator>();

		// Registrar ViewModels
		builder.Services.AddSingleton<AppShellViewModel>();
		builder.Services.AddTransient<DashboardViewModel>();
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<ClientesListViewModel>();
		builder.Services.AddTransient<ClienteDetailViewModel>();
		builder.Services.AddTransient<ClienteEditViewModel>();
		builder.Services.AddTransient<ProdutosListViewModel>();
		builder.Services.AddTransient<ProdutoDetailViewModel>();
		builder.Services.AddTransient<VendaCreateViewModel>();
		builder.Services.AddTransient<VendasListViewModel>();
		builder.Services.AddTransient<VendaDetailViewModel>();
		
		// Registrar Pages
		builder.Services.AddSingleton<AppShell>();
		builder.Services.AddSingleton<App>();
		builder.Services.AddTransient<Views.DashboardPage>();
		builder.Services.AddTransient<Views.LoginPage>();
		builder.Services.AddTransient<Views.ClientesListPage>();
		builder.Services.AddTransient<Views.ClienteDetailPage>();
		builder.Services.AddTransient<Views.ClienteEditPage>();
		builder.Services.AddTransient<Views.ProdutosListPage>();
		builder.Services.AddTransient<Views.ProdutoDetailPage>();
		builder.Services.AddTransient<Views.VendaCreatePage>();
		builder.Services.AddTransient<Views.VendasListPage>();
		builder.Services.AddTransient<Views.VendaDetailPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();

		// Inicializar banco de dados de forma síncrona
		using (var scope = app.Services.CreateScope())
		{
			var databaseService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
			// Inicialização será feita na primeira execução do app
		}

		return app;
	}
}
