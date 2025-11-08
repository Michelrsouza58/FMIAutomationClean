using Microsoft.Maui.Controls;
using FMIAutomation.Services;

namespace FMIAutomation.Views
{
    public partial class BluetoothDevicesPage : ContentPage
    {
        private readonly IBluetoothService _bluetoothService;
        private readonly IPermissionService _permissionService;
        
        public BluetoothDevicesPage(IBluetoothService bluetoothService, IPermissionService permissionService)
        {
            InitializeComponent();
            
            _bluetoothService = bluetoothService;
            _permissionService = permissionService;
            
            // Criar ViewModel com os serviços injetados
            var vm = new ViewModels.BluetoothDevicesViewModel(_bluetoothService, _permissionService);
            this.BindingContext = vm;
            
            // Configurar eventos dos botões
            ScanDevicesBtn.Clicked += async (s, e) => await StartBluetoothScan();
            TestBLEBtn.Clicked += async (s, e) => await TestBLEScan();
            TestClassicBtn.Clicked += async (s, e) => await TestClassicScan();
            
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
                System.Diagnostics.Debug.WriteLine("[PAGE] ==================== SCAN HÍBRIDO INICIADO ====================");
                // Primeiro, solicitar permissões Bluetooth
                System.Diagnostics.Debug.WriteLine("[BluetoothScan] Verificando permissões...");
                
                var hasPermissions = await _permissionService.CheckBluetoothPermissionsAsync();
                if (!hasPermissions)
                {
                    System.Diagnostics.Debug.WriteLine("[BluetoothScan] Solicitando permissões ao usuário...");
                    var granted = await _permissionService.RequestBluetoothPermissionsAsync();
                    
                    if (!granted)
                    {
                        await DisplayAlert("Permissões Necessárias", 
                            "Para usar o Bluetooth, é necessário conceder permissão de localização. " +
                            "Você pode fazer isso nas configurações do app.", "OK");
                        return;
                    }
                }

                // Mostrar indicador de scan
                ScanStatusFrame.IsVisible = true;
                ScanIndicator.IsRunning = true;
                ScanStatusLabel.Text = "Procurando dispositivos Bluetooth...";
                EmptyStateSection.IsVisible = false;

                System.Diagnostics.Debug.WriteLine("[BluetoothScan] Iniciando escaneamento real...");
                
                // Limpar dispositivos anteriores
                AvailableDevicesCollection.ItemsSource = null;
                
                // Configurar callback para dispositivos encontrados
                _bluetoothService.DeviceDiscovered += OnDeviceDiscovered;
                _bluetoothService.ScanStatusChanged += OnScanStatusChanged;
                
                // Iniciar scan real
                System.Diagnostics.Debug.WriteLine("[PAGE] Chamando _bluetoothService.StartScanAsync()...");
                var scanStarted = await _bluetoothService.StartScanAsync();
                System.Diagnostics.Debug.WriteLine($"[PAGE] StartScanAsync retornou: {scanStarted}");
                
                if (!scanStarted)
                {
                    // Falha ao iniciar scan
                    ScanIndicator.IsRunning = false;
                    ScanStatusLabel.Text = "Erro ao iniciar escaneamento Bluetooth";
                    await Task.Delay(2000);
                    ScanStatusFrame.IsVisible = false;
                    EmptyStateSection.IsVisible = true;
                    
                    await DisplayAlert("Erro", "Não foi possível iniciar o escaneamento Bluetooth. Verifique se o Bluetooth está habilitado.", "OK");
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
        
        private void OnDeviceDiscovered(object? sender, Models.BluetoothDevice device)
        {
            System.Diagnostics.Debug.WriteLine($"[BluetoothScan] Dispositivo descoberto: {device.Name}");
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    // Obter dispositivos atuais ou criar nova lista
                    var currentDevices = AvailableDevicesCollection.ItemsSource as List<Models.BluetoothDevice> 
                        ?? new List<Models.BluetoothDevice>();
                    
                    // Adicionar novo dispositivo se não existir
                    if (!currentDevices.Any(d => d.Address == device.Address))
                    {
                        var newList = currentDevices.ToList();
                        newList.Add(device);
                        AvailableDevicesCollection.ItemsSource = newList;
                        
                        // Esconder estado vazio se havia
                        EmptyStateSection.IsVisible = false;
                        
                        System.Diagnostics.Debug.WriteLine($"[BluetoothScan] Dispositivo adicionado à UI: {device.Name}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[BluetoothScan] Erro ao adicionar dispositivo à UI: {ex.Message}");
                }
            });
        }
        
        private void OnScanStatusChanged(object? sender, bool isScanning)
        {
            System.Diagnostics.Debug.WriteLine($"[BluetoothScan] Status do scan mudou: {isScanning}");
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (!isScanning)
                    {
                        // Scan terminou
                        ScanIndicator.IsRunning = false;
                        
                        var deviceCount = (AvailableDevicesCollection.ItemsSource as List<Models.BluetoothDevice>)?.Count ?? 0;
                        ScanStatusLabel.Text = deviceCount > 0 
                            ? $"Encontrados {deviceCount} dispositivos" 
                            : "Nenhum dispositivo encontrado";
                        
                        // Esconder indicador após 2 segundos
                        Task.Run(async () =>
                        {
                            await Task.Delay(2000);
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                ScanStatusFrame.IsVisible = false;
                                
                                // Mostrar estado vazio se nenhum dispositivo foi encontrado
                                if (deviceCount == 0)
                                {
                                    EmptyStateSection.IsVisible = true;
                                }
                            });
                        });
                        
