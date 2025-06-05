using NotifyAPI.Interfaces;
using NotifyAPI.Models;
using NotifyAPI.Models.DTOs;

namespace NotifyAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<string, User> _userRepository;
        private readonly IEncryptionService _encryptionService;

        public UserService(IRepository<string, User> userRepository, IEncryptionService encryptionService)
        {
            _userRepository = userRepository;
            _encryptionService = encryptionService;
        }

        public async Task<User> AddUser(UserAddRequestDto userDto)
        {
            // Encrypt the password using the EncryptionService
            var encryptModel = new EncryptModel { Data = userDto.Password };
            encryptModel = await _encryptionService.EncryptData(encryptModel);

            var newUser = new User
            {
                Username = userDto.Username,
                Role = userDto.Role,
                Password = encryptModel.EncryptedData,
                HashKey = encryptModel.HashKey
            };

            return await _userRepository.Add(newUser);
        }
    }
}
