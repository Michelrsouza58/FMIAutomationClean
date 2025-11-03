using Microsoft.Maui.Controls;

namespace FMIAutomation.Views
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
            AddTapGestures();
        }

        private void AddTapGestures()
        {
            // Home (não faz nada, já está na Home)
            var homeFrame = this.FindByName<Frame>("HomeFrame");
            if (homeFrame != null)
                homeFrame.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => { /* já está na Home */ }) });

            // Meus Dispositivos
            var devicesFrame = this.FindByName<Frame>("DevicesFrame");
            if (devicesFrame != null)
                devicesFrame.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await Shell.Current.GoToAsync("//BluetoothDevicesPage")) });

            // Setup
            var setupFrame = this.FindByName<Frame>("SetupFrame");
            if (setupFrame != null)
                setupFrame.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await Shell.Current.GoToAsync("//ProfilePage")) });

            // Logout
            var logoutFrame = this.FindByName<Frame>("LogoutFrame");
            if (logoutFrame != null)
                logoutFrame.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await LogoutAsync()) });
        }

        private async Task LogoutAsync()
        {
            // Limpar sessão
            try { Microsoft.Maui.Storage.SecureStorage.Remove("login_time"); } catch {}
            // Reiniciar o app para remover o Shell e exibir só a tela de login
            var mainPage = (FMIAutomation.MainPage?)Application.Current?.Handler?.MauiContext?.Services.GetService(typeof(FMIAutomation.MainPage));
            Application.Current.MainPage = mainPage ?? new FMIAutomation.MainPage(new FMIAutomation.Services.AuthService("https://fmiautomation-60e6e-default-rtdb.firebaseio.com/"));
        }
    }
}
