using Microsoft.Maui.Controls;
using FMIAutomation.Services;

namespace FMIAutomation.Views
{
    public partial class PreferencesPage : ContentPage
    {
        public PreferencesPage()
        {
            InitializeComponent();
            
            // Configura eventos
            SavePreferencesButton.Clicked += OnSavePreferencesClicked;
            DarkModeSwitch.Toggled += OnDarkModeToggled;
            
            // Carrega preferências salvas
            LoadPreferences();
        }

        private async void LoadPreferences()
        {
            try
            {
                // Carrega tema atual
                var currentTheme = await ThemeService.GetCurrentThemeAsync();
                DarkModeSwitch.IsToggled = currentTheme == ThemeService.AppTheme.Dark;
                
                // Carrega outras preferências
                PushNotificationsSwitch.IsToggled = Preferences.Get("PushNotifications", true);
                EmailNotificationsSwitch.IsToggled = Preferences.Get("EmailNotifications", false);
                AnalyticsSwitch.IsToggled = Preferences.Get("Analytics", false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PreferencesPage] Erro ao carregar preferências: {ex.Message}");
            }
        }

        private async void OnDarkModeToggled(object? sender, ToggledEventArgs e)
        {
            try
            {
                // Aplica o tema imediatamente
                var theme = e.Value ? ThemeService.AppTheme.Dark : ThemeService.AppTheme.Light;
                await ThemeService.SetThemeAsync(theme);
                
                // Feedback visual
                var message = e.Value ? "Tema escuro ativado! 🌙" : "Tema claro ativado! ☀️";
                await DisplayAlert("Tema Alterado", message, "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PreferencesPage] Erro ao alterar tema: {ex.Message}");
            }
        }

        private async void OnSavePreferencesClicked(object sender, EventArgs e)
        {
            try
            {
                SavePreferencesButton.IsEnabled = false;
                SavePreferencesButton.Text = "💾  Salvando...";

                // Salva outras preferências (tema já é salvo automaticamente)
                Preferences.Set("PushNotifications", PushNotificationsSwitch.IsToggled);
                Preferences.Set("EmailNotifications", EmailNotificationsSwitch.IsToggled);
                Preferences.Set("Analytics", AnalyticsSwitch.IsToggled);

                // Simula processamento
                await Task.Delay(800);
                
                await DisplayAlert("✅ Sucesso", "Todas as preferências foram salvas com sucesso!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("❌ Erro", $"Erro ao salvar preferências: {ex.Message}", "OK");
            }
            finally
            {
                SavePreferencesButton.IsEnabled = true;
                SavePreferencesButton.Text = "💾  Salvar Preferências";
            }
        }


    }
}