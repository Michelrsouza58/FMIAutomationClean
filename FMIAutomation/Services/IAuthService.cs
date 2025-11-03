using System.Threading.Tasks;

namespace FMIAutomation.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string senha);
        Task<string?> RegisterAsync(string nome, string email, string senha, string confirmarSenha);
    Task<string?> UpdateProfileAsync(string oldEmail, string newNome, string newEmail);
    Task<string?> ChangePasswordAsync(string email, string oldPassword, string newPassword, string confirmPassword);
    Task<bool> DeleteAccountAsync(string email, string senha);
    Task<(string? Nome, string? Email)> GetUserInfoAsync(string email);
    }
}