using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using FMIAutomation.Services;

namespace FMIAutomation.Views
{
    public partial class EditProfilePage : ContentPage
    {
        private readonly Services.IAuthService _authService;
        private string _userEmail = "";
        
        public EditProfilePage()
        {
            InitializeComponent();
            
            _authService = GetAuthService();
            
            // Configura eventos dos botões
            SaveButton.Clicked += OnSaveClicked;
            CancelButton.Clicked += OnCancelClicked;
            
            // Carrega dados do usuário
            _ = LoadUserData();
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
                System.Diagnostics.Debug.WriteLine($"[EditProfilePage] Erro ao obter AuthService: {ex.Message}");
            }

            return new Services.AuthService("https://fmiautomation-60e6e-default-rtdb.firebaseio.com/");
        }

        private async Task LoadUserData()
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
                
                var userInfo = await _authService.GetUserInfoAsync(email);
                
                if (userInfo != null)
                {
                    NameEntry.Text = userInfo.Nome ?? "";
                    EmailEntry.Text = userInfo.Email ?? email;
                    PhoneEntry.Text = userInfo.Telefone ?? "";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
                System.Diagnostics.Debug.WriteLine($"[EditProfilePage] Erro LoadUserData: {ex.Message}");
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NameEntry.Text))
                {
                    await DisplayAlert("Erro", "Nome é obrigatório.", "OK");
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(EmailEntry.Text))
                {
                    await DisplayAlert("Erro", "Email é obrigatório.", "OK");
                    return;
                }

                SaveButton.IsEnabled = false;
                SaveButton.Text = "💾  Salvando...";

                var userInfo = await _authService.GetUserInfoAsync(_userEmail);
                
                var result = await _authService.UpdateProfileWithImageAsync(
                    _userEmail,
                    NameEntry.Text.Trim(),
                    EmailEntry.Text.Trim(),
                    PhoneEntry.Text?.Trim(),
                    userInfo?.ProfileImageBase64
                );

                if (result == null)
                {
                    await DisplayAlert("Sucesso", "Perfil atualizado com sucesso!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await DisplayAlert("Erro", result, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao salvar: {ex.Message}", "OK");
            }
            finally
            {
                SaveButton.IsEnabled = true;
                SaveButton.Text = "💾  Salvar Alterações";
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}