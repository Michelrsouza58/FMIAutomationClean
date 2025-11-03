using Microsoft.Extensions.Logging;

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

	// Registrar MainPage para DI
	builder.Services.AddTransient<MainPage>();

	return builder.Build();
    }
}
