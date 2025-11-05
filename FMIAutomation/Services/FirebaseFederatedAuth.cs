// Classe utilitária para autenticação federada Firebase (Google, Facebook, Apple)
// Uso: chame AuthenticateWithFirebaseAsync("google", idToken) ou ("facebook", accessToken) ou ("apple", idToken)
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace FMIAutomation.Services
{
    public static class FirebaseFederatedAuth
    {
    // API KEY do Firebase fornecida pelo usuário
    private const string FirebaseApiKey = "AIzaSyABxQ7cHCynIzs82cF8oxHe6mMM1Gy3ysE";

        public static async Task<(string? FirebaseIdToken, string? Email, string? Name, string? Error)> AuthenticateWithFirebaseAsync(string provider, string idTokenOrAccessToken)
        {
            string? providerId = provider.ToLower() switch
            {
                "google" => "google.com",
                "facebook" => "facebook.com",
                "apple" => "apple.com",
                _ => null
            };
            if (providerId == null)
                return (null, null, null, "Provedor não suportado");

            var payload = new
            {
                postBody = providerId == "facebook.com"
                    ? $"access_token={idTokenOrAccessToken}&providerId={providerId}"
                    : $"id_token={idTokenOrAccessToken}&providerId={providerId}",
                requestUri = "http://localhost",
                returnIdpCredential = true,
                returnSecureToken = true
            };
            using var http = new HttpClient();
            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var response = await http.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp?key={FirebaseApiKey}", content);
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return (null, null, null, $"Erro Firebase: {json}");
            var doc = JsonDocument.Parse(json).RootElement;
            var firebaseIdToken = doc.GetProperty("idToken").GetString();
            var email = doc.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
            var name = doc.TryGetProperty("displayName", out var nameProp) ? nameProp.GetString() : null;
            return (firebaseIdToken, email, name, null);
        }
    }
}
