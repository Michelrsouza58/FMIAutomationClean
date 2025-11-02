using Microsoft.Maui.Controls;

namespace FMIAutomation.Views
{
    public partial class BluetoothDevicesPage : ContentPage
    {
        public BluetoothDevicesPage()
        {
            InitializeComponent();
            var vm = new ViewModels.BluetoothDevicesViewModel();
            this.BindingContext = vm;
            DevicesCollection.ItemsSource = vm.Devices;
            AddDeviceBtn.Clicked += (s, e) => ShowScanModal();
        }

        private void ShowScanModal()
        {
            var modal = new BluetoothScanModal();
            // Mock: adicionar dispositivos encontrados
            modal.ScanResults.Add(new Models.BluetoothDevice { Name = "Novo Fone", Address = "33:44:55:66:77:88", IsPaired = false });
            modal.ScanResults.Add(new Models.BluetoothDevice { Name = "Caixa Portátil", Address = "44:55:66:77:88:99", IsPaired = false });
            modal.Closed += (s, e) => this.Content = mainLayout;

            // Overlay modal
            if (this.Content is Layout main)
            {
                mainLayout = main;
                var overlay = new Grid();
                overlay.Children.Add(main);
                overlay.Children.Add(modal);
                this.Content = overlay;
            }
        }

        private Layout mainLayout;
    }
}
