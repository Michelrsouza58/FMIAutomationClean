using System.ComponentModel;

namespace FMIAutomation.Models
{
    public enum BluetoothDeviceType
    {
        Unknown,
        Phone,
        Computer,
        Audio,
        Input,
        Headset,
        Camera,
        Printer,
        Wearable,
        BLE,
        Classic
    }
    
    public class BluetoothDevice : INotifyPropertyChanged
    {
        private bool _isConnected;
        private bool _isPaired;
        private int _signalStrength;
        
        public string? Name { get; set; }
        public string? Address { get; set; }
        
        public bool IsPaired 
        { 
            get => _isPaired;
            set 
            { 
                if (_isPaired != value)
                {
                    _isPaired = value;
                    OnPropertyChanged(nameof(IsPaired));
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }
        
        public bool IsConnected 
        { 
            get => _isConnected;
            set 
            { 
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged(nameof(IsConnected));
                    OnPropertyChanged(nameof(StatusText));
                    OnPropertyChanged(nameof(ConnectionStatusColor));
                }
            }
        }
        
        public int SignalStrength 
        { 
            get => _signalStrength;
            set 
            { 
                if (_signalStrength != value)
                {
                    _signalStrength = value;
                    OnPropertyChanged(nameof(SignalStrength));
                    OnPropertyChanged(nameof(SignalStrengthText));
                }
            }
        }
        
        public BluetoothDeviceType DeviceType { get; set; } = BluetoothDeviceType.Unknown;
        
        public DateTime LastSeen { get; set; } = DateTime.Now;
        
        // Propriedades calculadas para a UI
        public string StatusText => IsConnected ? "Conectado" : (IsPaired ? "Pareado" : "Disponível");
        
        public Color ConnectionStatusColor => IsConnected ? Colors.Green : (IsPaired ? Colors.Blue : Colors.Gray);
        
        public string SignalStrengthText 
        { 
            get 
            { 
                if (SignalStrength >= -50) return "Excelente";
                if (SignalStrength >= -60) return "Bom";
                if (SignalStrength >= -70) return "Regular";
                return "Fraco";
            }
        }
        
        public string DeviceIcon 
        { 
            get 
            {
                return DeviceType switch
                {
                    BluetoothDeviceType.Phone => "📱",
                    BluetoothDeviceType.Computer => "💻",
                    BluetoothDeviceType.Audio => "🎧",
                    BluetoothDeviceType.Headset => "🎤",
                    BluetoothDeviceType.Input => "⌨️",
                    BluetoothDeviceType.Camera => "📷",
                    BluetoothDeviceType.Printer => "🖨️",
                    BluetoothDeviceType.Wearable => "⌚",
                    _ => "📶"
                };
            }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public override string ToString()
        {
            return $"{Name} ({Address}) - {StatusText}";
        }
        
        public override bool Equals(object? obj)
        {
            if (obj is BluetoothDevice other)
            {
                return Address?.Equals(other.Address, StringComparison.OrdinalIgnoreCase) == true;
            }
            return false;
        }
        
        public override int GetHashCode()
        {
            return Address?.GetHashCode() ?? 0;
        }
    }
}
