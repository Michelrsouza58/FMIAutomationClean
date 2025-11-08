namespace FMIAutomation;

public partial class App : Application
{
	public App()
	{
		try
		{
			System.Diagnostics.Debug.WriteLine("[App] === RESTAURANDO MAINPAGE === Iniciando App...");
			
			System.Diagnostics.Debug.WriteLine("[App] Chamando InitializeComponent...");
			InitializeComponent();
			System.Diagnostics.Debug.WriteLine("[App] InitializeComponent OK");
			
			System.Diagnostics.Debug.WriteLine("[App] Criando MainPage real...");
			// Agora vamos tentar usar a MainPage real
			MainPage = new MainPage(new Services.AuthService("https://fmiautomation-60e6e-default-rtdb.firebaseio.com/"));
			
			System.Diagnostics.Debug.WriteLine("[App] MainPage real criada com sucesso!");
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[App] ERRO CRÍTICO: {ex.GetType().Name}");
			System.Diagnostics.Debug.WriteLine($"[App] Mensagem: {ex.Message}");
			System.Diagnostics.Debug.WriteLine($"[App] StackTrace: {ex.StackTrace}");
			
			if (ex.InnerException != null)
			{
				System.Diagnostics.Debug.WriteLine($"[App] InnerException: {ex.InnerException.GetType().Name}");
				System.Diagnostics.Debug.WriteLine($"[App] InnerMessage: {ex.InnerException.Message}");
			}
			
			throw;
		}
	}


}