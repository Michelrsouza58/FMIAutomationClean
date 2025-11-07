using Microsoft.Maui.Controls;

namespace FMIAutomation.Views
{
    [QueryProperty(nameof(DeviceName), "deviceName")]
    [QueryProperty(nameof(IsOnline), "isOnline")]
    public partial class DeviceControlPage : ContentPage
    {
        private string _deviceName = string.Empty;
        private bool _isOnline = false;
        
        public string DeviceName
        {
            get => _deviceName;
            set
            {
                _deviceName = value;
                OnPropertyChanged();
                UpdateDeviceInfo();
            }
        }
        
        public bool IsOnline
        {
            get => _isOnline;
            set
            {
                _isOnline = value;
                OnPropertyChanged();
                UpdateConnectionStatus();
            }
        }
        
        public DeviceControlPage()
        {
            InitializeComponent();
            SetupSliderEvents();
            SetupButtonEvents();
        }
        
        private void UpdateDeviceInfo()
        {
            if (DeviceNameLabel != null)
            {
                DeviceNameLabel.Text = DeviceName;
            }
        }
        
        private void UpdateConnectionStatus()
        {
            if (StatusDot != null && StatusLabel != null && PowerToggleButton != null)
            {
                if (IsOnline)
                {
                    StatusDot.Fill = Color.FromArgb("#10B981");
                    StatusLabel.Text = "Online";
                    StatusLabel.TextColor = Color.FromArgb("#10B981");
                    
                    // Botão para desconectar
                    PowerToggleButton.Text = "🔌 Desconectar Dispositivo";
                    PowerToggleButton.BackgroundColor = Color.FromArgb("#EF4444");
                }
                else
                {
                    StatusDot.Fill = Colors.Gray;
                    StatusLabel.Text = "Offline";
                    StatusLabel.TextColor = Colors.Gray;
                    
                    // Botão para conectar
                    PowerToggleButton.Text = "⚡ Conectar Dispositivo";
                    PowerToggleButton.BackgroundColor = Color.FromArgb("#10B981");
                }
            }
            
            // Habilitar/desabilitar controles baseado no status
            UpdateControlsAvailability();
        }
        
        private void UpdateControlsAvailability()
        {
            var isEnabled = IsOnline;
            
            if (HumiditySlider != null) HumiditySlider.IsEnabled = isEnabled;
            if (FanSpeedSlider != null) FanSpeedSlider.IsEnabled = isEnabled;
            if (TemperatureSlider != null) TemperatureSlider.IsEnabled = isEnabled;
            if (LightIntensitySlider != null) LightIntensitySlider.IsEnabled = isEnabled;
            if (FeedingSlider != null) FeedingSlider.IsEnabled = isEnabled;
            if (ApplyConfigButton != null) ApplyConfigButton.IsEnabled = isEnabled;
        }
        
        private void SetupSliderEvents()
        {
            // Evento do slider de umidade
            HumiditySlider.ValueChanged += (s, e) =>
            {
                HumidityValueLabel.Text = $"{e.NewValue:F0}%";
            };
            
            // Evento do slider de velocidade do ventilador
            FanSpeedSlider.ValueChanged += (s, e) =>
            {
                var speed = (int)e.NewValue;
                FanSpeedValueLabel.Text = speed == 0 ? "Desligado" : speed.ToString();
            };
            
            // Evento do slider de temperatura
            TemperatureSlider.ValueChanged += (s, e) =>
            {
                TemperatureValueLabel.Text = $"{e.NewValue:F0}°C";
            };
            
            // Evento do slider de iluminação
            LightIntensitySlider.ValueChanged += (s, e) =>
            {
                LightIntensityValueLabel.Text = $"{e.NewValue:F0}%";
            };
            
            // Evento do slider de alimentação
            FeedingSlider.ValueChanged += (s, e) =>
            {
                var times = (int)e.NewValue;
                FeedingValueLabel.Text = $"{times}x/dia";
            };
        }
        
