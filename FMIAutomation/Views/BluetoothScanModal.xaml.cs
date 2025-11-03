using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using FMIAutomation.Models;

namespace FMIAutomation.Views
{
    public partial class BluetoothScanModal : ContentView
    {
        public ObservableCollection<BluetoothDevice> ScanResults { get; set; } = new();
    public event EventHandler? Closed;

        public BluetoothScanModal()
        {
            InitializeComponent();
            ScanResultsCollection.ItemsSource = ScanResults;
            CloseBtn.Clicked += (s, e) => Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}
