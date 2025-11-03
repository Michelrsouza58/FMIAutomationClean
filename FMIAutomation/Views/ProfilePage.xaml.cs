using Microsoft.Maui.Controls;

namespace FMIAutomation.Views
{
    public partial class ProfilePage : ContentPage
    {
    private readonly Services.IAuthService? _authService;
        private string _userEmail = "";
        public ProfilePage()
        {
            InitializeComponent();
        // Recupera serviço de autenticação
        _authService = Application.Current?.Handler?.MauiContext?.Services.GetService(typeof(Services.IAuthService)) as Services.IAuthService;
        // Carrega dados do usuário logado
        LoadUserData();
        // Carrega preferências visuais persistidas
        _ = LoadPreferencesAsync();

        // Opções com ícones: tap gesture
        var editProfileTap = new TapGestureRecognizer();
        editProfileTap.Tapped += async (s, e) => {
            if (_authService == null || string.IsNullOrEmpty(_userEmail)) { await DisplayAlert("Erro", "Serviço de autenticação ou usuário não disponível.", "OK"); return; }
            await EditProfileAsync();
        };
        EditProfileOption.GestureRecognizers.Add(editProfileTap);

        var changePasswordTap = new TapGestureRecognizer();
        changePasswordTap.Tapped += async (s, e) => {
            if (_authService == null || string.IsNullOrEmpty(_userEmail)) { await DisplayAlert("Erro", "Serviço de autenticação ou usuário não disponível.", "OK"); return; }
            await ChangePasswordAsync();
        };
        ChangePasswordOption.GestureRecognizers.Add(changePasswordTap);

        var preferencesTap = new TapGestureRecognizer();
        preferencesTap.Tapped += async (s, e) => await ShowPreferencesModal();
        PreferencesOption.GestureRecognizers.Add(preferencesTap);

        var aboutTap = new TapGestureRecognizer();
        aboutTap.Tapped += (s, e) => DisplayAlert("Sobre o App", "FMIAutomation v1.0\nDesenvolvido por você!", "OK");
        AboutOption.GestureRecognizers.Add(aboutTap);

        var deleteAccountTap = new TapGestureRecognizer();
        deleteAccountTap.Tapped += async (s, e) => {
            if (_authService == null || string.IsNullOrEmpty(_userEmail)) { await DisplayAlert("Erro", "Serviço de autenticação ou usuário não disponível.", "OK"); return; }
            await DeleteAccountAsync();
        };
        DeleteAccountOption.GestureRecognizers.Add(deleteAccountTap);

        LogoutBtn.Clicked += async (s, e) => {
            try { Microsoft.Maui.Storage.SecureStorage.Remove("login_time"); } catch {}
            var mainPage = (FMIAutomation.MainPage?)Application.Current?.Handler?.MauiContext?.Services.GetService(typeof(FMIAutomation.MainPage));
            if (Application.Current?.MainPage != null)
                Application.Current.MainPage = mainPage ?? new FMIAutomation.MainPage(new FMIAutomation.Services.AuthService("https://fmiautomation-60e6e-default-rtdb.firebaseio.com/"));
        };

    }

    // Carrega preferências salvas e aplica
    private async Task LoadPreferencesAsync()
    {
        var color = await Microsoft.Maui.Storage.SecureStorage.GetAsync("dominant_color");
        if (!string.IsNullOrWhiteSpace(color))
        {
            if (Application.Current != null)
            {
                Application.Current.Resources["DominantColor"] = Color.FromArgb(color);
            }
            this.BackgroundColor = Color.FromArgb(color);
        }
        var lang = await Microsoft.Maui.Storage.SecureStorage.GetAsync("app_language");
        if (!string.IsNullOrWhiteSpace(lang))
        {
            // Aqui você pode aplicar lógica de idioma se desejar
            // Exemplo: Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
        }
    }

    // Salva a cor dominante escolhida e persiste
    private void SetDominantColor(string hexColor)
    {
        if (Application.Current != null)
        {
            Application.Current.Resources["DominantColor"] = Color.FromArgb(hexColor);
        }
        this.BackgroundColor = Color.FromArgb(hexColor);
        Microsoft.Maui.Storage.SecureStorage.SetAsync("dominant_color", hexColor);
    }

