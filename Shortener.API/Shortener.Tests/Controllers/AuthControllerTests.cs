using Microsoft.AspNetCore.Mvc;
using Moq;
using Shortener.API.Controllers;
using Shortener.BLL.DTO_s;
using Shortener.BLL.Services.Interfaces;
using FluentAssertions;
using Xunit;

namespace Shortener.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);
        }

        [Fact]
        public async Task Register_WithValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var dto = new UserCredentialsDTO
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };
            var expectedToken = "jwt_token_here";
            _mockAuthService.Setup(x => x.RegisterAsync(dto))
                .ReturnsAsync(expectedToken);

            // Act
            var result = await _controller.Register(dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(new { token = expectedToken });
            _mockAuthService.Verify(x => x.RegisterAsync(dto), Times.Once);
        }

        [Fact]
        public async Task Register_WhenServiceThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var dto = new UserCredentialsDTO
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };
            _mockAuthService.Setup(x => x.RegisterAsync(dto))
                .ThrowsAsync(new Exception("Registration failed"));

            // Act
            var result = await _controller.Register(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Registration failed");
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var dto = new UserCredentialsDTO
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };
            var expectedToken = "jwt_token_here";
            _mockAuthService.Setup(x => x.LoginAsync(dto))
                .ReturnsAsync(expectedToken);

            // Act
            var result = await _controller.Login(dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(new { token = expectedToken });
            _mockAuthService.Verify(x => x.LoginAsync(dto), Times.Once);
        }

        [Fact]
        public async Task Login_WhenServiceThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var dto = new UserCredentialsDTO
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };
            _mockAuthService.Setup(x => x.LoginAsync(dto))
                .ThrowsAsync(new Exception("Invalid credentials"));

            // Act
            var result = await _controller.Login(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Invalid credentials");
        }

        [Fact]
        public async Task Register_WithNullDto_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Register(null!);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task Login_WithNullDto_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Login(null!);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }
    }
}
