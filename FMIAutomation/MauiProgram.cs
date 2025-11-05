using Microsoft.Extensions.Logging;
using FMIAutomation.Services;

namespace FMIAutomation;

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

#if DEBUG
	builder.Logging.AddDebug();
#endif

		// Registrar AuthService como singleton
		string firebaseUrl = "https://fmiautomation-60e6e-default-rtdb.firebaseio.com/";
		builder.Services.AddSingleton<Services.IAuthService>(sp => new Services.AuthService(firebaseUrl));

		// Registrar Pages para DI
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<Views.ProfilePage>();

		var app = builder.Build();

		// Inicializar tema na inicialização do app
		Task.Run(async () =>
		{
			await ThemeService.InitializeAsync();
		});

		return app;
    }
}
