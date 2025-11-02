using System.Threading.Tasks;

namespace FMIAutomation.Services
{
    public interface IAuthService
    {
    Task<bool> LoginAsync(string email, string senha);
    Task<string?> RegisterAsync(string nome, string email, string senha, string confirmarSenha);
    }
}