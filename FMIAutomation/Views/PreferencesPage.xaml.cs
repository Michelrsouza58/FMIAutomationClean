using Microsoft.Maui.Controls;

namespace FMIAutomation.Views
{
    public partial class PreferencesPage : ContentPage
    {
        public PreferencesPage()
        {
            InitializeComponent();
            
            // Configura eventos
            SavePreferencesButton.Clicked += OnSavePreferencesClicked;
            
            // Carrega preferências salvas
            LoadPreferences();
        }

        private void LoadPreferences()
        {
            try
            {
                // Carrega preferências do armazenamento local
                DarkModeSwitch.IsToggled = Preferences.Get("DarkMode", false);
                PushNotificationsSwitch.IsToggled = Preferences.Get("PushNotifications", true);
                EmailNotificationsSwitch.IsToggled = Preferences.Get("EmailNotifications", false);
                AnalyticsSwitch.IsToggled = Preferences.Get("Analytics", false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PreferencesPage] Erro ao carregar preferências: {ex.Message}");
            }
        }

        private async void OnSavePreferencesClicked(object sender, EventArgs e)
        {
            try
            {
                SavePreferencesButton.IsEnabled = false;
                SavePreferencesButton.Text = "💾  Salvando...";

                // Salva preferências no armazenamento local
                Preferences.Set("DarkMode", DarkModeSwitch.IsToggled);
                Preferences.Set("PushNotifications", PushNotificationsSwitch.IsToggled);
                Preferences.Set("EmailNotifications", EmailNotificationsSwitch.IsToggled);
                Preferences.Set("Analytics", AnalyticsSwitch.IsToggled);

                // Simula processamento
                await Task.Delay(1000);
                
                await DisplayAlert("Sucesso", "Preferências salvas com sucesso!", "OK");
                
                // Aplica tema se necessário
                if (DarkModeSwitch.IsToggled)
                {
                    await DisplayAlert("Tema", "O tema escuro será aplicado na próxima abertura do app.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao salvar preferências: {ex.Message}", "OK");
            }
            finally
            {
                SavePreferencesButton.IsEnabled = true;
                SavePreferencesButton.Text = "💾  Salvar Preferências";
            }
        }


    }
}