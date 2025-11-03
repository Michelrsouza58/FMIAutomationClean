
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

	public MainPage(IAuthService authService)
	{
		_authService = authService;
		InitializeComponent();
		SetupToggle();
		EntrarBtn.Clicked += EntrarBtn_Clicked;

	// Adicionar handler para login Google usando WebAuthenticator
	var googleBtn = this.FindByName<ImageButton>("GoogleBtn");
	if (googleBtn != null) googleBtn.Clicked += GoogleLogin_Clicked;

	// Facebook e Apple (supondo que existam botões com esses nomes)
	var facebookBtn = this.FindByName<ImageButton>("FacebookBtn");
	if (facebookBtn != null) facebookBtn.Clicked += FacebookLogin_Clicked;
	var appleBtn = this.FindByName<ImageButton>("AppleBtn");
	if (appleBtn != null) appleBtn.Clicked += AppleLogin_Clicked;
	}

	// Handler para login com Google usando WebAuthenticator
	async void GoogleLogin_Clicked(object? sender, EventArgs e)
	{
		try
		{
			var clientId = "890616607205-515t9s75cbdhja2i98td2q1hm4039um9.apps.googleusercontent.com";
			var redirectUri = "com.googleusercontent.apps.890616607205-515t9s75cbdhja2i98td2q1hm4039um9:/oauth2redirect";
			var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={clientId}&redirect_uri={HttpUtility.UrlEncode(redirectUri)}&response_type=code&scope=openid%20email%20profile";
			var callbackUrl = redirectUri;

			var result = await WebAuthenticator.Default.AuthenticateAsync(new Uri(authUrl), new Uri(callbackUrl));
			if (result != null && result.Properties.ContainsKey("code"))
			{
				var code = result.Properties["code"];
				using var http = new System.Net.Http.HttpClient();
				var tokenRequest = new System.Net.Http.FormUrlEncodedContent(new[]
				{
					new KeyValuePair<string, string>("code", code),
					new KeyValuePair<string, string>("client_id", clientId),
					new KeyValuePair<string, string>("redirect_uri", redirectUri),
					new KeyValuePair<string, string>("grant_type", "authorization_code"),
				});
				var tokenResponse = await http.PostAsync("https://oauth2.googleapis.com/token", tokenRequest);
				if (!tokenResponse.IsSuccessStatusCode)
				{
					await DisplayAlert("Erro", "Falha ao obter token do Google.", "OK");
					return;
				}
				var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
				var tokenObj = System.Text.Json.JsonDocument.Parse(tokenJson).RootElement;
				var idToken = tokenObj.GetProperty("id_token").GetString();
				if (string.IsNullOrEmpty(idToken))
				{
					await DisplayAlert("Erro", "Token de ID não retornado pelo Google.", "OK");
					return;
				}
				// Autenticação federada Firebase
				var firebaseResult = await FirebaseFederatedAuth.AuthenticateWithFirebaseAsync("google", idToken);
				if (!string.IsNullOrEmpty(firebaseResult.Error))
				{
					await DisplayAlert("Erro", firebaseResult.Error, "OK");
					return;
				}
				await Microsoft.Maui.Storage.SecureStorage.SetAsync("login_time", DateTime.UtcNow.Ticks.ToString());
				await Shell.Current.GoToAsync($"//BluetoothDevicesPage");
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

	// Facebook Login
	async void FacebookLogin_Clicked(object? sender, EventArgs e)
	{
		try
		{
			// Exemplo: use WebAuthenticator para Facebook OAuth
			var clientId = "SEU_FACEBOOK_APP_ID";
			var redirectUri = "https://www.facebook.com/connect/login_success.html";
			var authUrl = $"https://www.facebook.com/v10.0/dialog/oauth?client_id={clientId}&redirect_uri={HttpUtility.UrlEncode(redirectUri)}&response_type=token&scope=email,public_profile";
			var callbackUrl = redirectUri;

			var result = await WebAuthenticator.Default.AuthenticateAsync(new Uri(authUrl), new Uri(callbackUrl));
			if (result != null && result.Properties.ContainsKey("access_token"))
			{
				var accessToken = result.Properties["access_token"];
				var firebaseResult = await FirebaseFederatedAuth.AuthenticateWithFirebaseAsync("facebook", accessToken);
				if (!string.IsNullOrEmpty(firebaseResult.Error))
				{
					await DisplayAlert("Erro", firebaseResult.Error, "OK");
					return;
				}
				await Microsoft.Maui.Storage.SecureStorage.SetAsync("login_time", DateTime.UtcNow.Ticks.ToString());
				await Shell.Current.GoToAsync($"//BluetoothDevicesPage");
			}
			else
			{
				await DisplayAlert("Erro", "Login Facebook cancelado ou sem token.", "OK");
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Erro", ex.Message, "OK");
		}
	}

	// Apple Login
	async void AppleLogin_Clicked(object? sender, EventArgs e)
	{
		try
		{
			// O fluxo de login Apple requer configuração especial e uso de id_token
			// Aqui é um exemplo genérico, ajuste conforme seu fluxo
			var clientId = "SEU_APPLE_SERVICE_ID";
			var redirectUri = "https://SEU_DOMINIO/callback";
			var authUrl = $"https://appleid.apple.com/auth/authorize?client_id={clientId}&redirect_uri={HttpUtility.UrlEncode(redirectUri)}&response_type=code%20id_token&scope=name%20email&response_mode=form_post";
			var callbackUrl = redirectUri;

			var result = await WebAuthenticator.Default.AuthenticateAsync(new Uri(authUrl), new Uri(callbackUrl));
			if (result != null && result.Properties.ContainsKey("id_token"))
			{
				var idToken = result.Properties["id_token"];
				var firebaseResult = await FirebaseFederatedAuth.AuthenticateWithFirebaseAsync("apple", idToken);
				if (!string.IsNullOrEmpty(firebaseResult.Error))
				{
					await DisplayAlert("Erro", firebaseResult.Error, "OK");
					return;
				}
				await Microsoft.Maui.Storage.SecureStorage.SetAsync("login_time", DateTime.UtcNow.Ticks.ToString());
				await Shell.Current.GoToAsync($"//BluetoothDevicesPage");
			}
			else
			{
				await DisplayAlert("Erro", "Login Apple cancelado ou sem id_token.", "OK");
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Erro", ex.Message, "OK");
		}
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
				await Microsoft.Maui.Storage.SecureStorage.SetAsync("login_time", DateTime.UtcNow.Ticks.ToString());
				Application.Current.MainPage = new AppShell();
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