    // Modal elegante de preferências: cor dominante e idioma
    private async Task ShowPreferencesModal()
    {
        var picker = new Picker
        {
            Title = "Escolha o idioma",
            ItemsSource = new string[] { "Português", "English" },
            SelectedIndex = GetCurrentLanguageIndex(),
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#374151"),
            HorizontalOptions = LayoutOptions.Fill
        };
        picker.SelectedIndexChanged += async (s, e) =>
        {
            var idx = picker.SelectedIndex;
            var lang = idx == 1 ? "en" : "pt";
            await Microsoft.Maui.Storage.SecureStorage.SetAsync("app_language", lang);
            // Aqui você pode aplicar lógica de idioma se desejar
        };
        var modalPage = new ContentPage
        {
            BackgroundColor = Color.FromArgb("#25303B"),
            Content = new VerticalStackLayout
            {
                Padding = 24,
                Spacing = 24,
                Children =
                {
                    new Label { Text = "Preferências", FontSize = 22, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center },
                    new Label { Text = "Cor do Aplicativo", FontSize = 16, TextColor = Colors.White },
                    new HorizontalStackLayout
                    {
                        Spacing = 16,
                        Children =
                        {
                            new Button { Text = "Atual (Escura)", BackgroundColor = Color.FromArgb("#25303B"), TextColor = Colors.White, CornerRadius = 12, Command = new Command(() => { SetDominantColor("#25303B"); }) },
                            new Button { Text = "Mais Clara", BackgroundColor = Color.FromArgb("#4B5A6A"), TextColor = Colors.White, CornerRadius = 12, Command = new Command(() => { SetDominantColor("#4B5A6A"); }) }
                        }
                    },
                    new Label { Text = "Idioma", FontSize = 16, TextColor = Colors.White },
                    picker,
                    new Button { Text = "Fechar", BackgroundColor = Color.FromArgb("#374151"), TextColor = Colors.White, CornerRadius = 12, Command = new Command(async () => {
                        if (Application.Current?.MainPage?.Navigation != null)
                        {
                            await Application.Current.MainPage.Navigation.PopModalAsync();
                        }
                    }) }
                }
            }
        };
        if (Application.Current?.MainPage?.Navigation != null)
        {
            await Application.Current.MainPage.Navigation.PushModalAsync(modalPage);
        }
    }

    // Retorna o índice do idioma atual (0 = pt, 1 = en)
    private int GetCurrentLanguageIndex()
    {
        var lang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return lang == "en" ? 1 : 0;
    }

        private async void LoadUserData()
        {
            // Recupera email do usuário logado do SecureStorage
            var email = await Microsoft.Maui.Storage.SecureStorage.GetAsync("user_email");
            if (string.IsNullOrEmpty(email) || _authService == null)
            {
                NameLabel.Text = "Usuário";
                EmailLabel.Text = "-";
                _userEmail = "";
                return;
            }
            _userEmail = email;
            EmailLabel.Text = email;
            // Busca nome do usuário no AuthService
            try
            {
                var info = await _authService.GetUserInfoAsync(email);
                NameLabel.Text = info.Nome ?? "Usuário";
            }
            catch { NameLabel.Text = "Usuário"; }
        }



        private async Task EditProfileAsync()
        {
            string newNome = await DisplayPromptAsync("Editar Nome", "Digite o novo nome:", initialValue: NameLabel.Text);
            if (string.IsNullOrWhiteSpace(newNome)) return;
            string newEmail = await DisplayPromptAsync("Editar Email", "Digite o novo e-mail:", initialValue: EmailLabel.Text, keyboard: Keyboard.Email);
            if (string.IsNullOrWhiteSpace(newEmail)) return;
            if (_authService == null) { await DisplayAlert("Erro", "Serviço de autenticação não disponível.", "OK"); return; }
            var result = await _authService.UpdateProfileAsync(_userEmail, newNome, newEmail);
            if (result == null)
            {
                await DisplayAlert("Sucesso", "Dados atualizados!", "OK");
                _userEmail = newEmail;
                NameLabel.Text = newNome;
                EmailLabel.Text = newEmail;
                await Microsoft.Maui.Storage.SecureStorage.SetAsync("user_email", newEmail);
            }
            else
            {
                await DisplayAlert("Erro", result, "OK");
            }
        }

