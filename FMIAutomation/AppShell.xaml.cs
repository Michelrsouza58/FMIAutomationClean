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
	   }
}
