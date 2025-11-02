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
    }

    public class UserModel
    {
    public string email { get; set; }
        public string Nome { get; set; }
        public string Senha { get; set; }
    }
}