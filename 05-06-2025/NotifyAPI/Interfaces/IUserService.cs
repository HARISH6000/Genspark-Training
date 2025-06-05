using NotifyAPI.Models;
using NotifyAPI.Models.DTOs;

namespace NotifyAPI.Interfaces
{
    public interface IUserService
    {
        public Task<User> AddUser(UserAddRequestDto user);
    }
}