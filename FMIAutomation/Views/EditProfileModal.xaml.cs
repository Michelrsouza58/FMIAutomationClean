using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using FMIAutomation.Services;

namespace FMIAutomation.Views
{
    public partial class EditProfileModal : ContentPage
    {
        private readonly IAuthService? _authService;
        private string _userEmail = "";
        private string _profileImageBase64 = "";

        public EditProfileModal(IAuthService authService, string userEmail)
        {
            InitializeComponent();
            _authService = authService;
            _userEmail = userEmail;
            
            SetupEvents();
            LoadUserData();
            StartSlideUpAnimation();
        }

        private void SetupEvents()
        {
            CloseButton.Clicked += OnCloseClicked;
            CancelButton.Clicked += OnCloseClicked;
            SaveButton.Clicked += OnSaveClicked;
            UploadPhotoButton.Clicked += OnUploadPhotoClicked;
        }

        private async void LoadUserData()
        {
            try
            {
                if (_authService == null || string.IsNullOrEmpty(_userEmail)) return;

                var userInfo = await _authService.GetUserInfoAsync(_userEmail);
                if (userInfo != null)
                {
                    NameEntry.Text = userInfo.Nome ?? "";
                    EmailEntry.Text = userInfo.Email ?? "";
                    PhoneEntry.Text = userInfo.Telefone ?? "";
                    
                    // Carregar foto do perfil se existir
                    if (!string.IsNullOrEmpty(userInfo.ProfileImageBase64))
                    {
                        _profileImageBase64 = userInfo.ProfileImageBase64;
                        var imageBytes = Convert.FromBase64String(userInfo.ProfileImageBase64);
                        ProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
            }
        }

        private async void OnUploadPhotoClicked(object? sender, EventArgs e)
        {
            try
            {
                var result = await DisplayActionSheet("Selecionar Foto", "Cancelar", null, "📷 Câmera", "🖼️ Galeria");
                
                FileResult? photo = null;
                
                if (result == "📷 Câmera")
                {
                    if (MediaPicker.Default.IsCaptureSupported)
                    {
                        var status = await Permissions.RequestAsync<Permissions.Camera>();
                        if (status == PermissionStatus.Granted)
                        {
                            photo = await MediaPicker.Default.CapturePhotoAsync();
                        }
                        else
                        {
                            await DisplayAlert("Permissão Negada", "É necessário permitir acesso à câmera", "OK");
                            return;
                        }
                    }
                    else
                    {
                        await DisplayAlert("Erro", "Câmera não disponível", "OK");
                        return;
                    }
                }
                else if (result == "🖼️ Galeria")
                {
                    var status = await Permissions.RequestAsync<Permissions.Photos>();
                    if (status == PermissionStatus.Granted)
                    {
                        photo = await MediaPicker.Default.PickPhotoAsync();
                    }
                    else
                    {
                        await DisplayAlert("Permissão Negada", "É necessário permitir acesso às fotos", "OK");
                        return;
                    }
                }

                if (photo != null)
                {
                    // Converter para base64
                    using var stream = await photo.OpenReadAsync();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    var imageBytes = memoryStream.ToArray();
                    
                    // Redimensionar se necessário (opcional - para economizar espaço)
                    if (imageBytes.Length > 500000) // 500KB
                    {
                        imageBytes = await ResizeImageAsync(imageBytes, 300, 300);
                    }
                    
                    _profileImageBase64 = Convert.ToBase64String(imageBytes);
                    
                    // Exibir a imagem
                    ProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao selecionar foto: {ex.Message}", "OK");
            }
        }

        private Task<byte[]> ResizeImageAsync(byte[] imageBytes, int maxWidth, int maxHeight)
        {
            // Implementação básica - em produção, usar biblioteca como ImageSharp
            // Por agora, retorna a imagem original
            return Task.FromResult(imageBytes);
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            try
            {
                var name = NameEntry.Text?.Trim();
                var email = EmailEntry.Text?.Trim();
                var phone = PhoneEntry.Text?.Trim();

                if (string.IsNullOrWhiteSpace(name))
                {
                    await DisplayAlert("Erro", "Nome é obrigatório!", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    await DisplayAlert("Erro", "E-mail é obrigatório!", "OK");
                    return;
                }

                if (_authService == null)
                {
                    await DisplayAlert("Erro", "Serviço de autenticação não disponível.", "OK");
                    return;
                }

                // Mostrar loading
                SaveButton.Text = "💾 Salvando...";
                SaveButton.IsEnabled = false;

                // Atualizar perfil incluindo foto
                var result = await _authService.UpdateProfileWithImageAsync(_userEmail, name, email, phone, _profileImageBase64);
                
                if (result == null)
                {
                    await DisplayAlert("✅ Sucesso", "Perfil atualizado com sucesso!", "OK");
                    await SecureStorage.SetAsync("user_email", email);
                    await CloseModal();
                }
                else
                {
                    await DisplayAlert("❌ Erro", result, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao salvar: {ex.Message}", "OK");
            }
            finally
            {
                SaveButton.Text = "💾 Salvar Alterações";
                SaveButton.IsEnabled = true;
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