using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using FMIAutomation.Services;

namespace FMIAutomation.Views
{
    public partial class ProfilePage : BaseContentPage
    {
        private readonly Services.IAuthService _authService;
        private string _userEmail = "";
        
        public ProfilePage()
        {
            InitializeComponent();
            
            // Recupera serviço de autenticação de forma mais robusta
            _authService = GetAuthService();
            
            // Carrega dados do usuário logado
            _ = LoadUserData();
            
            // Configura gestos para as opções
            SetupGestures();
        }

        private Services.IAuthService GetAuthService()
        {
            try
            {
                // Primeira tentativa: usar Handler.MauiContext
                var services = Handler?.MauiContext?.Services;
                if (services != null)
                {
                    var service = services.GetService<Services.IAuthService>();
                    if (service != null) return service;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProfilePage] Erro ao obter AuthService via Handler: {ex.Message}");
            }

            try
            {
                // Segunda tentativa: usar Application.Current
                var services = Application.Current?.Handler?.MauiContext?.Services;
                if (services != null)
                {
                    var service = services.GetService<Services.IAuthService>();
                    if (service != null) return service;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProfilePage] Erro ao obter AuthService via Application: {ex.Message}");
            }

            // Fallback: criar instância direta
            System.Diagnostics.Debug.WriteLine("[ProfilePage] Usando fallback para AuthService");
            return new Services.AuthService("https://fmiautomation-60e6e-default-rtdb.firebaseio.com/");
        }

        private void SetupGestures()
        {
            // Profile Image
            ProfileImageTap.Tapped += OnProfileImageTapped;
            
            // Options
            var editProfileTap = new TapGestureRecognizer();
            editProfileTap.Tapped += async (s, e) => {
                RegisterUserActivity();
                if (string.IsNullOrEmpty(_userEmail)) 
                { 
                    await DisplayAlert("Erro", "Usuário não identificado. Faça login novamente.", "OK"); 
                    return; 
                }
                await Shell.Current.GoToAsync("EditProfilePage", true);
            };
            EditProfileOption.GestureRecognizers.Add(editProfileTap);

            var changePasswordTap = new TapGestureRecognizer();
            changePasswordTap.Tapped += async (s, e) => {
                RegisterUserActivity();
                if (string.IsNullOrEmpty(_userEmail)) 
                { 
                    await DisplayAlert("Erro", "Usuário não identificado. Faça login novamente.", "OK"); 
                    return; 
                }
                await Shell.Current.GoToAsync("ChangePasswordPage", true);
            };
            ChangePasswordOption.GestureRecognizers.Add(changePasswordTap);

            var preferencesTap = new TapGestureRecognizer();
            preferencesTap.Tapped += async (s, e) => { 
                RegisterUserActivity(); 
                await Shell.Current.GoToAsync("PreferencesPage", true); 
            };
            PreferencesOption.GestureRecognizers.Add(preferencesTap);

            var aboutTap = new TapGestureRecognizer();
            aboutTap.Tapped += (s, e) => { 
                RegisterUserActivity(); 
                DisplayAlert("Sobre o App", "FMIAutomation v1.0\n\n🚀 App de Automação FMI\n📱 Desenvolvido com .NET MAUI\n🔥 Firebase Backend\n\n© 2024 - Todos os direitos reservados", "OK"); 
            };
            AboutOption.GestureRecognizers.Add(aboutTap);

            var deleteAccountTap = new TapGestureRecognizer();
            deleteAccountTap.Tapped += async (s, e) => {
                if (string.IsNullOrEmpty(_userEmail)) 
                { 
                    await DisplayAlert("Erro", "Usuário não identificado. Faça login novamente.", "OK"); 
                    return; 
                }
                await DeleteAccountAsync();
            };
            DeleteAccountOption.GestureRecognizers.Add(deleteAccountTap);

            // Logout Button
            LogoutButton.Clicked += async (s, e) => {
                var confirm = await DisplayAlert("Sair", "Deseja realmente sair da sua conta?", "Sim", "Não");
                if (!confirm) return;
                
                try 
                { 
                    Microsoft.Maui.Storage.SecureStorage.Remove("login_time");
                    Microsoft.Maui.Storage.SecureStorage.Remove("user_email");
                } 
                catch {}
                
                var mainPage = (FMIAutomation.MainPage?)Application.Current?.Handler?.MauiContext?.Services.GetService(typeof(FMIAutomation.MainPage));
                if (Application.Current != null)
                {
                    Application.Current.MainPage = mainPage ?? new FMIAutomation.MainPage(new FMIAutomation.Services.AuthService("https://fmiautomation-60e6e-default-rtdb.firebaseio.com/"));
                }
            };
        }

