using BankingApplication.Models;
using BankingApplication.DTOs;
namespace BankingApplication.Interfaces
{
    public interface IUserService
    {
        public Task<User> AddUser(CreateUserRequest user);
    }
}