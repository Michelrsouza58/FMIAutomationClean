using Microsoft.Maui.Controls;
using FMIAutomation.Services;

namespace FMIAutomation.Views
{
    public partial class ChangePasswordModal : ContentPage
    {
        private readonly IAuthService? _authService;
        private string _userEmail = "";

        public ChangePasswordModal(IAuthService authService, string userEmail)
        {
            InitializeComponent();
            _authService = authService;
            _userEmail = userEmail;
            
            SetupEvents();
            StartSlideUpAnimation();
        }

        private void SetupEvents()
        {
            CloseButton.Clicked += OnCloseClicked;
            CancelButton.Clicked += OnCloseClicked;
            ChangePasswordButton.Clicked += OnChangePasswordClicked;
            
            // Toggle password visibility
            ToggleCurrentPasswordButton.Clicked += (s, e) => TogglePasswordVisibility(CurrentPasswordEntry, ToggleCurrentPasswordButton);
            ToggleNewPasswordButton.Clicked += (s, e) => TogglePasswordVisibility(NewPasswordEntry, ToggleNewPasswordButton);
            ToggleConfirmPasswordButton.Clicked += (s, e) => TogglePasswordVisibility(ConfirmPasswordEntry, ToggleConfirmPasswordButton);
            
            // Password validation
            NewPasswordEntry.TextChanged += OnNewPasswordChanged;
            ConfirmPasswordEntry.TextChanged += OnConfirmPasswordChanged;
        }

        private void TogglePasswordVisibility(Entry passwordEntry, Button toggleButton)
        {
            passwordEntry.IsPassword = !passwordEntry.IsPassword;
            toggleButton.Text = passwordEntry.IsPassword ? "👁️" : "🙈";
        }

        private void OnNewPasswordChanged(object? sender, TextChangedEventArgs e)
        {
            var password = e.NewTextValue ?? "";
            var strength = GetPasswordStrength(password);
            
            PasswordStrengthLabel.Text = strength.Message;
            PasswordStrengthLabel.TextColor = strength.Color;
            
            ValidatePasswordMatch();
        }

        private void OnConfirmPasswordChanged(object? sender, TextChangedEventArgs e)
        {
            ValidatePasswordMatch();
        }

        private void ValidatePasswordMatch()
        {
            var newPassword = NewPasswordEntry.Text ?? "";
            var confirmPassword = ConfirmPasswordEntry.Text ?? "";
            
            if (string.IsNullOrEmpty(confirmPassword))
            {
                PasswordMatchLabel.Text = " ";
                return;
            }
            
            if (newPassword == confirmPassword)
            {
                PasswordMatchLabel.Text = "✅ Senhas coincidem";
                PasswordMatchLabel.TextColor = Colors.Green;
            }
            else
            {
                PasswordMatchLabel.Text = "❌ Senhas não coincidem";
                PasswordMatchLabel.TextColor = Color.FromArgb("#DC2626");
            }
        }

        private (string Message, Color Color) GetPasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return ("Mínimo 6 caracteres", Color.FromArgb("#6B7280"));
            }
            
            if (password.Length < 6)
            {
                return ("❌ Muito fraca - mínimo 6 caracteres", Color.FromArgb("#DC2626"));
            }
            
            int score = 0;
            if (password.Length >= 8) score++;
            if (password.Any(char.IsUpper)) score++;
            if (password.Any(char.IsLower)) score++;
            if (password.Any(char.IsDigit)) score++;
            if (password.Any(c => !char.IsLetterOrDigit(c))) score++;
            
            return score switch
            {
                1 => ("🟡 Fraca", Color.FromArgb("#F59E0B")),
                2 => ("🟡 Regular", Color.FromArgb("#F59E0B")),
                3 => ("🟢 Boa", Color.FromArgb("#10B981")),
                4 or 5 => ("🟢 Muito forte", Color.FromArgb("#059669")),
                _ => ("❌ Muito fraca", Color.FromArgb("#DC2626"))
            };
        }

        private async void OnChangePasswordClicked(object? sender, EventArgs e)
        {
            try
            {
                var currentPassword = CurrentPasswordEntry.Text?.Trim();
                var newPassword = NewPasswordEntry.Text?.Trim();
                var confirmPassword = ConfirmPasswordEntry.Text?.Trim();

                // Validações
                if (string.IsNullOrWhiteSpace(currentPassword))
                {
                    await DisplayAlert("❌ Erro", "Digite sua senha atual!", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    await DisplayAlert("❌ Erro", "Digite a nova senha!", "OK");
                    return;
                }

                if (newPassword.Length < 6)
                {
                    await DisplayAlert("❌ Erro", "A nova senha deve ter pelo menos 6 caracteres!", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(confirmPassword))
                {
                    await DisplayAlert("❌ Erro", "Confirme a nova senha!", "OK");
                    return;
                }

                if (newPassword != confirmPassword)
                {
                    await DisplayAlert("❌ Erro", "As senhas não coincidem!", "OK");
                    return;
                }

                if (currentPassword == newPassword)
                {
                    await DisplayAlert("❌ Erro", "A nova senha deve ser diferente da atual!", "OK");
                    return;
                }

                if (_authService == null)
                {
                    await DisplayAlert("❌ Erro", "Serviço de autenticação não disponível.", "OK");
                    return;
                }

                // Mostrar loading
                ChangePasswordButton.Text = "🔄 Alterando...";
                ChangePasswordButton.IsEnabled = false;

                var result = await _authService.ChangePasswordAsync(_userEmail, currentPassword, newPassword, confirmPassword);
                
                if (result == null)
                {
                    await DisplayAlert("✅ Sucesso", "Senha alterada com sucesso!", "OK");
                    await CloseModal();
                }
                else
                {
                    await DisplayAlert("❌ Erro", result, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao alterar senha: {ex.Message}", "OK");
            }
            finally
            {
                ChangePasswordButton.Text = "🔄 Alterar Senha";
                ChangePasswordButton.IsEnabled = true;
            }
        }

        private async void OnCloseClicked(object? sender, EventArgs e)
        {
            await CloseModal();
        }

        private void StartSlideUpAnimation()
        {
            MainContent.TranslationY = 500;
            MainContent.Opacity = 0;

            MainContent.TranslateTo(0, 0, 400, Easing.CubicOut);
            MainContent.FadeTo(1, 300);
        }

        private async Task CloseModal()
        {
            await MainContent.TranslateTo(0, 500, 300, Easing.CubicIn);
            await MainContent.FadeTo(0, 200);
            
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.Navigation.PopModalAsync();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            Task.Run(async () => await CloseModal());
            return true;
        }
    }
}