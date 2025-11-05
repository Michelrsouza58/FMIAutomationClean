using Microsoft.Maui.Controls;
using FMIAutomation.Services;

namespace FMIAutomation.Views
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
            
            // Inscrever-se para mudanças de tema
            ThemeService.ThemeChanged += OnThemeChanged;
            
            // Configurar eventos de toque nos dispositivos
            Device1Tap.Tapped += async (s, e) => await NavigateToDeviceControl("ESP32 Suínos", true);
            Device2Tap.Tapped += async (s, e) => await NavigateToDeviceControl("ESP32 Aves", false);
            Device3Tap.Tapped += async (s, e) => await NavigateToDeviceControl("Controle Geral", true);
            Device4Tap.Tapped += async (s, e) => await NavigateToDeviceControl("Monitoramento", true);
            
            // Configurar botão flutuante para adicionar dispositivo
            AddDeviceFloatingButton.Clicked += async (s, e) => await NavigateToBluetoothScan();
        }

        private void OnThemeChanged(object? sender, ThemeService.AppTheme newTheme)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Força a atualização das cores
                this.BackgroundColor = (Color)Application.Current!.Resources["BackgroundColor"];
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Desinscrever-se do evento para evitar vazamentos de memória
            ThemeService.ThemeChanged -= OnThemeChanged;
        }

        private async Task NavigateToDeviceControl(string deviceName, bool isOnline)
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "deviceName", deviceName },
                    { "isOnline", isOnline }
                };
                
                // Teste primeiro sem os parâmetros para ver se funciona
                await Shell.Current.GoToAsync("devicecontrol", parameters);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Não foi possível navegar para o controle do dispositivo: {ex.Message}", "OK");
            }
        }
        
        private async Task NavigateToBluetoothScan()
        {
            try
            {
                await Shell.Current.GoToAsync("bluetoothscan");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao abrir scanner Bluetooth: {ex.Message}", "OK");
            }
        }

        private void AddTapGestures()
        {
            // Gestos já configurados no constructor
        }

        private async Task LogoutAsync()
        {
            // Limpar sessão
            try { Microsoft.Maui.Storage.SecureStorage.Remove("login_time"); } catch {}
            // Reiniciar o app para remover o Shell e exibir só a tela de login
            var mainPage = (FMIAutomation.MainPage?)Application.Current?.Handler?.MauiContext?.Services.GetService(typeof(FMIAutomation.MainPage));
            if (Application.Current != null)
            {
                Application.Current.MainPage = mainPage ?? new FMIAutomation.MainPage(new FMIAutomation.Services.AuthService("https://fmiautomation-60e6e-default-rtdb.firebaseio.com/"));
            }
            await Task.CompletedTask; // Para resolver o warning CS1998
        }
    }
}
