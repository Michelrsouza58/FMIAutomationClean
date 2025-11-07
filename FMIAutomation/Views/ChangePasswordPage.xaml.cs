using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using FMIAutomation.Services;

namespace FMIAutomation.Views
{
    public partial class ChangePasswordPage : BaseContentPage
    {
        private readonly Services.IAuthService _authService;
        private string _userEmail = "";
        
        public ChangePasswordPage()
        {
            InitializeComponent();
            
            _authService = GetAuthService();
            
            // Configura eventos dos botões
            ChangePasswordButton.Clicked += (s, e) => { RegisterUserActivity(); OnChangePasswordClicked(s, e); };
            CancelButton.Clicked += (s, e) => { RegisterUserActivity(); OnCancelClicked(s, e); };
            
            // Monitora força da senha
            NewPasswordEntry.TextChanged += (s, e) => { RegisterUserActivity(); OnNewPasswordTextChanged(s, e); };
            
            // Carrega email do usuário
            _ = LoadUserEmail();
        }

        private Services.IAuthService GetAuthService()
        {
            try
            {
                var services = Handler?.MauiContext?.Services ?? Application.Current?.Handler?.MauiContext?.Services;
                if (services != null)
                {
                    var service = services.GetService<Services.IAuthService>();
                    if (service != null) return service;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ChangePasswordPage] Erro ao obter AuthService: {ex.Message}");
            }

            return new Services.AuthService("https://fmiautomation-60e6e-default-rtdb.firebaseio.com/");
        }

        private async Task LoadUserEmail()
        {
            try
            {
                var email = await Microsoft.Maui.Storage.SecureStorage.GetAsync("user_email");
                
                if (string.IsNullOrEmpty(email))
                {
                    await DisplayAlert("Erro", "Usuário não identificado. Faça login novamente.", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }
                
                _userEmail = email;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
                System.Diagnostics.Debug.WriteLine($"[ChangePasswordPage] Erro LoadUserEmail: {ex.Message}");
            }
        }

        private void OnNewPasswordTextChanged(object sender, TextChangedEventArgs e)
        {
            var password = e.NewTextValue ?? "";
            
            if (string.IsNullOrEmpty(password))
            {
                PasswordStrengthLabel.Text = "Mínimo 6 caracteres";
                PasswordStrengthLabel.TextColor = Colors.Gray;
            }
            else if (password.Length < 6)
            {
                PasswordStrengthLabel.Text = "❌ Senha muito fraca";
                PasswordStrengthLabel.TextColor = Colors.Red;
            }
            else if (password.Length < 8)
            {
                PasswordStrengthLabel.Text = "⚠️ Senha fraca";
                PasswordStrengthLabel.TextColor = Colors.Orange;
            }
            else if (HasMixedCharacters(password))
            {
                PasswordStrengthLabel.Text = "✅ Senha forte";
                PasswordStrengthLabel.TextColor = Colors.Green;
            }
            else
            {
                PasswordStrengthLabel.Text = "⚠️ Senha média";
                PasswordStrengthLabel.TextColor = Colors.Orange;
            }
        }

        private bool HasMixedCharacters(string password)
        {
            return password.Any(char.IsUpper) && 
                   password.Any(char.IsLower) && 
                   password.Any(char.IsDigit);
        }

        private async void OnChangePasswordClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CurrentPasswordEntry.Text))
                {
                    await DisplayAlert("Erro", "Digite sua senha atual.", "OK");
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(NewPasswordEntry.Text))
                {
                    await DisplayAlert("Erro", "Digite sua nova senha.", "OK");
                    return;
                }
                
                if (NewPasswordEntry.Text.Length < 6)
                {
                    await DisplayAlert("Erro", "A nova senha deve ter pelo menos 6 caracteres.", "OK");
                    return;
                }
                
                if (NewPasswordEntry.Text != ConfirmPasswordEntry.Text)
                {
                    await DisplayAlert("Erro", "A confirmação da senha não confere.", "OK");
                    return;
                }

                ChangePasswordButton.IsEnabled = false;
                ChangePasswordButton.Text = "🔒  Alterando...";

                // Aqui implementaria a lógica de alteração de senha
                // Por enquanto, vou simular uma operação bem-sucedida
                await Task.Delay(2000); // Simula processamento
                
                await DisplayAlert("Sucesso", "Senha alterada com sucesso!", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao alterar senha: {ex.Message}", "OK");
            }
            finally
            {
                ChangePasswordButton.IsEnabled = true;
                ChangePasswordButton.Text = "🔒  Alterar Senha";
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}