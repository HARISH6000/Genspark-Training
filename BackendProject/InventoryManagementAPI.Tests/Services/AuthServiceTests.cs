using Xunit;
using Moq;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Services;
using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Models;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt; 

namespace InventoryManagementAPI.Tests.Services
{
    public class AuthServiceTests
    {
        // Mock objects for dependencies
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly Mock<ITokenService> _mockTokenService;

        // The service to be tested
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Initialize mocks in the constructor
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockTokenService = new Mock<ITokenService>();

            // Instantiate the AuthService with the mocked dependencies
            _authService = new AuthService(
                _mockUserRepository.Object,
                _mockPasswordHasher.Object,
                _mockTokenService.Object
            );
        }

        #region Login Method Tests

        [Fact]
        public async Task Login_ValidCredentials_ReturnsLoginResponseDto()
        {
            // Arrange
            var userLoginDto = new UserLoginDto { Username = "validuser", Password = "ValidPassword123!" };
            var user = new User { UserId = 1, Username = "validuser", PasswordHash = "hashedpassword", IsDeleted = false, Role = new Role { RoleName = "User" } };
            var expectedToken = "mocked_jwt_token";

            // Setup mock behavior for dependencies
            _mockUserRepository.Setup(repo => repo.GetAll())
                               .ReturnsAsync(new List<User> { user }); 
            _mockPasswordHasher.Setup(hasher => hasher.VerifyPassword(userLoginDto.Password, user.PasswordHash))
                               .Returns(true); 
            _mockTokenService.Setup(token => token.GenerateJwtToken(It.IsAny<User>()))
                             .Returns(expectedToken);

            // Act
            var result = await _authService.Login(userLoginDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.UserId, result.UserId);
            Assert.Equal(user.Username, result.Username);
            Assert.Equal(expectedToken, result.Token);

            // Verify that the mocked methods were called as expected
            _mockUserRepository.Verify(repo => repo.GetAll(), Times.Once);
            _mockPasswordHasher.Verify(hasher => hasher.VerifyPassword(userLoginDto.Password, user.PasswordHash), Times.Once);
            _mockTokenService.Verify(token => token.GenerateJwtToken(It.Is<User>(u => u.UserId == user.UserId)), Times.Once);
        }

        [Fact]
        public async Task Login_InvalidUsername_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userLoginDto = new UserLoginDto { Username = "nonexistent", Password = "anypassword" };

            // Simulate no user found
            _mockUserRepository.Setup(repo => repo.GetAll())
                               .ReturnsAsync(new List<User>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.Login(userLoginDto));
            Assert.Equal("Invalid username or password.", exception.Message);

            // Verify that only UserRepository.GetAll was called
            _mockUserRepository.Verify(repo => repo.GetAll(), Times.Once);
            _mockPasswordHasher.Verify(hasher => hasher.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockTokenService.Verify(token => token.GenerateJwtToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Login_InvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userLoginDto = new UserLoginDto { Username = "validuser", Password = "WrongPassword!" };
            var user = new User { UserId = 1, Username = "validuser", PasswordHash = "hashedpassword", IsDeleted = false, Role = new Role { RoleName = "User" } };

            // Simulate user found, but password verification fails
            _mockUserRepository.Setup(repo => repo.GetAll())
                               .ReturnsAsync(new List<User> { user });
            _mockPasswordHasher.Setup(hasher => hasher.VerifyPassword(userLoginDto.Password, user.PasswordHash))
                               .Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.Login(userLoginDto));
            Assert.Equal("Invalid username or password.", exception.Message);

            // Verify calls
            _mockUserRepository.Verify(repo => repo.GetAll(), Times.Once);
            _mockPasswordHasher.Verify(hasher => hasher.VerifyPassword(userLoginDto.Password, user.PasswordHash), Times.Once);
            _mockTokenService.Verify(token => token.GenerateJwtToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Login_DeactivatedUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userLoginDto = new UserLoginDto { Username = "deactivateduser", Password = "DeactivatedPassword!" };
            var user = new User { UserId = 2, Username = "deactivateduser", PasswordHash = "hashedpassword", IsDeleted = true, Role = new Role { RoleName = "User" } };

            // Simulate user found, password verified, but user is deactivated
            _mockUserRepository.Setup(repo => repo.GetAll())
                               .ReturnsAsync(new List<User> { user });
            _mockPasswordHasher.Setup(hasher => hasher.VerifyPassword(userLoginDto.Password, user.PasswordHash))
                               .Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.Login(userLoginDto));
            Assert.Equal("User account is deactivated.", exception.Message);

            // Verify calls
            _mockUserRepository.Verify(repo => repo.GetAll(), Times.Once);
            _mockPasswordHasher.Verify(hasher => hasher.VerifyPassword(userLoginDto.Password, user.PasswordHash), Times.Once);
            _mockTokenService.Verify(token => token.GenerateJwtToken(It.IsAny<User>()), Times.Never);
        }

