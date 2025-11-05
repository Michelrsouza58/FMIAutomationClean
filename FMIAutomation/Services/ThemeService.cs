using Microsoft.Maui.Controls;
using System.Globalization;

namespace FMIAutomation.Services
{
    public class ThemeService
    {
        public enum AppTheme
        {
            Light,
            Dark
        }

        private const string THEME_KEY = "app_theme";
        private const string LANGUAGE_KEY = "app_language";

        // Evento para notificar mudanças de tema
        public static event EventHandler<AppTheme>? ThemeChanged;

        public static async Task<AppTheme> GetCurrentThemeAsync()
        {
            var theme = await SecureStorage.GetAsync(THEME_KEY);
            return theme == "Light" ? AppTheme.Light : AppTheme.Dark;
        }

        public static async Task SetThemeAsync(AppTheme theme)
        {
            try
            {
                await SecureStorage.SetAsync(THEME_KEY, theme.ToString());
                
                // Aplicar tema na thread principal
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        ApplyTheme(theme);
                        // Notificar após aplicar o tema
                        ThemeChanged?.Invoke(null, theme);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erro ao aplicar tema na UI: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao definir tema: {ex.Message}");
            }
        }

        public static void ApplyTheme(AppTheme theme)
        {
            try
            {
                if (Application.Current?.Resources == null) return;

                var resources = Application.Current.Resources;

                // Definir cores seguramente
                var colors = theme == AppTheme.Light ? 
                    new Dictionary<string, Color>
                    {
                        ["BackgroundColor"] = Color.FromArgb("#F8FAFC"),
                        ["CardColor"] = Color.FromArgb("#FFFFFF"),
                        ["TextColor"] = Color.FromArgb("#111827"),
                        ["SecondaryTextColor"] = Color.FromArgb("#4B5563"),
                        ["AccentColor"] = Color.FromArgb("#1D4ED8"),
                        ["DangerColor"] = Color.FromArgb("#DC2626"),
                        ["BorderColor"] = Color.FromArgb("#D1D5DB"),
                        ["LoginBackgroundColor"] = Color.FromArgb("#F3F4F6")
                    } :
                    new Dictionary<string, Color>
                    {
                        ["BackgroundColor"] = Color.FromArgb("#1F2937"),
                        ["CardColor"] = Color.FromArgb("#374151"),
                        ["TextColor"] = Color.FromArgb("#F9FAFB"),
                        ["SecondaryTextColor"] = Color.FromArgb("#D1D5DB"),
                        ["AccentColor"] = Color.FromArgb("#60A5FA"),
                        ["DangerColor"] = Color.FromArgb("#F87171"),
                        ["BorderColor"] = Color.FromArgb("#4B5563"),
                        ["LoginBackgroundColor"] = Color.FromArgb("#1F2937")
                    };

                // Aplicar cores de forma segura
                foreach (var color in colors)
                {
                    try
                    {
                        if (resources.ContainsKey(color.Key))
                        {
                            resources[color.Key] = color.Value;
                        }
                        else
                        {
                            resources.Add(color.Key, color.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erro ao definir cor {color.Key}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao aplicar tema: {ex.Message}");
            }
        }

        private static void UpdatePageColors(Page page)
        {
            if (Application.Current?.Resources == null) return;

            // Atualizar cor de fundo da página
            page.BackgroundColor = (Color)Application.Current.Resources["BackgroundColor"];

            // Atualizar todas as páginas modais e navegação
            if (page.Navigation != null)
            {
                foreach (var modalPage in page.Navigation.ModalStack)
                {
                    UpdatePageColors(modalPage);
                }
                
                if (page.Navigation.NavigationStack != null)
                {
                    foreach (var navPage in page.Navigation.NavigationStack)
                    {
                        UpdatePageColors(navPage);
                    }
                }
            }

            // Forçar atualização visual
            if (page is ContentPage contentPage && contentPage.Content != null)
            {
                RefreshElementColors(contentPage.Content);
            }
        }

        private static void RefreshElementColors(View element)
        {
            if (Application.Current?.Resources == null) return;

            // Atualizar cores baseado no tipo de elemento
            switch (element)
            {
                case Frame frame:
                    if (frame.BackgroundColor == Color.FromArgb("#3B5A7A") || 
                        frame.BackgroundColor == Color.FromArgb("#FFFFFF") ||
                        frame.BackgroundColor == Color.FromArgb("#374151"))
                    {
                        frame.BackgroundColor = (Color)Application.Current.Resources["CardColor"];
                    }
                    break;
                case Label label:
                    if (label.TextColor == Colors.White || 
                        label.TextColor == Color.FromArgb("#111827") ||
                        label.TextColor == Color.FromArgb("#F9FAFB"))
                    {
                        label.TextColor = (Color)Application.Current.Resources["TextColor"];
                    }
                    break;
            }

            // Recursivamente atualizar filhos
            if (element is Layout layout)
            {
                foreach (var child in layout.Children.OfType<View>())
                {
                    RefreshElementColors(child);
                }
            }
            else if (element is ContentView contentView && contentView.Content != null)
            {
                RefreshElementColors(contentView.Content);
            }
            else if (element is ScrollView scrollView && scrollView.Content != null)
            {
                RefreshElementColors(scrollView.Content);
            }
        }

        public static async Task<string> GetCurrentLanguageAsync()
        {
            var language = await SecureStorage.GetAsync(LANGUAGE_KEY);
            return language ?? "pt";
        }

        public static async Task SetLanguageAsync(string languageCode)
        {
            await SecureStorage.SetAsync(LANGUAGE_KEY, languageCode);
            ApplyLanguage(languageCode);
        }

        public static void ApplyLanguage(string languageCode)
        {
            try
            {
                var culture = new CultureInfo(languageCode);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }
            catch
            {
                // Fallback para português se der erro
                var culture = new CultureInfo("pt-BR");
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
        }

        public static async Task InitializeAsync()
        {
            var theme = await GetCurrentThemeAsync();
            ApplyTheme(theme);
            
            var language = await GetCurrentLanguageAsync();
            ApplyLanguage(language);
        }
    }
}