using OramaGo.Services;

namespace OramaGo;

public partial class App : Application
{
	private readonly IStartupService _startupService;

	public App(AppShell appShell, IStartupService startupService)
	{
		InitializeComponent();
		MainPage = appShell;
		_startupService = startupService;
	}

	protected override async void OnStart()
	{
		base.OnStart();
		await _startupService.InitializeAsync();
	}
}
