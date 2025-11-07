using System.Collections.ObjectModel;
using FMIAutomation.Models;
using FMIAutomation.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace FMIAutomation.ViewModels
{
    public class BluetoothDevicesViewModel : INotifyPropertyChanged
    {
        private readonly IBluetoothService _bluetoothService;
        private readonly IPermissionService _permissionService;
        private bool _isScanning;
        private bool _isLoading;
        private string _statusMessage = "Pronto para escanear dispositivos";

        public ObservableCollection<BluetoothDevice> PairedDevices { get; set; } = new();
        public ObservableCollection<BluetoothDevice> DiscoveredDevices { get; set; } = new();
        
        public bool IsScanning
        {
            get => _isScanning;
            set
            {
                if (_isScanning != value)
                {
                    _isScanning = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ScanButtonText));
                }
            }
        }
        
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public string ScanButtonText => IsScanning ? "Parar Scan" : "Escanear";
        
        public ICommand ScanCommand { get; }
        public ICommand PairDeviceCommand { get; }
        public ICommand ConnectDeviceCommand { get; }
        public ICommand DisconnectDeviceCommand { get; }
        public ICommand UnpairDeviceCommand { get; }
        public ICommand RefreshCommand { get; }

        public BluetoothDevicesViewModel(IBluetoothService bluetoothService, IPermissionService permissionService)
        {
            _bluetoothService = bluetoothService;
            _permissionService = permissionService;
            
            // Comandos
            ScanCommand = new Command(async () => await ExecuteScanCommand());
            PairDeviceCommand = new Command<BluetoothDevice>(async (device) => await ExecutePairCommand(device));
            ConnectDeviceCommand = new Command<BluetoothDevice>(async (device) => await ExecuteConnectCommand(device));
            DisconnectDeviceCommand = new Command<BluetoothDevice>(async (device) => await ExecuteDisconnectCommand(device));
            UnpairDeviceCommand = new Command<BluetoothDevice>(async (device) => await ExecuteUnpairCommand(device));
            RefreshCommand = new Command(async () => await ExecuteRefreshCommand());
            
            // Eventos do serviço Bluetooth
            _bluetoothService.DeviceDiscovered += OnDeviceDiscovered;
            _bluetoothService.ScanStatusChanged += OnScanStatusChanged;
            _bluetoothService.DeviceConnectionChanged += OnDeviceConnectionChanged;
            
            // Carrega dispositivos inicial
            _ = Task.Run(LoadInitialData);
        }

        private async Task LoadInitialData()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Carregando dispositivos pareados...";
                
                var pairedDevices = await _bluetoothService.GetPairedDevicesAsync();
                
                Application.Current?.Dispatcher.Dispatch(() =>
                {
                    PairedDevices.Clear();
                    foreach (var device in pairedDevices)
                    {
                        PairedDevices.Add(device);
                    }
                });
                
                StatusMessage = "Dispositivos carregados";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao carregar dispositivos: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[VIEWMODEL] Erro ao carregar dispositivos: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task ExecuteScanCommand()
        {
            try
            {
                if (IsScanning)
                {
                    await _bluetoothService.StopScanAsync();
                }
                else
                {
                    StatusMessage = "Verificando permissões...";
                    
                    // Verificar permissões primeiro
                    var hasPermissions = await _permissionService.CheckBluetoothPermissionsAsync();
                    
                    if (!hasPermissions)
                    {
                        StatusMessage = "Solicitando permissões...";
                        var permissionsGranted = await _permissionService.RequestBluetoothPermissionsAsync();
                        
                        if (!permissionsGranted)
                        {
                            StatusMessage = "Permissões necessárias não concedidas. Scan cancelado.";
                            return;
                        }
                    }
                    
                    StatusMessage = "Iniciando scan...";
                    var success = await _bluetoothService.StartScanAsync();
                    
                    if (!success)
                    {
                        StatusMessage = "Falha ao iniciar scan. Verifique as permissões.";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro no scan: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[VIEWMODEL] Erro no scan: {ex.Message}");
            }
        }
        
        private async Task ExecutePairCommand(BluetoothDevice? device)
        {
            if (device == null) return;
            
            try
            {
                StatusMessage = $"Pareando com {device.Name}...";
                var success = await _bluetoothService.PairDeviceAsync(device);
                
                if (success)
                {
                    StatusMessage = $"Pareado com {device.Name}";
                    
                    Application.Current?.Dispatcher.Dispatch(() =>
                    {
                        DiscoveredDevices.Remove(device);
                        PairedDevices.Add(device);
                    });
                }
                else
                {
                    StatusMessage = $"Falha ao parear com {device.Name}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao parear: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[VIEWMODEL] Erro ao parear: {ex.Message}");
            }
        }
        
        private async Task ExecuteConnectCommand(BluetoothDevice? device)
        {
            if (device == null) return;
            
            try
            {
                StatusMessage = $"Conectando com {device.Name}...";
                var success = await _bluetoothService.ConnectDeviceAsync(device);
                
                if (success)
                {
                    StatusMessage = $"Conectado com {device.Name}";
                }
                else
                {
                    StatusMessage = $"Falha ao conectar com {device.Name}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao conectar: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[VIEWMODEL] Erro ao conectar: {ex.Message}");
            }
        }
        
        private async Task ExecuteDisconnectCommand(BluetoothDevice? device)
        {
            if (device == null) return;
            
            try
            {
                StatusMessage = $"Desconectando de {device.Name}...";
                var success = await _bluetoothService.DisconnectDeviceAsync(device);
                
                if (success)
                {
                    StatusMessage = $"Desconectado de {device.Name}";
                }
                else
                {
                    StatusMessage = $"Falha ao desconectar de {device.Name}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao desconectar: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[VIEWMODEL] Erro ao desconectar: {ex.Message}");
            }
        }
        
        private async Task ExecuteUnpairCommand(BluetoothDevice? device)
        {
            if (device == null) return;
            
            try
            {
                StatusMessage = $"Removendo pareamento com {device.Name}...";
                var success = await _bluetoothService.UnpairDeviceAsync(device);
                
                if (success)
                {
                    StatusMessage = $"Pareamento removido: {device.Name}";
                    
                    Application.Current?.Dispatcher.Dispatch(() =>
                    {
                        PairedDevices.Remove(device);
                    });
                }
                else
                {
                    StatusMessage = $"Falha ao remover pareamento com {device.Name}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao remover pareamento: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[VIEWMODEL] Erro ao remover pareamento: {ex.Message}");
            }
        }
        
        private async Task ExecuteRefreshCommand()
        {
            await LoadInitialData();
        }
        
        private void OnDeviceDiscovered(object? sender, BluetoothDevice device)
        {
            Application.Current?.Dispatcher.Dispatch(() =>
            {
                if (!DiscoveredDevices.Contains(device) && !PairedDevices.Contains(device))
                {
                    DiscoveredDevices.Add(device);
                }
            });
            
            StatusMessage = $"Dispositivo encontrado: {device.Name}";
        }
        
        private void OnScanStatusChanged(object? sender, bool isScanning)
        {
            IsScanning = isScanning;
            
            if (isScanning)
            {
                StatusMessage = "Escaneando dispositivos...";
                Application.Current?.Dispatcher.Dispatch(() =>
                {
                    DiscoveredDevices.Clear();
                });
            }
            else
            {
                StatusMessage = $"Scan finalizado. {DiscoveredDevices.Count} dispositivos encontrados.";
            }
        }
        
        private void OnDeviceConnectionChanged(object? sender, BluetoothDevice device)
        {
            StatusMessage = device.IsConnected ? 
                $"Conectado com {device.Name}" : 
                $"Desconectado de {device.Name}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