        private void SetupButtonEvents()
        {
            PowerToggleButton.Clicked += async (s, e) => await OnPowerToggleClicked();
            ApplyConfigButton.Clicked += async (s, e) => await OnApplyConfigClicked();
        }
        
        private async Task OnPowerToggleClicked()
        {
            if (IsOnline)
            {
                await DisconnectDevice();
            }
            else
            {
                await ConnectDevice();
            }
        }

        private async Task ConnectDevice()
        {
            // Simular processo de conexão
            PowerToggleButton.IsEnabled = false;
            PowerToggleButton.Text = "🔄 Conectando...";
            
            try
            {
                // Simular delay de conexão
                await Task.Delay(2000);
                
                // Simular sucesso (90% de chance)
                var random = new Random();
                var success = random.Next(1, 11) <= 9;
                
                if (success)
                {
                    IsOnline = true;
                    await DisplayAlert("✅ Sucesso", $"Conectado ao {DeviceName} com sucesso!", "OK");
                }
                else
                {
                    await DisplayAlert("❌ Erro", "Falha ao conectar ao dispositivo. Tente novamente.", "OK");
                }
            }
            finally
            {
                PowerToggleButton.IsEnabled = true;
            }
        }
        
        private async Task DisconnectDevice()
        {
            var result = await DisplayAlert("⚠️ Confirmação", 
                $"Deseja realmente desconectar do {DeviceName}?", 
                "Sim", "Cancelar");
                
            if (result)
            {
                PowerToggleButton.IsEnabled = false;
                PowerToggleButton.Text = "🔄 Desconectando...";
                
                try
                {
                    // Simular delay de desconexão
                    await Task.Delay(1000);
                    
                    IsOnline = false;
                    await DisplayAlert("ℹ️ Desconectado", $"Desconectado do {DeviceName}.", "OK");
                }
                finally
                {
                    PowerToggleButton.IsEnabled = true;
                }
            }
        }
        
        private async Task OnApplyConfigClicked()
        {
            if (!IsOnline)
            {
                await DisplayAlert("⚠️ Atenção", "Dispositivo não está conectado!", "OK");
                return;
            }
            
            ApplyConfigButton.IsEnabled = false;
            ApplyConfigButton.Text = "Aplicando...";
            
            try
            {
                // Simular envio das configurações
                await Task.Delay(2000);
                
                // Coletar valores atuais dos sliders
                var config = new
                {
                    Humidity = HumiditySlider.Value,
                    FanSpeed = (int)FanSpeedSlider.Value,
                    Temperature = TemperatureSlider.Value,
                    LightIntensity = LightIntensitySlider.Value,
                    FeedingTimes = (int)FeedingSlider.Value
                };
                
                // Simular sucesso (95% de chance)
                var random = new Random();
                var success = random.Next(1, 21) <= 19;
                
                if (success)
                {
                    await DisplayAlert("✅ Configurações Aplicadas", 
                        $"Configurações enviadas para {DeviceName}:\n\n" +
                        $"💧 Umidade: {config.Humidity:F0}%\n" +
                        $"🌪️ Ventilador: {(config.FanSpeed == 0 ? "Desligado" : config.FanSpeed.ToString())}\n" +
                        $"🌡️ Temperatura: {config.Temperature:F0}°C\n" +
                        $"💡 Iluminação: {config.LightIntensity:F0}%\n" +
                        $"🍽️ Alimentação: {config.FeedingTimes}x/dia", 
                        "OK");
                }
                else
                {
                    await DisplayAlert("❌ Erro", "Falha ao aplicar configurações. Tente novamente.", "OK");
                }
            }
            finally
            {
                ApplyConfigButton.Text = "💾 Aplicar Configurações";
                ApplyConfigButton.IsEnabled = true;
            }
        }
    }
}