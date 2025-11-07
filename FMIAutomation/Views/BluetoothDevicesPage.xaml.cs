using Microsoft.Maui.Controls;
using FMIAutomation.Services;

namespace FMIAutomation.Views
{
    public partial class BluetoothDevicesPage : ContentPage
    {
        public BluetoothDevicesPage()
        {
            InitializeComponent();
            var vm = new ViewModels.BluetoothDevicesViewModel();
            this.BindingContext = vm;
            
            // Configurar eventos dos botões
            ScanDevicesBtn.Clicked += async (s, e) => await StartBluetoothScan();
            
            // Inscrever-se para mudanças de tema
            ThemeService.ThemeChanged += OnThemeChanged;
            
            LoadMockDevices();
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

        private async Task StartBluetoothScan()
        {
            try
            {
                // Mostrar indicador de scan
                ScanStatusFrame.IsVisible = true;
                ScanIndicator.IsRunning = true;
                ScanStatusLabel.Text = "Procurando dispositivos Bluetooth...";
                EmptyStateSection.IsVisible = false;

                // Simular scan (substituir por implementação real)
                await Task.Delay(3000);

                // Mock de dispositivos encontrados
                var foundDevices = new List<Models.BluetoothDevice>
                {
                    new Models.BluetoothDevice { Name = "Fone JBL", Address = "00:11:22:33:44:55", IsPaired = false },
                    new Models.BluetoothDevice { Name = "Caixa Sony", Address = "11:22:33:44:55:66", IsPaired = false },
                    new Models.BluetoothDevice { Name = "Mouse BT", Address = "22:33:44:55:66:77", IsPaired = false }
                };

                // Atualizar UI com dispositivos encontrados
                AvailableDevicesCollection.ItemsSource = foundDevices;
                
                // Esconder indicador de scan
                ScanIndicator.IsRunning = false;
                ScanStatusLabel.Text = $"Encontrados {foundDevices.Count} dispositivos";
                
                await Task.Delay(2000);
                ScanStatusFrame.IsVisible = false;
                
                if (!foundDevices.Any())
                {
                    EmptyStateSection.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                ScanIndicator.IsRunning = false;
                ScanStatusLabel.Text = $"Erro no scan: {ex.Message}";
                await DisplayAlert("Erro", $"Falha ao escanear dispositivos: {ex.Message}", "OK");
            }
        }

        private void LoadMockDevices()
        {
            // Dispositivos conectados (mock)
            var connectedDevices = new List<Models.BluetoothDevice>
            {
                new Models.BluetoothDevice { Name = "Dispositivo ESP32", Address = "AA:BB:CC:DD:EE:FF", IsPaired = true }
            };
            
            ConnectedDevicesCollection.ItemsSource = connectedDevices;
            
            // Se não há dispositivos, mostrar estado vazio
            if (!connectedDevices.Any())
            {
                EmptyStateSection.IsVisible = true;
            }
        }
    }
}
