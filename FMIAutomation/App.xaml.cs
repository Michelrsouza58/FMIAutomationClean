using FMIAutomation.Services;

namespace FMIAutomation;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		
		// Inicializar tema antes de criar a página principal
		_ = InitializeThemeAsync();
		
		// Inicialização mais simples possível - sem dependências externas
		MainPage = new MainPage(new Services.AuthService("https://fmiautomation-60e6e-default-rtdb.firebaseio.com/"));
	}

	private async Task InitializeThemeAsync()
	{
		try
		{
			// Carrega e aplica o tema salvo
			var savedTheme = await ThemeService.GetCurrentThemeAsync();
			ThemeService.ApplyTheme(savedTheme);
			
			System.Diagnostics.Debug.WriteLine($"[App] Tema inicializado: {savedTheme}");
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[App] Erro ao inicializar tema: {ex.Message}");
		}
	}
}