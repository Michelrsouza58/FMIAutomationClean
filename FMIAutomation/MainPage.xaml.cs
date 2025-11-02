
using FMIAutomation.Services;
using FMIAutomation.Views;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using Microsoft.Maui.Authentication;
using System.Web;

namespace FMIAutomation;


public partial class MainPage : ContentPage
{
	private bool isCadastro = false;
	private Entry? nomeEntry, confirmarSenhaEntry;
	private readonly IAuthService _authService;

	public MainPage()
	{
		_authService = Application.Current?.Handler?.MauiContext?.Services.GetService(typeof(IAuthService)) as IAuthService
			?? throw new System.Exception("AuthService not found in DI");
		InitializeComponent();
		SetupToggle();
		EntrarBtn.Clicked += EntrarBtn_Clicked;

		// Adicionar handler para login Google usando WebAuthenticator
		var googleBtn = this.FindByName<ImageButton>("GoogleBtn");
		if (googleBtn != null) googleBtn.Clicked += GoogleLogin_Clicked;
	}

	// Handler para login com Google usando WebAuthenticator
	async void GoogleLogin_Clicked(object? sender, EventArgs e)
	{
		try
		{
			// Substitua pelos valores do seu projeto Google Cloud
			var clientId = "890616607205-515t9s75cbdhja2i98td2q1hm4039um9.apps.googleusercontent.com";
			var redirectUri = "com.googleusercontent.apps.890616607205-515t9s75cbdhja2i98td2q1hm4039um9:/oauth2redirect";
			var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={clientId}&redirect_uri={HttpUtility.UrlEncode(redirectUri)}&response_type=code&scope=openid%20email%20profile";
			var callbackUrl = redirectUri;

			var result = await WebAuthenticator.Default.AuthenticateAsync(new Uri(authUrl), new Uri(callbackUrl));
			if (result != null && result.Properties.ContainsKey("code"))
			{
				var code = result.Properties["code"];
				await DisplayAlert("Login Google", $"Código de autorização: {code}", "OK");
				// Troque o código por token de acesso em backend seguro
			}
			else
			{
				await DisplayAlert("Erro", "Login Google cancelado ou sem código.", "OK");
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Erro", ex.Message, "OK");
		}
	}
	private async void EntrarBtn_Clicked(object? sender, EventArgs e)
	{

			if (isCadastro)
			{
				string nomeCadastro = nomeEntry?.Text?.Trim() ?? string.Empty;
				string emailCadastro = EmailEntry.Text?.Trim() ?? string.Empty;
				string senhaCadastro = SenhaEntry.Text ?? string.Empty;
				string confirmarSenhaCadastro = confirmarSenhaEntry?.Text ?? string.Empty;
				if (string.IsNullOrWhiteSpace(nomeCadastro) || string.IsNullOrWhiteSpace(emailCadastro) || string.IsNullOrWhiteSpace(senhaCadastro) || string.IsNullOrWhiteSpace(confirmarSenhaCadastro))
				{
					await DisplayAlert("Erro", "Preencha todos os campos.", "OK");
					return;
				}
				EntrarBtn.IsEnabled = false;
				EntrarBtn.Text = "Cadastrando...";
				try
				{
					var result = await _authService.RegisterAsync(nomeCadastro, emailCadastro, senhaCadastro, confirmarSenhaCadastro);
					if (result == null)
					{
						await DisplayAlert("Sucesso", "Cadastro realizado! Você já pode fazer login.", "OK");
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

		string email = EmailEntry.Text?.Trim() ?? string.Empty;
		string senha = SenhaEntry.Text ?? string.Empty;
		System.Diagnostics.Debug.WriteLine($"[LOGIN UI] Valor lido de EmailEntry: '{EmailEntry.Text}'");
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
				await Shell.Current.GoToAsync($"//BluetoothDevicesPage");
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

		// Remove campos extras se existirem (sempre tenta remover)
		if (this.Content is ScrollView scroll && scroll.Content is Layout layout)
		{
			if (nomeEntry != null && layout.Children.Contains(nomeEntry))
				layout.Children.Remove(nomeEntry);
			if (confirmarSenhaEntry != null && layout.Children.Contains(confirmarSenhaEntry))
				layout.Children.Remove(confirmarSenhaEntry);
		}
	}

	private void ShowCadastro()
	{
		isCadastro = true;
		CadastroTab.BackgroundColor = Color.FromArgb("#3B5A7A");
		CadastroTab.TextColor = Colors.White;
		LoginTab.BackgroundColor = Colors.White;
		LoginTab.TextColor = Color.FromArgb("#3B5A7A");
		EntrarBtn.Text = "CADASTRAR";

		if (this.Content is ScrollView scroll && scroll.Content is Layout layout)
		{
			// Remove se já existirem (garante ordem)
			if (nomeEntry != null && layout.Children.Contains(nomeEntry))
				layout.Children.Remove(nomeEntry);
			if (confirmarSenhaEntry != null && layout.Children.Contains(confirmarSenhaEntry))
				layout.Children.Remove(confirmarSenhaEntry);

			// Cria se necessário
			if (nomeEntry == null)
				nomeEntry = new Entry { Placeholder = "Nome", BackgroundColor = Colors.Transparent, TextColor = Colors.White, PlaceholderColor = Color.FromArgb("#B0B0B0"), HeightRequest = 48 };
			if (confirmarSenhaEntry == null)
				confirmarSenhaEntry = new Entry { Placeholder = "Confirmar Senha", IsPassword = true, BackgroundColor = Colors.Transparent, TextColor = Colors.White, PlaceholderColor = Color.FromArgb("#B0B0B0"), HeightRequest = 48 };

			// Adiciona sempre na ordem correta
			layout.Children.Insert(3, nomeEntry); // Após o toggle
			layout.Children.Insert(6, confirmarSenhaEntry); // Após senha
		}
	}
}
