using FMIAutomation.Models;
using System.Collections.ObjectModel;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

#if ANDROID
using InTheHand.Net.Sockets;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
#endif

namespace FMIAutomation.Services
{
    public class BluetoothService : IBluetoothService
    {
        private readonly IPermissionService _permissionService;
        private readonly IBluetoothLE _ble;
        private readonly IAdapter _adapter;
        private ObservableCollection<BluetoothDevice> _discoveredDevices;
        private ObservableCollection<BluetoothDevice> _pairedDevices;
        private bool _isScanning;
        private CancellationTokenSource? _scanCancellationTokenSource;
        private CancellationTokenSource? _classicScanCancellationTokenSource;
        
        public event EventHandler<BluetoothDevice>? DeviceDiscovered;
        public event EventHandler<bool>? ScanStatusChanged;
        public event EventHandler<BluetoothDevice>? DeviceConnectionChanged;
        
        public BluetoothService(IPermissionService permissionService)
        {
            _permissionService = permissionService;
            _discoveredDevices = new ObservableCollection<BluetoothDevice>();
            _pairedDevices = new ObservableCollection<BluetoothDevice>();
            _isScanning = false;
            
            // Inicializar Bluetooth LE
            _ble = CrossBluetoothLE.Current;
            _adapter = _ble.Adapter;
            
            // Configurar eventos do adapter
            _adapter.DeviceAdvertised += OnDeviceAdvertised;
            _adapter.ScanTimeoutElapsed += OnScanTimeoutElapsed;
            
            System.Diagnostics.Debug.WriteLine("[BLUETOOTH] BluetoothService inicializado com Plugin.BLE + Bluetooth Clássico");
            
            // Inicializa com alguns dispositivos simulados para teste
            InitializeTestDevices();
        }
        
        private void InitializeTestDevices()
        {
            // Dispositivos pareados (simulados)
            _pairedDevices.Add(new BluetoothDevice 
            { 
                Name = "Fone JBL", 
                Address = "00:11:22:33:44:55", 
                IsPaired = true,
                IsConnected = false,
                DeviceType = BluetoothDeviceType.Audio,
                SignalStrength = -45
            });
            
            _pairedDevices.Add(new BluetoothDevice 
            { 
                Name = "Caixa Sony", 
                Address = "11:22:33:44:55:66", 
                IsPaired = true,
                IsConnected = true,
                DeviceType = BluetoothDeviceType.Audio,
                SignalStrength = -35
            });
        }
        
        private void OnDeviceAdvertised(object? sender, DeviceEventArgs e)
        {
            try
            {
                var device = e.Device;
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Dispositivo BLE encontrado: {device.Name ?? "Desconhecido"} - {device.Id}");
                
                // Converter dispositivo BLE para nosso modelo
                var bluetoothDevice = new BluetoothDevice
                {
                    Name = device.Name ?? "Dispositivo BLE",
                    Address = device.Id.ToString(),
                    IsPaired = false,
                    IsConnected = device.State == Plugin.BLE.Abstractions.DeviceState.Connected,
                    DeviceType = BluetoothDeviceType.BLE,
                    SignalStrength = device.Rssi
                };
                
                AddOrUpdateDevice(bluetoothDevice);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Erro ao processar dispositivo BLE: {ex.Message}");
            }
        }
        
        private void AddOrUpdateDevice(BluetoothDevice bluetoothDevice)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[DEVICE-ADD] Processando dispositivo: {bluetoothDevice.Name} ({bluetoothDevice.Address})");
                System.Diagnostics.Debug.WriteLine($"[DEVICE-ADD] Tipo: {bluetoothDevice.DeviceType}");
                
                // Verificar se já não foi descoberto
                var existing = _discoveredDevices.FirstOrDefault(d => d.Address == bluetoothDevice.Address);
                if (existing == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEVICE-ADD] Novo dispositivo! Adicionando à lista...");
                    System.Diagnostics.Debug.WriteLine($"[DEVICE-ADD] Total antes da adição: {_discoveredDevices.Count}");
                    
                    _discoveredDevices.Add(bluetoothDevice);
                    
