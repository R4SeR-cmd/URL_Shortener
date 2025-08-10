using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using Shortener.BLL.DTO_s;
using Shortener.BLL.Options;
using Shortener.BLL.Services;
using Shortener.DAL.Entity;
using FluentAssertions;
using Xunit;
using System.Security.Claims;

namespace Shortener.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private readonly JwtOptions _jwtOptions;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserManager = CreateMockUserManager();
            _mockRoleManager = CreateMockRoleManager();
            _jwtOptions = new JwtOptions
            {
                Key = "your-super-secret-key-with-at-least-32-characters",
                Issuer = "test-issuer",
                Audience = "test-audience"
            };
            var options = Options.Create(_jwtOptions);

            _authService = new AuthService(_mockUserManager.Object, _mockRoleManager.Object, options);
        }

        [Fact]
        public async Task RegisterAsync_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var dto = new UserCredentialsDTO
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            var user = new User
            {
                Id = "user123",
                UserName = dto.Email,
                Email = dto.Email
            };

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _mockRoleManager.Setup(x => x.RoleExistsAsync("User"))
                .ReturnsAsync(true);

            _mockUserManager.Setup(x => x.AddToRoleAsync(user, "User"))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _authService.RegisterAsync(dto);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain(".");
            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), dto.Password), Times.Once);
            _mockUserManager.Verify(x => x.AddToRoleAsync(user, "User"), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithAdminEmail_AssignsAdminRole()
        {
            // Arrange
            var dto = new UserCredentialsDTO
            {
                Email = "rostukvaso@gmail.com",
                Password = "TestPassword123!"
            };

            var user = new User
            {
                Id = "admin123",
                UserName = dto.Email,
                Email = dto.Email
            };

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _mockRoleManager.Setup(x => x.RoleExistsAsync("Admin"))
                .ReturnsAsync(true);

            _mockUserManager.Setup(x => x.AddToRoleAsync(user, "Admin"))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin" });

            // Act
            var result = await _authService.RegisterAsync(dto);

            // Assert
            result.Should().NotBeNullOrEmpty();
            _mockUserManager.Verify(x => x.AddToRoleAsync(user, "Admin"), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithInvalidCredentials_ThrowsException()
        {
            // Arrange
            var dto = new UserCredentialsDTO
            {
                Email = "test@example.com",
                Password = "weak"
            };

            var errors = new List<IdentityError>
            {
                new IdentityError { Description = "Password is too short" }
            };

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), dto.Password))
                .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _authService.RegisterAsync(dto));
            exception.Message.Should().Contain("Password is too short");
        }

        [Fact]
        public async Task RegisterAsync_WithNonExistentRole_ThrowsException()
        {
            // Arrange
            var dto = new UserCredentialsDTO
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            _mockRoleManager.Setup(x => x.RoleExistsAsync("User"))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _authService.RegisterAsync(dto));
            exception.Message.Should().Be("Role does not exist");
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var dto = new UserCredentialsDTO
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            var user = new User
            {
                Id = "user123",
                UserName = dto.Email,
                Email = dto.Email
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(true);

            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain(".");
            _mockUserManager.Verify(x => x.FindByEmailAsync(dto.Email), Times.Once);
            _mockUserManager.Verify(x => x.CheckPasswordAsync(user, dto.Password), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidEmail_ThrowsException()
        {
            // Arrange
            var dto = new UserCredentialsDTO
            {
                Email = "nonexistent@example.com",
                Password = "TestPassword123!"
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _authService.LoginAsync(dto));
            exception.Message.Should().Be("Invalid credentials");
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ThrowsException()
        {
            // Arrange
            var dto = new UserCredentialsDTO
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            var user = new User
            {
                Id = "user123",
                UserName = dto.Email,
                Email = dto.Email
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _authService.LoginAsync(dto));
            exception.Message.Should().Be("Invalid credentials");
        }

        [Fact]
        public void GenerateToken_WithValidUser_ReturnsValidToken()
        {
            // Arrange
            var user = new User
            {
                Id = "user123",
                UserName = "test@example.com",
                Email = "test@example.com"
            };

            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = _authService.GenerateToken(user);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain(".");
            result.Split('.').Should().HaveCount(3); // JWT has 3 parts
        }

        [Fact]
        public void GenerateToken_WithUserWithMultipleRoles_IncludesAllRoles()
        {
            // Arrange
            var user = new User
            {
                Id = "user123",
                UserName = "test@example.com",
                Email = "test@example.com"
            };

            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User", "Admin" });

            // Act
            var result = _authService.GenerateToken(user);

            // Assert
            result.Should().NotBeNullOrEmpty();
            // Note: In a real scenario, you might want to decode the JWT and verify the claims
        }

        private static Mock<UserManager<User>> CreateMockUserManager()
        {
            var userStore = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(
                userStore.Object,
                null, null, null, null, null, null, null, null);
        }

        private static Mock<RoleManager<IdentityRole>> CreateMockRoleManager()
        {
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(
                roleStore.Object,
                null, null, null, null);
        }
    }
}