                        // Remover callbacks para evitar vazamentos de memória
                        _bluetoothService.DeviceDiscovered -= OnDeviceDiscovered;
                        _bluetoothService.ScanStatusChanged -= OnScanStatusChanged;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[BluetoothScan] Erro ao atualizar status do scan: {ex.Message}");
                }
            });
        }

        private async Task TestBLEScan()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[TEST-BLE] ===== TESTE BLE ISOLADO =====");
                
                ScanStatusFrame.IsVisible = true;
                ScanIndicator.IsRunning = true;
                ScanStatusLabel.Text = "🔷 Testando scan BLE...";
                EmptyStateSection.IsVisible = false;
                
                // Limpar lista atual
                ConnectedDevicesCollection.ItemsSource = null;
                AvailableDevicesCollection.ItemsSource = null;
                
                // Testar apenas BLE
                if (await _bluetoothService.CheckPermissionsAsync())
                {
                    if (await _bluetoothService.IsBluetoothEnabledAsync())
                    {
                        await _bluetoothService.StartScanAsync();
                        
                        await Task.Delay(10000); // 10 segundos
                        await _bluetoothService.StopScanAsync();
                        
                        var devices = await _bluetoothService.GetDiscoveredDevicesAsync();
                        var bleDevices = devices.Where(d => d.DeviceType == FMIAutomation.Models.BluetoothDeviceType.BLE).ToList();
                        
                        System.Diagnostics.Debug.WriteLine($"[TEST-BLE] Encontrados {bleDevices.Count} dispositivos BLE");
                        AvailableDevicesCollection.ItemsSource = bleDevices;
                    }
                }
                
                ScanStatusFrame.IsVisible = false;
                ScanIndicator.IsRunning = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TEST-BLE] Erro: {ex.Message}");
                ScanStatusFrame.IsVisible = false;
                ScanIndicator.IsRunning = false;
            }
        }

        private async Task TestClassicScan()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[TEST-CLASSIC] ===== TESTE BLUETOOTH CLÁSSICO ISOLADO =====");
                
                ScanStatusFrame.IsVisible = true;
                ScanIndicator.IsRunning = true;
                ScanStatusLabel.Text = "🔶 Testando scan Clássico...";
                EmptyStateSection.IsVisible = false;
                
                // TESTE: Simular o mesmo que acontece no híbrido
                System.Diagnostics.Debug.WriteLine("[TEST-CLASSIC] 🧪 Testando Task como no híbrido...");
                var testTask = Task.Run(async () =>
                {
                    System.Diagnostics.Debug.WriteLine("[TEST-CLASSIC] Task iniciada!");
                    await Task.Delay(1000);
                    System.Diagnostics.Debug.WriteLine("[TEST-CLASSIC] Após delay na Task!");
                    
                    // Chamar o scan clássico
                    await _bluetoothService.StartScanAsync();
                });
                
                System.Diagnostics.Debug.WriteLine("[TEST-CLASSIC] Task criada, aguardando...");
                
                // Limpar lista atual
                ConnectedDevicesCollection.ItemsSource = null;
                AvailableDevicesCollection.ItemsSource = null;
                
                // Forçar apenas scan clássico
                await TestClassicScanOnly();
                
                var devices = await _bluetoothService.GetDiscoveredDevicesAsync();
                var classicDevices = devices.Where(d => d.DeviceType == FMIAutomation.Models.BluetoothDeviceType.Classic).ToList();
                
                System.Diagnostics.Debug.WriteLine($"[TEST-CLASSIC] Encontrados {classicDevices.Count} dispositivos clássicos");
                AvailableDevicesCollection.ItemsSource = classicDevices;
                
                ScanStatusFrame.IsVisible = false;
                ScanIndicator.IsRunning = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TEST-CLASSIC] Erro: {ex.Message}");
                ScanStatusFrame.IsVisible = false;
                ScanIndicator.IsRunning = false;
            }
        }

        private async Task TestClassicScanOnly()
        {
            // Simulação de scan clássico direto
#if ANDROID
            try
            {
                System.Diagnostics.Debug.WriteLine("[TEST-CLASSIC-DIRECT] Iniciando teste direto...");
                
                await Task.Run(() =>
                {
                    try
                    {
                        var client = new InTheHand.Net.Sockets.BluetoothClient();
                        System.Diagnostics.Debug.WriteLine("[TEST-CLASSIC-DIRECT] Cliente criado!");
                        
                        var devices = client.DiscoverDevices();
                        System.Diagnostics.Debug.WriteLine($"[TEST-CLASSIC-DIRECT] {devices.Count} dispositivos encontrados!");
                        
                        foreach (var device in devices)
                        {
                            System.Diagnostics.Debug.WriteLine($"[TEST-CLASSIC-DIRECT] - {device.DeviceName ?? "Sem nome"} ({device.DeviceAddress})");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[TEST-CLASSIC-DIRECT] Erro: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TEST-CLASSIC-DIRECT] Erro externo: {ex.Message}");
            }
#else
            await Task.CompletedTask;
            System.Diagnostics.Debug.WriteLine("[TEST-CLASSIC-DIRECT] Não suportado nesta plataforma");
#endif
        }
    }
}