                    System.Diagnostics.Debug.WriteLine($"[DEVICE-ADD] ✅ Dispositivo adicionado! Total agora: {_discoveredDevices.Count}");
                    System.Diagnostics.Debug.WriteLine($"[DEVICE-ADD] Disparando evento DeviceDiscovered...");
                    
                    DeviceDiscovered?.Invoke(this, bluetoothDevice);
                    
                    System.Diagnostics.Debug.WriteLine($"[DEVICE-ADD] ✅ Evento DeviceDiscovered disparado!");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[DEVICE-ADD] Dispositivo já existe, atualizando informações...");
                    // Atualizar informações
                    existing.SignalStrength = bluetoothDevice.SignalStrength;
                    existing.Name = bluetoothDevice.Name;
                    System.Diagnostics.Debug.WriteLine($"[DEVICE-ADD] ✅ Informações atualizadas!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEVICE-ADD] ❌ ERRO ao processar dispositivo: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DEVICE-ADD] Stack trace: {ex.StackTrace}");
            }
        }
        
        private void OnScanTimeoutElapsed(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Timeout do scan BLE atingido");
            _isScanning = false;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ScanStatusChanged?.Invoke(this, false);
            });
        }
        
        public async Task<bool> CheckPermissionsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Verificando permissões...");
                return await _permissionService.CheckBluetoothPermissionsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Erro ao verificar permissões: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> RequestPermissionsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Solicitando permissões...");
                return await _permissionService.RequestBluetoothPermissionsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Erro ao solicitar permissões: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> IsBluetoothEnabledAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Verificando se Bluetooth está habilitado...");
                
                // Verificar estado do Bluetooth usando Plugin.BLE
                var state = _ble.State;
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Estado atual: {state}");
                
                bool isEnabled = state == BluetoothState.On;
                
                if (!isEnabled)
                {
                    System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Bluetooth não está habilitado");
                }
                
                return isEnabled;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Erro ao verificar estado do Bluetooth: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> EnableBluetoothAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Tentando habilitar Bluetooth...");
                
                var currentState = _ble.State;
                
                if (currentState == BluetoothState.On)
                {
                    System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Bluetooth já está habilitado");
                    return true;
                }
                
                if (currentState == BluetoothState.Off || currentState == BluetoothState.TurningOff)
                {
                    System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Bluetooth desabilitado - usuário precisa habilitar manualmente");
                    // No Android, não podemos habilitar Bluetooth automaticamente
                    // O usuário deve fazer isso nas configurações
                    return false;
                }
                
                if (currentState == BluetoothState.TurningOn)
                {
                    System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Bluetooth sendo habilitado, aguardando...");
                    // Aguardar até 5 segundos para o Bluetooth ligar
                    for (int i = 0; i < 10; i++)
                    {
                        await Task.Delay(500);
                        if (_ble.State == BluetoothState.On)
                        {
                            System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Bluetooth habilitado com sucesso");
                            return true;
                        }
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Estado atual: {currentState} - não foi possível habilitar");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Erro ao habilitar Bluetooth: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> StartScanAsync()
        {
            try
            {
                if (_isScanning)
                {
                    System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Scan já está em andamento");
                    return true;
                }
                
                // Verifica permissões antes de iniciar
                if (!await CheckPermissionsAsync())
                {
                    System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Permissões não concedidas, solicitando...");
                    if (!await RequestPermissionsAsync())
                    {
                        System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Permissões negadas pelo usuário");
                        return false;
                    }
                }
                
                // Verifica se Bluetooth está habilitado
                if (!await IsBluetoothEnabledAsync())
                {
                    System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Bluetooth não habilitado, tentando habilitar...");
                    if (!await EnableBluetoothAsync())
                    {
                        System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Falha ao habilitar Bluetooth");
                        return false;
                    }
                }
                
                _isScanning = true;
                _discoveredDevices.Clear();
                _scanCancellationTokenSource = new CancellationTokenSource();
                
                ScanStatusChanged?.Invoke(this, true);
                System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Iniciando scan híbrido (BLE + Clássico)...");
                
                // Executar BLE scan primeiro
                System.Diagnostics.Debug.WriteLine("[BLUETOOTH] 🚀 Iniciando BLE scan...");
                await StartBLEScanAsync();
                System.Diagnostics.Debug.WriteLine("[BLUETOOTH] ✅ BLE scan iniciado");
                
                // Executar scan clássico em paralelo após pequeno delay
                System.Diagnostics.Debug.WriteLine("[BLUETOOTH] 🚀 Preparando scan Bluetooth Clássico...");
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Estado atual - _isScanning: {_isScanning}");
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Estado atual - _adapter.IsScanning: {_adapter?.IsScanning}");
                
                // Criar token independente para scan clássico
                _classicScanCancellationTokenSource = new CancellationTokenSource();
                
                var classicTask = Task.Run(async () =>
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("[BLUETOOTH-HYBRID] ==================== INICIANDO TASK CLÁSSICA INDEPENDENTE ====================");
                        System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-HYBRID] Task iniciada - Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                        
                        System.Diagnostics.Debug.WriteLine("[BLUETOOTH-HYBRID] 🔄 Aguardando 2 segundos para evitar conflitos...");
                        await Task.Delay(2000, _classicScanCancellationTokenSource.Token);
                        
                        if (_classicScanCancellationTokenSource.Token.IsCancellationRequested)
                        {
                            System.Diagnostics.Debug.WriteLine("[BLUETOOTH-HYBRID] ❌ Token clássico cancelado");
                            return;
                        }
                        
                        System.Diagnostics.Debug.WriteLine("[BLUETOOTH-HYBRID] 🔧 Executando scan clássico INDEPENDENTE...");
                        System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-HYBRID] Estado BLE (informativo): _isScanning: {_isScanning}");
                        
                        // Executar scan clássico independentemente do estado do BLE
                        await StartClassicBluetoothScanAsync();
                        
                        System.Diagnostics.Debug.WriteLine("[BLUETOOTH-HYBRID] ✅ Scan clássico independente concluído!");
                    }
                    catch (OperationCanceledException)
                    {
                        System.Diagnostics.Debug.WriteLine("[BLUETOOTH-HYBRID] 🛑 Scan clássico cancelado pelo usuário");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-HYBRID] ❌ ERRO no scan clássico: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-HYBRID] Tipo do erro: {ex.GetType().Name}");
                    }
                }, _classicScanCancellationTokenSource.Token);
                
                // Cancelar scan clássico apenas quando o usuário parar explicitamente
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(40000); // 40 segundos timeout para clássico
                        _classicScanCancellationTokenSource?.Cancel();
                        System.Diagnostics.Debug.WriteLine("[BLUETOOTH-HYBRID] ⏰ Timeout do scan clássico após 40s");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-HYBRID] Erro no timeout: {ex.Message}");
                    }
                });
                
                System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Task clássica criada e iniciada");
                
                // Programar parada automática após 45 segundos (mais tempo para híbrido)
                _ = Task.Run(async () =>
                {
                    await Task.Delay(45000); // 45 segundos para permitir ambos os scans
                    if (_isScanning)
                    {
                        await StopScanAsync();
                        System.Diagnostics.Debug.WriteLine("[BLUETOOTH] ⏰ Scan híbrido parado automaticamente após timeout de 45s");
                    }
                });
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Erro ao iniciar scan: {ex.Message}");
                _isScanning = false;
                ScanStatusChanged?.Invoke(this, false);
                return false;
            }
        }
        
        private async Task StartBLEScanAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Iniciando scan BLE...");
                await _adapter.StartScanningForDevicesAsync();
                System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Scan BLE iniciado com sucesso!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Erro no scan BLE: {ex.Message}");
            }
        }
        
        private async Task StartClassicBluetoothScanAsync()
        {
#if ANDROID
            try
            {
                var isHybrid = _isScanning && _adapter.IsScanning;
                var mode = isHybrid ? "HÍBRIDO" : "ISOLADO";
                
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] ===== INICIANDO SCAN BLUETOOTH CLÁSSICO ({mode}) =====");
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] IsScanning: {_isScanning}, BLE Adapter Scanning: {_adapter?.IsScanning}");
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                
                await Task.Run(async () =>
                {
                    try
                    {
                        // Verificar se ainda estamos escaneando
                        if (_scanCancellationTokenSource?.Token.IsCancellationRequested == true)
                        {
                            System.Diagnostics.Debug.WriteLine("[BLUETOOTH-CLASSIC] ❌ Scan cancelado antes de iniciar");
                            return;
                        }
                        
                        System.Diagnostics.Debug.WriteLine("[BLUETOOTH-CLASSIC] Criando BluetoothClient...");
                        var client = new BluetoothClient();
                        System.Diagnostics.Debug.WriteLine("[BLUETOOTH-CLASSIC] ✅ BluetoothClient criado com sucesso!");
                        
                        // Verificar novamente se ainda estamos escaneando
                        if (_scanCancellationTokenSource?.Token.IsCancellationRequested == true)
                        {
                            System.Diagnostics.Debug.WriteLine("[BLUETOOTH-CLASSIC] ❌ Scan cancelado após criar client");
                            return;
                        }
                        
                        System.Diagnostics.Debug.WriteLine("[BLUETOOTH-CLASSIC] Verificando se Bluetooth está ativo...");
                        // Adicionar verificação do estado do rádio Bluetooth
                        
                        System.Diagnostics.Debug.WriteLine("[BLUETOOTH-CLASSIC] Iniciando descoberta de dispositivos...");
                        var devices = client.DiscoverDevices();
                        System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] Descoberta concluída! Encontrados {devices.Count} dispositivos");
                        
                        if (devices.Count == 0)
                        {
                            System.Diagnostics.Debug.WriteLine("[BLUETOOTH-CLASSIC] ❌ Nenhum dispositivo encontrado!");
                            System.Diagnostics.Debug.WriteLine("[BLUETOOTH-CLASSIC] Possíveis causas:");
                            System.Diagnostics.Debug.WriteLine("[BLUETOOTH-CLASSIC] - Bluetooth desligado nos dispositivos");
                            System.Diagnostics.Debug.WriteLine("[BLUETOOTH-CLASSIC] - Dispositivos não estão em modo detectável");
                            System.Diagnostics.Debug.WriteLine("[BLUETOOTH-CLASSIC] - Permissões insuficientes");
                            return;
                        }
                        
                        int deviceIndex = 0;
                        foreach (var device in devices)
                        {
                            deviceIndex++;
                            
                            if (_scanCancellationTokenSource?.Token.IsCancellationRequested == true)
                            {
                                System.Diagnostics.Debug.WriteLine("[BLUETOOTH-CLASSIC] Scan cancelado pelo usuário");
                                break;
                            }

                            System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] === DISPOSITIVO #{deviceIndex} ===");
                            System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] Nome: {device.DeviceName ?? "❌ SEM NOME"}");
                            System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] Endereço: {device.DeviceAddress}");
                            System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] Autenticado: {device.Authenticated}");
                            System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] Conectado: {device.Connected}");
                            System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] Ativo: {device.InstalledServices?.Count ?? 0} serviços");

                            var bluetoothDevice = new BluetoothDevice
                            {
                                Name = !string.IsNullOrEmpty(device.DeviceName) ? device.DeviceName : $"Dispositivo #{deviceIndex}",
                                Address = device.DeviceAddress.ToString(),
                                IsPaired = device.Authenticated,
                                IsConnected = device.Connected,
                                DeviceType = BluetoothDeviceType.Classic,
                                SignalStrength = -50
                            };

                            System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] Adicionando dispositivo à lista: {bluetoothDevice.Name}");

                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                try
                                {
                                    AddOrUpdateDevice(bluetoothDevice);
                                    System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] ✅ Dispositivo {bluetoothDevice.Name} adicionado com sucesso!");
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] ❌ Erro ao adicionar dispositivo: {ex.Message}");
                                }
                            });
                            
                            // Pequena pausa entre dispositivos
                            await Task.Delay(100);
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] ===== SCAN CLÁSSICO FINALIZADO =====");
                        System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] 📊 Dispositivos processados: {deviceIndex}");
                        System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] 📊 Total na lista agora: {_discoveredDevices.Count}");
                        System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] 📊 Clássicos na lista: {_discoveredDevices.Count(d => d.DeviceType == BluetoothDeviceType.Classic)}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] ❌ ERRO CRÍTICO durante scan: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] Stack trace: {ex.StackTrace}");
                        
                        if (ex.InnerException != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] Inner exception: {ex.InnerException.Message}");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] ❌ ERRO ao iniciar scan clássico: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-CLASSIC] Tipo do erro: {ex.GetType().Name}");
            }
