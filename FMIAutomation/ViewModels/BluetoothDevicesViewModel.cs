using System.Collections.ObjectModel;
using FMIAutomation.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FMIAutomation.ViewModels
{
    public class BluetoothDevicesViewModel : INotifyPropertyChanged
    {
    public ObservableCollection<BluetoothDevice> Devices { get; set; } = new();
    public event PropertyChangedEventHandler? PropertyChanged;

        public BluetoothDevicesViewModel()
        {
            // Mock inicial
            Devices.Add(new BluetoothDevice { Name = "Fone JBL", Address = "00:11:22:33:44:55", IsPaired = true });
            Devices.Add(new BluetoothDevice { Name = "Caixa Sony", Address = "11:22:33:44:55:66", IsPaired = true });
            Devices.Add(new BluetoothDevice { Name = "Teclado BT", Address = "22:33:44:55:66:77", IsPaired = false });
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
