namespace FMIAutomation;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		MainPage = CreateStartupPage();
	}

	private Page CreateStartupPage()
	{
		var isLogged = IsUserLoggedIn();
		if (isLogged)
			return new AppShell();
		else
		{
			// Resolve MainPage via DI (MAUI)
			var mainPage = (MainPage?)Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services.GetService(typeof(MainPage));
			return mainPage ?? new MainPage(new Services.AuthService("https://fmiautomation-60e6e-default-rtdb.firebaseio.com/"));
		}
	}

	private bool IsUserLoggedIn()
	{
		try
		{
			var loginTimeStr = Microsoft.Maui.Storage.SecureStorage.GetAsync("login_time").GetAwaiter().GetResult();
			if (string.IsNullOrEmpty(loginTimeStr))
				return false;
			if (!long.TryParse(loginTimeStr, out var loginTicks))
				return false;
			var loginTime = new DateTime(loginTicks);
			// 15 minutos de validade
			if ((DateTime.UtcNow - loginTime).TotalMinutes > 15)
				return false;
			return true;
		}
		catch
		{
			return false;
		}
	}
}