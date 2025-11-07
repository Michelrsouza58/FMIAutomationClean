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
                        
                        // Forçar atualização de todas as páginas abertas
                        RefreshAllPages();
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
                if (Application.Current == null) return;
                
                // Usar o sistema nativo do MAUI para alternar temas
                var mauiTheme = theme == AppTheme.Light ? 
                    Microsoft.Maui.ApplicationModel.AppTheme.Light : 
                    Microsoft.Maui.ApplicationModel.AppTheme.Dark;

                Application.Current.UserAppTheme = mauiTheme;
                
                System.Diagnostics.Debug.WriteLine($"[ThemeService] Tema aplicado: {theme}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao aplicar tema: {ex.Message}");
            }
        }

        private static void RefreshAllPages()
        {
            try
            {
                if (Application.Current?.MainPage == null) return;

                // Forçar refresh da página principal
                var mainPage = Application.Current.MainPage;
                
                if (mainPage is Shell shell)
                {
                    // Atualizar todas as páginas do Shell
                    RefreshShellPages(shell);
                }
                else if (mainPage is NavigationPage navPage)
                {
                    // Atualizar páginas de navegação
                    foreach (var page in navPage.Navigation.NavigationStack)
                    {
                        RefreshPageElements(page);
                    }
                }
                else
                {
                    RefreshPageElements(mainPage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar páginas: {ex.Message}");
            }
        }

        private static void RefreshShellPages(Shell shell)
        {
            try
            {
                if (shell.CurrentPage != null)
                {
                    RefreshPageElements(shell.CurrentPage);
                }
                
                // Forçar re-render do Shell
                shell.BatchBegin();
                shell.BatchCommit();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar Shell: {ex.Message}");
            }
        }

        private static void RefreshPageElements(Page page)
        {
            try
            {
                if (page == null) return;

                // Forçar re-render da página
                page.BatchBegin();
                page.BatchCommit();

                // Se for ContentPage, atualizar o conteúdo
                if (page is ContentPage contentPage && contentPage.Content != null)
                {
                    RefreshViewElements(contentPage.Content);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar página {page?.GetType().Name}: {ex.Message}");
            }
        }

        private static void RefreshViewElements(View view)
        {
            try
            {
                if (view == null) return;

                // Forçar re-render do elemento
                view.BatchBegin();
                view.BatchCommit();

                // Se for um layout, atualizar filhos recursivamente
                if (view is Layout layout)
                {
                    foreach (var child in layout.Children.OfType<View>())
                    {
                        RefreshViewElements(child);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar elemento {view?.GetType().Name}: {ex.Message}");
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