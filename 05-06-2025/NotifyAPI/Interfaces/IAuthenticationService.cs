
using NotifyAPI.Models.DTOs;

namespace NotifyAPI.Interfaces
{
    public interface IAuthenticationService
    {
        public Task<UserLoginResponse> Login(UserLoginRequest user);
    }
}