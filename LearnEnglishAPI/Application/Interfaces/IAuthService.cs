// IngilizceProje/Application/Interfaces/IAuthService.cs
using System.Threading.Tasks;
using IngilizceProje.Application.DTOs;

namespace IngilizceProje.Application.Interfaces
{
    public interface IAuthService
    {
        Task<TokenDto?> LoginAsync(LoginDto loginDto);
        Task<(bool Success, string Message)> RegisterAsync(RegisterDto registerDto);
    }
}
