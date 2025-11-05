using FMIAutomation.Views;

namespace FMIAutomation;

public partial class AppShell : Shell
{
	   public AppShell()
	   {
		   InitializeComponent();
		   
		   // Registra rotas para navegação
		   Routing.RegisterRoute("MainPage", typeof(MainPage));
		   Routing.RegisterRoute("EditProfilePage", typeof(EditProfilePage));
		   Routing.RegisterRoute("ChangePasswordPage", typeof(ChangePasswordPage));
		   Routing.RegisterRoute("PreferencesPage", typeof(PreferencesPage));
		   Routing.RegisterRoute("devicecontrol", typeof(DeviceControlPage));
		   Routing.RegisterRoute("bluetoothscan", typeof(BluetoothDevicesPage));
		   
		   // Interceptar navegação para logout
		   Navigating += OnShellNavigating;
	   }
	   
	   private async void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
	   {
		   if (e.Target.Location.OriginalString.Contains("LogoutPage"))
		   {
			   e.Cancel();
			   await HandleLogout();
		   }
	   }
	   
	   private async Task HandleLogout()
	   {
		   var result = await DisplayAlert("Confirmar Logout", 
			   "Tem certeza que deseja sair do aplicativo?", 
			   "Sim", "Cancelar");
			   
		   if (result)
		   {
			   // Limpar dados de sessão
			   try 
			   { 
				   Microsoft.Maui.Storage.SecureStorage.Remove("login_time");
				   Microsoft.Maui.Storage.SecureStorage.Remove("user_email");
			   } 
			   catch { }
			   
			   // Voltar para a tela de login
			   Application.Current!.MainPage = new MainPage(
				   new FMIAutomation.Services.AuthService("https://fmiautomation-60e6e-default-rtdb.firebaseio.com/")
			   );
		   }
	   }
}