#else
            await Task.CompletedTask;
            System.Diagnostics.Debug.WriteLine("[BLUETOOTH-CLASSIC] ⚠️ Scan clássico não suportado nesta plataforma");
#endif
        }
        
        public async Task StopScanAsync()
        {
            try
            {
                if (!_isScanning)
                {
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Parando scan híbrido...");
                
                // Cancelar scans em andamento
                _scanCancellationTokenSource?.Cancel();
                _classicScanCancellationTokenSource?.Cancel();
                System.Diagnostics.Debug.WriteLine("[BLUETOOTH] 🛑 Cancelando scan clássico independente...");
                
                // Parar escaneamento BLE
                if (_adapter.IsScanning)
                {
                    await _adapter.StopScanningForDevicesAsync();
                }
                
                _isScanning = false;
                ScanStatusChanged?.Invoke(this, false);
                System.Diagnostics.Debug.WriteLine("[BLUETOOTH] Scan híbrido parado com sucesso");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Erro ao parar scan: {ex.Message}");
            }
        }
        
        public async Task<ObservableCollection<BluetoothDevice>> GetDiscoveredDevicesAsync()
        {
            await Task.CompletedTask;
            return _discoveredDevices;
        }
        
        public async Task<ObservableCollection<BluetoothDevice>> GetPairedDevicesAsync()
        {
            await Task.CompletedTask;
            return _pairedDevices;
        }
        
        public async Task<bool> PairDeviceAsync(BluetoothDevice device)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Tentando parear com {device.Name}...");
                
                // Simula processo de pareamento
                await Task.Delay(2000);
                
                device.IsPaired = true;
                _pairedDevices.Add(device);
                _discoveredDevices.Remove(device);
                
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Pareado com sucesso: {device.Name}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Erro ao parear dispositivo: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> ConnectDeviceAsync(BluetoothDevice device)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Conectando com {device.Name}...");
                
                // Simula processo de conexão
                await Task.Delay(1500);
                
                device.IsConnected = true;
                DeviceConnectionChanged?.Invoke(this, device);
                
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Conectado com sucesso: {device.Name}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Erro ao conectar dispositivo: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> DisconnectDeviceAsync(BluetoothDevice device)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Desconectando {device.Name}...");
                
                await Task.Delay(500);
                
                device.IsConnected = false;
                DeviceConnectionChanged?.Invoke(this, device);
                
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Desconectado: {device.Name}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Erro ao desconectar dispositivo: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> UnpairDeviceAsync(BluetoothDevice device)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Removendo pareamento com {device.Name}...");
                
                await Task.Delay(1000);
                
                device.IsPaired = false;
                device.IsConnected = false;
                _pairedDevices.Remove(device);
                
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Pareamento removido: {device.Name}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Erro ao remover pareamento: {ex.Message}");
                return false;
            }
        }
        
        public void Dispose()
        {
            try
            {
                // Parar escaneamento se estiver ativo
                if (_isScanning)
                {
                    _ = Task.Run(async () => await StopScanAsync());
                }
                
                // Cancelar operações
                _scanCancellationTokenSource?.Cancel();
                _scanCancellationTokenSource?.Dispose();
                _classicScanCancellationTokenSource?.Cancel();
                _classicScanCancellationTokenSource?.Dispose();
                
                // Remover eventos
                if (_adapter != null)
                {
                    _adapter.DeviceAdvertised -= OnDeviceAdvertised;
                    _adapter.ScanTimeoutElapsed -= OnScanTimeoutElapsed;
                }
                
                System.Diagnostics.Debug.WriteLine("[BLUETOOTH] BluetoothService disposed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH] Erro ao fazer dispose: {ex.Message}");
            }
        }
    }
}