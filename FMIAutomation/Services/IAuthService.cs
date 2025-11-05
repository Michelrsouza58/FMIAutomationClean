using System.Threading.Tasks;

namespace FMIAutomation.Services
{
    public class UserInfo
    {
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? ProfileImageBase64 { get; set; }
    }

    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string senha);
        Task<string?> RegisterAsync(string nome, string email, string senha, string confirmarSenha);
        Task<string?> RegisterAsync(string nome, string email, string senha, string confirmarSenha, string? telefone);
        Task<string?> UpdateProfileAsync(string oldEmail, string newNome, string newEmail);
        Task<string?> UpdateProfileWithImageAsync(string oldEmail, string newNome, string newEmail, string? telefone, string? profileImageBase64);
        Task<string?> ChangePasswordAsync(string email, string oldPassword, string newPassword, string confirmPassword);
        Task<bool> DeleteAccountAsync(string email, string senha);
        Task<UserInfo?> GetUserInfoAsync(string email);
    }
}