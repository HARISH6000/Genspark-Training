using BankingApplication.Interfaces;
using BankingApplication.Models;
using BankingApplication.DTOs;
using System.Threading.Tasks;

namespace BankingApplication.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<int, User> _userRepository;

        
        public UserService(IRepository<int, User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> AddUser(CreateUserRequest userRequest)
        {

            var newUser = new User
            {
                Username = userRequest.Username,
                PasswordHash = userRequest.Password,
                Name = userRequest.Name,
                Email = userRequest.Email,
                PhoneNumber = userRequest.PhoneNumber
            };

            var addedUser = await _userRepository.Add(newUser);

            return addedUser;
        }
    }
}