namespace FMIAutomation;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		
		// Inicialização mais simples possível - sem dependências externas
		MainPage = new MainPage(new Services.AuthService("https://fmiautomation-60e6e-default-rtdb.firebaseio.com/"));
	}
}