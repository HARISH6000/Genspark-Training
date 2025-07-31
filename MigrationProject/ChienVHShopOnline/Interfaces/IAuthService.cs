using ChienVHShopOnline.DTOs;
using ChienVHShopOnline.Models;

public interface IAuthService
{
    Task<bool> RegisterAsync(UserRegisterDto dto);
    Task<AuthResponseDto?> LoginAsync(UserLoginDto dto);
}
