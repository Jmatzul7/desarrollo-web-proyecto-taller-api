using TallerMecanicoAPI.DTOs;

namespace TallerMecanicoAPI.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDTO?> Login(LoginDTO dto);
    }
}
