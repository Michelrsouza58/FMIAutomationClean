using FMIAutomation.Services;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace FMIAutomation
{
    public partial class MainPage : ContentPage
    {
        private bool isCadastro = false;
        private readonly IAuthService _authService;

        public MainPage(IAuthService authService)
        {
            _authService = authService;
            InitializeComponent();
            
            // Inicializar tema
            _ = InitializeThemeAsync();
            
            SetupToggle();
            EntrarBtn.Clicked += EntrarBtn_Clicked;
        }

        private async Task InitializeThemeAsync()
        {
            await ThemeService.InitializeAsync();
        }		private async void EntrarBtn_Clicked(object? sender, EventArgs e)
		{
			if (isCadastro)
			{
				// Validação dos campos de cadastro
				string nomeCadastro = NomeEntry?.Text?.Trim() ?? string.Empty;
				string emailCadastro = EmailEntry?.Text?.Trim() ?? string.Empty;
				string confirmarEmailCadastro = ConfirmarEmailEntry?.Text?.Trim() ?? string.Empty;
				string telefoneCadastro = TelefoneEntry?.Text?.Trim() ?? string.Empty;
				string senhaCadastro = SenhaEntry?.Text ?? string.Empty;
				string confirmarSenhaCadastro = ConfirmarSenhaEntry?.Text ?? string.Empty;

				// Validação de campos obrigatórios
				if (string.IsNullOrWhiteSpace(nomeCadastro))
				{
					await DisplayAlert("Erro", "Nome é obrigatório.", "OK");
					return;
				}

				if (string.IsNullOrWhiteSpace(emailCadastro))
				{
					await DisplayAlert("Erro", "E-mail é obrigatório.", "OK");
					return;
				}

				if (string.IsNullOrWhiteSpace(confirmarEmailCadastro))
				{
					await DisplayAlert("Erro", "Confirmação de e-mail é obrigatória.", "OK");
					return;
				}

				// Validação se os emails coincidem
				if (emailCadastro != confirmarEmailCadastro)
				{
					await DisplayAlert("Erro", "E-mail e confirmação de e-mail não coincidem.", "OK");
					return;
				}

				if (string.IsNullOrWhiteSpace(senhaCadastro))
				{
					await DisplayAlert("Erro", "Senha é obrigatória.", "OK");
					return;
				}

				if (string.IsNullOrWhiteSpace(confirmarSenhaCadastro))
				{
					await DisplayAlert("Erro", "Confirmação de senha é obrigatória.", "OK");
					return;
				}

				EntrarBtn.IsEnabled = false;
				EntrarBtn.Text = "Cadastrando...";
				try
				{
					// Usar sobrecarga com telefone (pode ser null/empty se não preenchido)
					string? telefoneParaCadastro = string.IsNullOrWhiteSpace(telefoneCadastro) ? null : telefoneCadastro;
					var result = await _authService.RegisterAsync(nomeCadastro, emailCadastro, senhaCadastro, confirmarSenhaCadastro, telefoneParaCadastro);
					
					if (result == null)
					{
						await DisplayAlert("Sucesso", "Cadastro realizado com sucesso! 🎉\n\nVocê já pode fazer login.", "OK");
						ShowLogin();
					}
					else
					{
						await DisplayAlert("Erro", result, "OK");
					}
				}
				catch (Exception ex)
				{
					await DisplayAlert("Erro", $"Falha ao cadastrar: {ex.Message}", "OK");
				}
				finally
				{
					EntrarBtn.IsEnabled = true;
					EntrarBtn.Text = isCadastro ? "CADASTRAR" : "ENTRAR";
				}
				return;
			}

			if (EmailEntry == null || SenhaEntry == null)
			{
				await DisplayAlert("Erro", "Campos de login não encontrados na tela.", "OK");
				return;
			}
			if (_authService == null)
			{
				await DisplayAlert("Erro", "Serviço de autenticação não disponível.", "OK");
				return;
			}
			string email = EmailEntry.Text?.Trim() ?? string.Empty;
			string senha = SenhaEntry.Text ?? string.Empty;
			if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
			{
				await DisplayAlert("Erro", "Preencha e-mail e senha.", "OK");
				return;
			}

			EntrarBtn.IsEnabled = false;
			EntrarBtn.Text = "Entrando...";
			try
			{
				bool success = await _authService.LoginAsync(email, senha);
				if (success)
				{
					// Salva o tempo de login E o email do usuário
					await Microsoft.Maui.Storage.SecureStorage.SetAsync("login_time", DateTime.UtcNow.Ticks.ToString());
					await Microsoft.Maui.Storage.SecureStorage.SetAsync("user_email", email);
					
					System.Diagnostics.Debug.WriteLine($"[LOGIN] Email salvo no SecureStorage: '{email}'");
					
					if (Application.Current != null)
					{
						Application.Current.MainPage = new AppShell();
					}
				}
				else
				{
					await DisplayAlert("Erro", "E-mail ou senha inválidos.", "OK");
				}
			}
			catch (Exception ex)
			{
				await DisplayAlert("Erro", $"Falha ao autenticar: {ex.Message}", "OK");
			}
			finally
			{
				EntrarBtn.IsEnabled = true;
				EntrarBtn.Text = isCadastro ? "CADASTRAR" : "ENTRAR";
			}
		}

		private void SetupToggle()
		{
			LoginTab.Clicked += (s, e) => ShowLogin();
			CadastroTab.Clicked += (s, e) => ShowCadastro();
			ShowLogin();
		}

		private void ShowLogin()
		{
			isCadastro = false;
			LoginTab.BackgroundColor = Color.FromArgb("#3B5A7A");
			LoginTab.TextColor = Colors.White;
			CadastroTab.BackgroundColor = Colors.White;
			CadastroTab.TextColor = Color.FromArgb("#3B5A7A");
			EntrarBtn.Text = "ENTRAR";

			// Mostrar apenas campos do login
			NomeEntry.IsVisible = false;
			ConfirmarEmailEntry.IsVisible = false;
			TelefoneEntry.IsVisible = false;
			ConfirmarSenhaEntry.IsVisible = false;
			LembrarStackLayout.IsVisible = true;

			// Limpar campos
			NomeEntry.Text = string.Empty;
			ConfirmarEmailEntry.Text = string.Empty;
			TelefoneEntry.Text = string.Empty;
			ConfirmarSenhaEntry.Text = string.Empty;
		}

		private void ShowCadastro()
		{
			isCadastro = true;
			CadastroTab.BackgroundColor = Color.FromArgb("#3B5A7A");
			CadastroTab.TextColor = Colors.White;
			LoginTab.BackgroundColor = Colors.White;
			LoginTab.TextColor = Color.FromArgb("#3B5A7A");
			EntrarBtn.Text = "CADASTRAR";

			// Mostrar campos do cadastro
			NomeEntry.IsVisible = true;
			ConfirmarEmailEntry.IsVisible = true;
			TelefoneEntry.IsVisible = true;
			ConfirmarSenhaEntry.IsVisible = true;
			LembrarStackLayout.IsVisible = false; // Esconder "Lembrar-me a senha"

			// Limpar campos
			EmailEntry.Text = string.Empty;
			SenhaEntry.Text = string.Empty;
		}
	}
}
