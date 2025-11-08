using FMIAutomation.Services;

namespace FMIAutomation;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		System.Diagnostics.Debug.WriteLine("[MauiProgram] === VERSÃO COM SERVIÇOS BÁSICOS ===");
		
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
		
		System.Diagnostics.Debug.WriteLine("[MauiProgram] Fontes configuradas");

		// Adicionando serviços essenciais gradualmente
		System.Diagnostics.Debug.WriteLine("[MauiProgram] Registrando serviços essenciais...");
		
		// AuthService
		string firebaseUrl = "https://fmiautomation-60e6e-default-rtdb.firebaseio.com/";
		builder.Services.AddSingleton<IAuthService>(sp => new AuthService(firebaseUrl));
		
		// SessionService
		builder.Services.AddSingleton<ISessionService, SessionService>();
		System.Diagnostics.Debug.WriteLine("[MauiProgram] SessionService registrado");
		
		// Serviços de Bluetooth e Permissões
		builder.Services.AddSingleton<IPermissionService, PermissionService>();
		builder.Services.AddSingleton<IBluetoothService, BluetoothService>();
		System.Diagnostics.Debug.WriteLine("[MauiProgram] Serviços Bluetooth registrados");
		
		// Páginas
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<Views.ProfilePage>();
		builder.Services.AddTransient<Views.BluetoothDevicesPage>();
		System.Diagnostics.Debug.WriteLine("[MauiProgram] Páginas registradas no DI");

		System.Diagnostics.Debug.WriteLine("[MauiProgram] Construindo app...");
		var app = builder.Build();

		// Inicializar tema na inicialização do app (sem await para não bloquear)
		Task.Run(async () =>
		{
			try
			{
				await ThemeService.InitializeAsync();
				System.Diagnostics.Debug.WriteLine("[MauiProgram] Tema inicializado");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[MauiProgram] Erro ao inicializar tema: {ex.Message}");
			}
		});

		return app;
	}
}