using MyMvcApp.Models.DTOs;
using MyMvcApp.Models.Entities;

namespace MyMvcApp.Services.Contracts
{
    public interface IAuthService
    {
        Task<ApplicationUser> RegisterAsync(RegisterDto dto);
        Task<ApplicationUser> LoginAsync(LoginDto dto);
    }
}
