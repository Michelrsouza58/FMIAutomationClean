
using Newtonsoft.Json;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;

namespace FMIAutomation.Services
{
    public class AuthService : IAuthService
    {
        private readonly FirebaseClient _firebaseClient;

        public AuthService(string firebaseUrl)
        {
            _firebaseClient = new FirebaseClient(firebaseUrl);
        }

        public async Task<(string? Nome, string? Email)> GetUserInfoAsync(string email)
        {
            var emailLower = email.Trim().ToLowerInvariant();
            var users = await _firebaseClient
                .Child("users")
                .OrderBy("email")
                .EqualTo(emailLower)
                .OnceAsync<UserModel>();
            var user = users.FirstOrDefault();
            if (user == null)
                return (null, null);
            return (user.Object.Nome, user.Object.email);
        }

        public async Task<bool> LoginAsync(string email, string senha)
        {
            var emailLower = email?.Trim().ToLowerInvariant() ?? string.Empty;
            System.Diagnostics.Debug.WriteLine($"[LOGIN] Buscando email: {emailLower}");
            var users = await _firebaseClient
                .Child("users")
                .OrderBy("email")
                .EqualTo(emailLower)
                .OnceAsync<UserModel>();

            System.Diagnostics.Debug.WriteLine($"[LOGIN] Usuários encontrados: {users.Count}");
            var user = users.FirstOrDefault();
            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine("[LOGIN] Nenhum usuário encontrado.");
                return false;
            }

            var senhaSalva = user.Object.Senha?.Trim() ?? "";
            var senhaInformada = senha?.Trim() ?? "";
            System.Diagnostics.Debug.WriteLine($"[LOGIN] Senha salva: '{senhaSalva}', Senha informada: '{senhaInformada}'");

            // Permitir login Google mesmo se o usuário já existir
            if (senhaInformada == "GOOGLE")
            {
                if (senhaSalva == "GOOGLE")
                {
                    return true;
                }
                else
                {
                    // Vincular conta ao Google: atualizar senha para GOOGLE
                    user.Object.Senha = "GOOGLE";
                    await _firebaseClient.Child("users").Child(user.Key).PutAsync(user.Object);
                    return true;
                }
            }
            return senhaSalva == senhaInformada;
        }

        public async Task<string?> RegisterAsync(string nome, string email, string senha, string confirmarSenha)
        {
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(confirmarSenha))
                return "Preencha todos os campos.";
            if (senha != confirmarSenha)
                return "As senhas não coincidem.";

            var emailLower = email.Trim().ToLowerInvariant();
            // Verifica se já existe usuário
            var users = await _firebaseClient
                .Child("users")
                .OrderBy("email")
                .EqualTo(emailLower)
                .OnceAsync<UserModel>();
            if (users.Any())
                return "E-mail já cadastrado.";

            var user = new UserModel { Nome = nome, email = emailLower, Senha = senha };
            await _firebaseClient.Child("users").PostAsync(user);
            return null; // null = sucesso
        }

        public async Task<string?> UpdateProfileAsync(string oldEmail, string newNome, string newEmail)
        {
            var emailLower = oldEmail.Trim().ToLowerInvariant();
            var users = await _firebaseClient
                .Child("users")
                .OrderBy("email")
                .EqualTo(emailLower)
                .OnceAsync<UserModel>();
            var user = users.FirstOrDefault();
            if (user == null)
                return "Usuário não encontrado.";
            // Atualiza nome e email
            var updated = user.Object;
            updated.Nome = newNome;
            updated.email = newEmail.Trim().ToLowerInvariant();
            await _firebaseClient.Child("users").Child(user.Key).PutAsync(updated);
            return null;
        }

        public async Task<string?> ChangePasswordAsync(string email, string oldPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
                return "Preencha todos os campos.";
            if (newPassword != confirmPassword)
                return "As senhas não coincidem.";
            var emailLower = email.Trim().ToLowerInvariant();
            var users = await _firebaseClient
                .Child("users")
                .OrderBy("email")
                .EqualTo(emailLower)
                .OnceAsync<UserModel>();
            var user = users.FirstOrDefault();
            if (user == null)
                return "Usuário não encontrado.";
            if ((user.Object.Senha ?? "") != (oldPassword ?? ""))
                return "Senha atual incorreta.";
            user.Object.Senha = newPassword;
            await _firebaseClient.Child("users").Child(user.Key).PutAsync(user.Object);
            return null;
        }

        public async Task<bool> DeleteAccountAsync(string email, string senha)
        {
            var emailLower = email.Trim().ToLowerInvariant();
            var users = await _firebaseClient
                .Child("users")
                .OrderBy("email")
                .EqualTo(emailLower)
                .OnceAsync<UserModel>();
            var user = users.FirstOrDefault();
            if (user == null)
                return false;
            if ((user.Object.Senha ?? "") != (senha ?? ""))
                return false;
            await _firebaseClient.Child("users").Child(user.Key).DeleteAsync();
            return true;
        }
    }

    public class UserModel
    {
        public string email { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }
}