        private async Task ChangePasswordAsync()
        {
            string oldPassword = await SecurePasswordPrompt("Senha Atual", "Digite sua senha atual:");
            if (string.IsNullOrWhiteSpace(oldPassword)) return;
            string newPassword = await SecurePasswordPrompt("Nova Senha", "Digite a nova senha:");
            if (string.IsNullOrWhiteSpace(newPassword)) return;
            string confirmPassword = await SecurePasswordPrompt("Confirmar Nova Senha", "Confirme a nova senha:");
            if (string.IsNullOrWhiteSpace(confirmPassword)) return;
            if (_authService == null) { await DisplayAlert("Erro", "Serviço de autenticação não disponível.", "OK"); return; }
            var result = await _authService.ChangePasswordAsync(_userEmail, oldPassword, newPassword, confirmPassword);
            if (result == null)
                await DisplayAlert("Sucesso", "Senha alterada!", "OK");
            else
                await DisplayAlert("Erro", result, "OK");
        }

        private async Task DeleteAccountAsync()
        {
            string senha = await SecurePasswordPrompt("Excluir Conta", "Digite sua senha para confirmar:");
            if (string.IsNullOrWhiteSpace(senha)) return;
            bool confirm = await DisplayAlert("Excluir Conta", "Tem certeza que deseja excluir sua conta? Esta ação não pode ser desfeita.", "Sim", "Não");
            if (!confirm) return;
            if (_authService == null) { await DisplayAlert("Erro", "Serviço de autenticação não disponível.", "OK"); return; }
            var result = await _authService.DeleteAccountAsync(_userEmail, senha);
            if (result)
            {
                await DisplayAlert("Conta Excluída", "Sua conta foi excluída.", "OK");
                try { Microsoft.Maui.Storage.SecureStorage.Remove("login_time"); Microsoft.Maui.Storage.SecureStorage.Remove("user_email"); } catch {}
                var mainPage = (FMIAutomation.MainPage?)Application.Current?.Handler?.MauiContext?.Services.GetService(typeof(FMIAutomation.MainPage));
                if (Application.Current?.MainPage != null)
                    Application.Current.MainPage = mainPage ?? new FMIAutomation.MainPage(new FMIAutomation.Services.AuthService("https://fmiautomation-60e6e-default-rtdb.firebaseio.com/"));
            }
            else
            {
                await DisplayAlert("Erro", "Senha incorreta ou erro ao excluir.", "OK");
            }
        }

        // Workaround para prompt de senha (sem isPassword nativo)
        private async Task<string> SecurePasswordPrompt(string title, string message)
        {
            string senha = string.Empty;
            var tcs = new TaskCompletionSource<string>();
            var entry = new Entry { IsPassword = true, Placeholder = "Senha" };
            var layout = new VerticalStackLayout { Padding = 10, Spacing = 10 };
            layout.Add(new Label { Text = message });
            layout.Add(entry);
            var popup = new ContentPage
            {
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        layout,
                        new HorizontalStackLayout
                        {
                            Spacing = 10,
                            Children =
                            {
                                new Button { Text = "OK", Command = new Command(() => {
                                    tcs.TrySetResult(entry.Text ?? "");
                                    if (Application.Current?.MainPage?.Navigation != null)
                                        Application.Current.MainPage.Navigation.PopModalAsync();
                                }) },
                                new Button { Text = "Cancelar", Command = new Command(() => {
                                    tcs.TrySetResult("");
                                    if (Application.Current?.MainPage?.Navigation != null)
                                        Application.Current.MainPage.Navigation.PopModalAsync();
                                }) }
                            }
                        }
                    }
                }
            };
            if (Application.Current?.MainPage?.Navigation != null)
                await Application.Current.MainPage.Navigation.PushModalAsync(popup);
            senha = await tcs.Task;
            return senha;
        }
    }
}