        private async Task LoadUserData()
        {
            try
            {
                var email = await Microsoft.Maui.Storage.SecureStorage.GetAsync("user_email");
                
                if (string.IsNullOrEmpty(email))
                {
                    NameLabel.Text = "Usuário não logado";
                    EmailLabel.Text = "Faça login novamente";
                    _userEmail = "";
                    return;
                }
                
                _userEmail = email;
                
                var userInfo = await _authService.GetUserInfoAsync(email);
                
                if (userInfo != null)
                {
                    NameLabel.Text = userInfo.Nome ?? "Nome não informado";
                    EmailLabel.Text = userInfo.Email ?? email;
                    
                    // Carrega imagem de perfil
                    UpdateProfileImageDisplay(userInfo.ProfileImageBase64);
                }
                else
                {
                    NameLabel.Text = "Dados não encontrados";
                    EmailLabel.Text = email;
                }
            }
            catch (Exception ex)
            {
                NameLabel.Text = "Erro ao carregar dados";
                EmailLabel.Text = "Erro";
                _userEmail = "";
                System.Diagnostics.Debug.WriteLine($"[ProfilePage] Erro LoadUserData: {ex.Message}");
            }
        }

        // Método para exclusão de conta
        private async Task DeleteAccountAsync()
        {
            var confirm = await DisplayAlert("Excluir Conta", "Esta ação não pode ser desfeita. Deseja continuar?", "Sim", "Não");
            if (!confirm) return;
            
            await DisplayAlert("Em breve", "Funcionalidade de excluir conta será implementada.", "OK");
        }

        // Método simplificado para imagem
        private async void OnProfileImageTapped(object? sender, EventArgs e)
        {
            await PickAndUploadImage();
        }

        private async Task PickAndUploadImage()
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Selecione uma foto"
                });

                if (result != null)
                {
                    await ProcessSelectedImage(result);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao selecionar imagem: {ex.Message}", "OK");
            }
        }

        private async Task ProcessSelectedImage(FileResult photo)
        {
            try
            {
                using var stream = await photo.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                var base64String = Convert.ToBase64String(imageBytes);

                await UpdateProfileWithImage(base64String);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao processar imagem: {ex.Message}", "OK");
            }
        }

        private async Task UpdateProfileWithImage(string imageBase64)
        {
            try
            {
                var userInfo = await _authService.GetUserInfoAsync(_userEmail);
                if (userInfo == null)
                {
                    await DisplayAlert("Erro", "Não foi possível carregar as informações do usuário.", "OK");
                    return;
                }

                var result = await _authService.UpdateProfileWithImageAsync(
                    _userEmail, 
                    userInfo.Nome ?? "", 
                    userInfo.Email ?? _userEmail,
                    userInfo.Telefone,
                    imageBase64
                );

                if (result == null)
                {
                    await DisplayAlert("Sucesso", "Foto de perfil atualizada com sucesso!", "OK");
                    await LoadUserData();
                }
                else
                {
                    await DisplayAlert("Erro", result, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao atualizar foto: {ex.Message}", "OK");
            }
        }



        private void UpdateProfileImageDisplay(string? imageBase64)
        {
            if (string.IsNullOrEmpty(imageBase64))
            {
                ProfileImage.IsVisible = false;
                DefaultAvatarLabel.IsVisible = true;
            }
            else
            {
                var imageBytes = Convert.FromBase64String(imageBase64);
                ProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                ProfileImage.IsVisible = true;
                DefaultAvatarLabel.IsVisible = false;
            }
        }
    }
}