        #endregion

        #region RefreshToken Method Tests

        [Fact]
        public async Task RefreshToken_ValidPrincipal_ReturnsLoginResponseDto()
        {
            // Arrange
            var userId = 1;
            var username = "refreshuser";
            var existingClaims = new List<Claim>
            {
                new Claim("UserID", userId.ToString()),
                new Claim("Username", username),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds().ToString()) // Soon to expire
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(existingClaims, "TestAuth"));
            var user = new User { UserId = userId, Username = username, IsDeleted = false, Role = new Role { RoleName = "User" } };
            var newAccessToken = "new_mocked_jwt_token";

            // Setup mock behavior
            _mockUserRepository.Setup(repo => repo.Get(userId))
                               .ReturnsAsync(user);
            _mockTokenService.Setup(token => token.GenerateJwtToken(It.IsAny<IEnumerable<Claim>>()))
                             .Returns(newAccessToken);

            // Act
            var result = await _authService.RefreshToken(principal);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(username, result.Username);
            Assert.Equal(newAccessToken, result.Token);

            // Verify calls
            _mockUserRepository.Verify(repo => repo.Get(userId), Times.Once);
            _mockTokenService.Verify(token => token.GenerateJwtToken(It.IsAny<IEnumerable<Claim>>()), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_MissingUserIdClaim_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var existingClaims = new List<Claim>
            {
                new Claim("Username", "userwithoutid"),
                new Claim(ClaimTypes.Role, "User")
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(existingClaims, "TestAuth"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.RefreshToken(principal));
            Assert.Equal("Invalid user ID in token claims.", exception.Message);

            // Verify no further calls were made
            _mockUserRepository.Verify(repo => repo.Get(It.IsAny<int>()), Times.Never);
            _mockTokenService.Verify(token => token.GenerateJwtToken(It.IsAny<IEnumerable<Claim>>()), Times.Never);
        }

        [Fact]
        public async Task RefreshToken_InvalidUserIdClaim_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var existingClaims = new List<Claim>
            {
                new Claim("UserID", "notaninteger"),
                new Claim("Username", "userwithinvalidid"),
                new Claim(ClaimTypes.Role, "User")
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(existingClaims, "TestAuth"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.RefreshToken(principal));
            Assert.Equal("Invalid user ID in token claims.", exception.Message);

            // Verify no further calls were made
            _mockUserRepository.Verify(repo => repo.Get(It.IsAny<int>()), Times.Never);
            _mockTokenService.Verify(token => token.GenerateJwtToken(It.IsAny<IEnumerable<Claim>>()), Times.Never);
        }

        [Fact]
        public async Task RefreshToken_UserNotFound_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = 99; // Non-existent user ID
            var existingClaims = new List<Claim>
            {
                new Claim("UserID", userId.ToString()),
                new Claim("Username", "unknownuser"),
                new Claim(ClaimTypes.Role, "User")
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(existingClaims, "TestAuth"));

            // Simulate user not found in repository
            _mockUserRepository.Setup(repo => repo.Get(userId))
                               .ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.RefreshToken(principal));
            Assert.Equal("User associated with token not found or deactivated.", exception.Message);

            // Verify calls
            _mockUserRepository.Verify(repo => repo.Get(userId), Times.Once);
            _mockTokenService.Verify(token => token.GenerateJwtToken(It.IsAny<IEnumerable<Claim>>()), Times.Never);
        }

        [Fact]
        public async Task RefreshToken_DeactivatedUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = 2;
            var username = "deactivatedrefreshuser";
            var existingClaims = new List<Claim>
            {
                new Claim("UserID", userId.ToString()),
                new Claim("Username", username),
                new Claim(ClaimTypes.Role, "User")
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(existingClaims, "TestAuth"));
            var deactivatedUser = new User { UserId = userId, Username = username, IsDeleted = true, Role = new Role { RoleName = "User" } };

            // Simulate user found but is deactivated
            _mockUserRepository.Setup(repo => repo.Get(userId))
                               .ReturnsAsync(deactivatedUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.RefreshToken(principal));
            Assert.Equal("User associated with token not found or deactivated.", exception.Message);

            // Verify calls
            _mockUserRepository.Verify(repo => repo.Get(userId), Times.Once);
            _mockTokenService.Verify(token => token.GenerateJwtToken(It.IsAny<IEnumerable<Claim>>()), Times.Never);
        }

        #endregion
    }
}