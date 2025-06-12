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
        private readonly Mock<ITokenBlacklistService> _mockTokenBlacklistService;
        private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;

        // The service to be tested
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Initialize mocks in the constructor
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockTokenService = new Mock<ITokenService>();
            _mockTokenBlacklistService = new Mock<ITokenBlacklistService>();
            _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();

            // Instantiate the AuthService with the mocked dependencies
            _authService = new AuthService(
                _mockUserRepository.Object,
                _mockPasswordHasher.Object,
                _mockTokenService.Object,
                _mockTokenBlacklistService.Object,
                _mockRefreshTokenRepository.Object
            );
        }

        #region Login Method Tests

        [Fact]
        public async Task Login_ValidCredentials_ReturnsLoginResponseDto()
        {
            // Arrange
            var userLoginDto = new UserLoginDto { Username = "validuser", Password = "ValidPassword123!" };
            var user = new User { UserId = 1, Username = "validuser", PasswordHash = "hashedpassword", IsDeleted = false, Role = new Role { RoleName = "User" } };
            var expectedAccessToken = "mocked_access_token";
            var mockedRefreshTokenString = "mocked_refresh_token_string";
            var hashedRefreshToken = "hashed_mocked_refresh_token_string";
            var refreshTokenJti = Guid.NewGuid().ToString();
            var refreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(7);
            var refreshTokenPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, refreshTokenJti),
                new Claim(JwtRegisteredClaimNames.Exp, refreshTokenExpiry.ToUnixTimeSeconds().ToString()),
                new Claim("UserID", user.UserId.ToString()),
                new Claim("TokenType", "RefreshToken")
            }, "RefreshTokenAuth"));


            // Setup mock behavior for dependencies
            _mockUserRepository.Setup(repo => repo.GetAll())
                               .ReturnsAsync(new List<User> { user });
            _mockPasswordHasher.Setup(hasher => hasher.VerifyPassword(userLoginDto.Password, user.PasswordHash))
                               .Returns(true);
            _mockTokenService.Setup(token => token.GenerateJwtToken(It.IsAny<User>()))
                             .Returns(expectedAccessToken);
            _mockTokenService.Setup(token => token.GenerateRefreshToken(It.IsAny<User>()))
                             .Returns(mockedRefreshTokenString);
            _mockTokenService.Setup(token => token.GetPrincipalFromToken(mockedRefreshTokenString))
                             .Returns(refreshTokenPrincipal);
            _mockPasswordHasher.Setup(hasher => hasher.HashPassword(mockedRefreshTokenString))
                               .Returns(hashedRefreshToken);
            _mockRefreshTokenRepository.Setup(repo => repo.Add(It.IsAny<RefreshToken>()))
                                       .ReturnsAsync((RefreshToken rt) => rt);

            // Act
            var result = await _authService.Login(userLoginDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.UserId, result.UserId);
            Assert.Equal(user.Username, result.Username);
            Assert.Equal(expectedAccessToken, result.Token);
            Assert.Equal(mockedRefreshTokenString, result.RefreshToken);

            // Verify that the mocked methods were called as expected
            _mockUserRepository.Verify(repo => repo.GetAll(), Times.Once);
            _mockPasswordHasher.Verify(hasher => hasher.VerifyPassword(userLoginDto.Password, user.PasswordHash), Times.Once);
            _mockTokenService.Verify(token => token.GenerateJwtToken(It.Is<User>(u => u.UserId == user.UserId)), Times.Once);
            _mockTokenService.Verify(token => token.GenerateRefreshToken(It.Is<User>(u => u.UserId == user.UserId)), Times.Once);
            _mockTokenService.Verify(token => token.GetPrincipalFromToken(mockedRefreshTokenString), Times.Once);
            _mockPasswordHasher.Verify(hasher => hasher.HashPassword(mockedRefreshTokenString), Times.Once);
            _mockRefreshTokenRepository.Verify(repo => repo.Add(It.Is<RefreshToken>(rt =>
                rt.TokenHash == hashedRefreshToken &&
                rt.UserId == user.UserId &&
                rt.Jti == refreshTokenJti &&
                !rt.IsRevoked
            )), Times.Once);
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
            _mockTokenService.Verify(token => token.GenerateRefreshToken(It.IsAny<User>()), Times.Never);
            _mockRefreshTokenRepository.Verify(repo => repo.Add(It.IsAny<RefreshToken>()), Times.Never);
        }

        #endregion

        #region RefreshToken Method Tests

        [Fact]
        public async Task RefreshToken_ValidRefreshToken_ReturnsLoginResponseDto()
        {
            // Arrange
            var userId = 1;
            var username = "refreshuser";
            var originalRefreshTokenString = "valid_refresh_token_string";
            var originalRefreshTokenHash = "hashed_valid_refresh_token_string";
            var originalRefreshTokenJti = Guid.NewGuid().ToString();
            var originalRefreshTokenExpiry = DateTime.UtcNow.AddDays(7); // Not expired

            var user = new User { UserId = userId, Username = username, IsDeleted = false, Role = new Role { RoleName = "User" } };
            var storedRefreshToken = new RefreshToken
            {
                Id = 1,
                TokenHash = originalRefreshTokenHash,
                UserId = userId,
                ExpiryDate = originalRefreshTokenExpiry,
                IsRevoked = false,
                Jti = originalRefreshTokenJti
            };

            var originalPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim("UserID", userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, originalRefreshTokenJti),
                new Claim("TokenType", "RefreshToken"),
                new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds().ToString())
            }, "RefreshTokenAuth"));

            var newAccessToken = "new_mocked_access_token";
            var newRefreshTokenString = "new_mocked_refresh_token_string";
            var newHashedRefreshToken = "hashed_new_mocked_refresh_token_string";
            var newRefreshTokenJti = Guid.NewGuid().ToString();
            var newRefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(7);
            var newPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim("UserID", userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, newRefreshTokenJti),
                new Claim("TokenType", "RefreshToken"),
                new Claim(JwtRegisteredClaimNames.Exp, newRefreshTokenExpiry.ToUnixTimeSeconds().ToString())
            }, "RefreshTokenAuth"));


            _mockTokenService.Setup(token => token.GetPrincipalFromToken(originalRefreshTokenString))
                             .Returns(originalPrincipal);
            _mockUserRepository.Setup(repo => repo.Get(userId))
                               .ReturnsAsync(user);
            _mockRefreshTokenRepository.Setup(repo => repo.GetUserRefreshTokens(userId))
                                       .ReturnsAsync(new List<RefreshToken> { storedRefreshToken });
            _mockPasswordHasher.Setup(hasher => hasher.VerifyPassword(originalRefreshTokenString, originalRefreshTokenHash))
                               .Returns(true);
            _mockTokenService.Setup(token => token.GenerateJwtToken(It.IsAny<User>()))
                             .Returns(newAccessToken);
            _mockRefreshTokenRepository.Setup(repo => repo.Update(storedRefreshToken.Id, It.IsAny<RefreshToken>()))
                                       .ReturnsAsync((int id, RefreshToken rt) => rt);
            _mockTokenService.Setup(token => token.GenerateRefreshToken(It.IsAny<User>()))
                             .Returns(newRefreshTokenString);
            _mockTokenService.Setup(token => token.GetPrincipalFromToken(newRefreshTokenString))
                             .Returns(newPrincipal);
            _mockPasswordHasher.Setup(hasher => hasher.HashPassword(newRefreshTokenString))
                               .Returns(newHashedRefreshToken);
            _mockRefreshTokenRepository.Setup(repo => repo.Add(It.IsAny<RefreshToken>()))
                                       .ReturnsAsync((RefreshToken rt) => rt);

            // Act
            var result = await _authService.RefreshToken(originalRefreshTokenString);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(username, result.Username);
            Assert.Equal(newAccessToken, result.Token);
            Assert.Equal(newRefreshTokenString, result.RefreshToken);

            // Verify calls
            _mockTokenService.Verify(token => token.GetPrincipalFromToken(originalRefreshTokenString), Times.Once);
            _mockUserRepository.Verify(repo => repo.Get(userId), Times.Once);
            _mockRefreshTokenRepository.Verify(repo => repo.GetUserRefreshTokens(userId), Times.Once);
            _mockPasswordHasher.Verify(hasher => hasher.VerifyPassword(originalRefreshTokenString, originalRefreshTokenHash), Times.Once);
            _mockTokenService.Verify(token => token.GenerateJwtToken(It.Is<User>(u => u.UserId == userId)), Times.Once);
            _mockRefreshTokenRepository.Verify(repo => repo.Update(storedRefreshToken.Id, It.Is<RefreshToken>(rt => rt.IsRevoked == true)), Times.Once);
            _mockTokenService.Verify(token => token.GenerateRefreshToken(It.Is<User>(u => u.UserId == userId)), Times.Once);
            _mockTokenService.Verify(token => token.GetPrincipalFromToken(newRefreshTokenString), Times.Once);
            _mockPasswordHasher.Verify(hasher => hasher.HashPassword(newRefreshTokenString), Times.Once);
            _mockRefreshTokenRepository.Verify(repo => repo.Add(It.Is<RefreshToken>(rt =>
                rt.TokenHash == newHashedRefreshToken &&
                rt.UserId == userId &&
                rt.Jti == newRefreshTokenJti &&
                !rt.IsRevoked
            )), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_ExpiredRefreshToken_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = 1;
            var username = "user";
            var expiredRefreshTokenString = "expired_refresh_token_string";
            var expiredRefreshTokenHash = "hashed_expired_refresh_token_string";
            var expiredRefreshTokenJti = Guid.NewGuid().ToString();
            var expiredRefreshTokenExpiry = DateTime.UtcNow.AddMinutes(-10); // Expired

            var user = new User { UserId = userId, Username = username, IsDeleted = false, Role = new Role { RoleName = "User" } };
            var storedRefreshToken = new RefreshToken
            {
                Id = 1,
                TokenHash = expiredRefreshTokenHash,
                UserId = userId,
                ExpiryDate = expiredRefreshTokenExpiry,
                IsRevoked = false,
                Jti = expiredRefreshTokenJti
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim("UserID", userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, expiredRefreshTokenJti),
                new Claim("TokenType", "RefreshToken"),
                new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeSeconds().ToString())
            }, "RefreshTokenAuth"));


            _mockTokenService.Setup(token => token.GetPrincipalFromToken(expiredRefreshTokenString))
                             .Returns(principal);
            _mockUserRepository.Setup(repo => repo.Get(userId))
                               .ReturnsAsync(user);
            _mockRefreshTokenRepository.Setup(repo => repo.GetUserRefreshTokens(userId))
                                       .ReturnsAsync(new List<RefreshToken> { storedRefreshToken });
            _mockPasswordHasher.Setup(hasher => hasher.VerifyPassword(expiredRefreshTokenString, expiredRefreshTokenHash))
                               .Returns(true);
            _mockRefreshTokenRepository.Setup(repo => repo.Delete(storedRefreshToken.Id))
                                       .ReturnsAsync(storedRefreshToken); // Simulate deletion

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.RefreshToken(expiredRefreshTokenString));
            Assert.Equal("Refresh token has expired.", exception.Message);

            // Verify calls
            _mockTokenService.Verify(token => token.GetPrincipalFromToken(expiredRefreshTokenString), Times.Once);
            _mockUserRepository.Verify(repo => repo.Get(userId), Times.Once);
            _mockRefreshTokenRepository.Verify(repo => repo.GetUserRefreshTokens(userId), Times.Once);
            _mockPasswordHasher.Verify(hasher => hasher.VerifyPassword(expiredRefreshTokenString, expiredRefreshTokenHash), Times.Once);
            _mockRefreshTokenRepository.Verify(repo => repo.Delete(storedRefreshToken.Id), Times.Once);
            _mockTokenService.Verify(token => token.GenerateJwtToken(It.IsAny<User>()), Times.Never);
            _mockRefreshTokenRepository.Verify(repo => repo.Update(It.IsAny<int>(), It.IsAny<RefreshToken>()), Times.Never);
            _mockTokenService.Verify(token => token.GenerateRefreshToken(It.IsAny<User>()), Times.Never);
            _mockRefreshTokenRepository.Verify(repo => repo.Add(It.IsAny<RefreshToken>()), Times.Never);
        }

        #endregion

        #region Logout Method Tests

        [Fact]
        public async Task Logout_ValidTokens_SuccessfullyLogsOutUser()
        {
            // Arrange
            var userId = 1;
            var userIdString = userId.ToString();
            var accessTokenString = "valid_access_token";
            var refreshTokenString = "valid_refresh_token";
            var accessTokenJti = Guid.NewGuid().ToString();
            var accessTokenExp = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds().ToString();
            var accessTokenPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, accessTokenJti),
                new Claim(JwtRegisteredClaimNames.Exp, accessTokenExp)
            }, "AccessTokenAuth"));

            var user = new User { UserId = userId, Username = "testuser", IsDeleted = false };
            // FIX: Provide a pre-hashed string for TokenHash instead of calling _passwordHasher directly.
            var storedRefreshToken = new RefreshToken
            {
                Id = 101,
                TokenHash = "pre_hashed_valid_refresh_token", // Corrected line
                UserId = userId,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _mockTokenService.Setup(token => token.GetPrincipalFromToken(accessTokenString))
                             .Returns(accessTokenPrincipal);
            _mockTokenBlacklistService.Setup(service => service.AddTokenToBlacklist(accessTokenJti, It.IsAny<DateTime>()))
                                      .Returns(Task.CompletedTask);
            _mockUserRepository.Setup(repo => repo.Get(userId))
                               .ReturnsAsync(user);
            _mockRefreshTokenRepository.Setup(repo => repo.GetUserRefreshTokens(userId))
                                       .ReturnsAsync(new List<RefreshToken> { storedRefreshToken });
            // Setup VerifyPassword to return true for the given refresh token string and its pre-hashed version
            _mockPasswordHasher.Setup(hasher => hasher.VerifyPassword(refreshTokenString, "pre_hashed_valid_refresh_token"))
                               .Returns(true);
            _mockRefreshTokenRepository.Setup(repo => repo.Update(storedRefreshToken.Id, It.IsAny<RefreshToken>()))
                                       .ReturnsAsync(storedRefreshToken);

            // Act
            await _authService.Logout(userIdString, accessTokenString, refreshTokenString);

            // Assert
            _mockTokenService.Verify(token => token.GetPrincipalFromToken(accessTokenString), Times.Once);
            _mockTokenBlacklistService.Verify(service => service.AddTokenToBlacklist(accessTokenJti, It.IsAny<DateTime>()), Times.Once);
            _mockUserRepository.Verify(repo => repo.Get(userId), Times.Once);
            _mockRefreshTokenRepository.Verify(repo => repo.GetUserRefreshTokens(userId), Times.Once);
            _mockPasswordHasher.Verify(hasher => hasher.VerifyPassword(refreshTokenString, storedRefreshToken.TokenHash), Times.Once);
            _mockRefreshTokenRepository.Verify(repo => repo.Update(storedRefreshToken.Id, It.Is<RefreshToken>(rt => rt.IsRevoked == true)), Times.Once);
        }

        [Fact]
        public async Task Logout_InvalidUserIdFormat_ThrowsArgumentException()
        {
            // Arrange
            var invalidUserIdString = "abc";
            var accessTokenString = "mock_access_token";
            var refreshTokenString = "mock_refresh_token";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _authService.Logout(invalidUserIdString, accessTokenString, refreshTokenString));
            Assert.Equal("Invalid user ID format.", exception.Message);

            // Verify no other mocks were called
            _mockTokenService.VerifyNoOtherCalls();
            _mockTokenBlacklistService.VerifyNoOtherCalls();
            _mockUserRepository.VerifyNoOtherCalls();
            _mockRefreshTokenRepository.VerifyNoOtherCalls();
            _mockPasswordHasher.VerifyNoOtherCalls();
        }
        #endregion
    }
}
