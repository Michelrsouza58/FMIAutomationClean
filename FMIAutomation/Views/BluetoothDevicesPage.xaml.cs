using Microsoft.Maui.Controls;
using FMIAutomation.Services;
using FMIAutomation.ViewModels;

namespace FMIAutomation.Views
{
    public partial class BluetoothDevicesPage : ContentPage
    {
        private readonly IBluetoothService _bluetoothService;
        private readonly IPermissionService _permissionService;
        private BluetoothDevicesViewModel _viewModel;
        
        public BluetoothDevicesPage(IBluetoothService bluetoothService, IPermissionService permissionService)
        {
            InitializeComponent();
            
            _bluetoothService = bluetoothService;
            _permissionService = permissionService;
            _viewModel = new BluetoothDevicesViewModel(_bluetoothService, _permissionService);
            this.BindingContext = _viewModel;
            
            // Configurar eventos
            AddDeviceBtn.Clicked += (s, e) => ShowScanModal();
            
            // Inscrever-se para mudanças de tema
            ThemeService.ThemeChanged += OnThemeChanged;
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

        private void ShowScanModal()
        {
            var modal = new BluetoothScanModal();
            // Mock: adicionar dispositivos encontrados
            modal.ScanResults.Add(new Models.BluetoothDevice { Name = "Novo Fone", Address = "33:44:55:66:77:88", IsPaired = false });
            modal.ScanResults.Add(new Models.BluetoothDevice { Name = "Caixa Portátil", Address = "44:55:66:77:88:99", IsPaired = false });

            // Overlay modal
            if (this.Content is Layout main)
            {
                mainLayout = main;
                var overlay = new Grid();
                overlay.Children.Add(main);
                overlay.Children.Add(modal);
                this.Content = overlay;

                modal.Closed += (s, e) =>
                {
                    overlay.Children.Remove(modal);
                };
            }
        }

    private Layout? mainLayout;
    }
}
