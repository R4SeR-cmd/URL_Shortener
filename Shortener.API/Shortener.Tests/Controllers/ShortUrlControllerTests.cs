using Microsoft.AspNetCore.Mvc;
using Moq;
using Shortener.API.Controllers;
using Shortener.BLL.DTO_s;
using Shortener.BLL.Services.Interfaces;
using Shortener.DAL.Entity;
using System.Security.Claims;
using FluentAssertions;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace Shortener.Tests.Controllers
{
    public class ShortUrlControllerTests
    {
        private readonly Mock<IShortUrlService> _mockShortUrlService;
        private readonly ShortUrlController _controller;

        public ShortUrlControllerTests()
        {
            _mockShortUrlService = new Mock<IShortUrlService>();
            _controller = new ShortUrlController(_mockShortUrlService.Object);
        }

        [Fact]
        public async Task RedirectToOriginal_WithValidShortCode_ReturnsRedirectResult()
        {
            // Arrange
            var shortCode = "abc123";
            var shortUrl = new ShortUrl
            {
                Id = Guid.NewGuid(),
                ShortCode = shortCode,
                OriginalUrl = "https://example.com",
                VisitCount = 0
            };

            _mockShortUrlService.Setup(x => x.GetByShortCodeAsync(shortCode))
                .ReturnsAsync(shortUrl);

            // Act
            var result = await _controller.RedirectToOriginal(shortCode);

            // Assert
            result.Should().BeOfType<RedirectResult>();
            var redirectResult = result as RedirectResult;
            redirectResult!.Url.Should().Be("https://example.com");
            _mockShortUrlService.Verify(x => x.UpdateAsync(It.IsAny<ShortUrl>()), Times.Once);
        }

        [Fact]
        public async Task RedirectToOriginal_WithInvalidShortCode_ReturnsNotFound()
        {
            // Arrange
            var shortCode = "invalid";
            _mockShortUrlService.Setup(x => x.GetByShortCodeAsync(shortCode))
                .ReturnsAsync((ShortUrl?)null);

            // Act
            var result = await _controller.RedirectToOriginal(shortCode);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetAll_WithValidUser_ReturnsOkResult()
        {
            // Arrange
            var userId = "user123";
            var shortUrls = new List<ShortUrl>
            {
                new ShortUrl { Id = Guid.NewGuid(), ShortCode = "abc123", OriginalUrl = "https://example.com" }
            };

            SetupUserClaims(userId, new[] { "User" });

            _mockShortUrlService.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(shortUrls);

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(shortUrls);
        }

        [Fact]
        public async Task GetAll_WithAdminUser_ReturnsAllUrls()
        {
            // Arrange
            var userId = "admin123";
            var allUrls = new List<ShortUrl>
            {
                new ShortUrl { Id = Guid.NewGuid(), ShortCode = "abc123", OriginalUrl = "https://example.com" },
                new ShortUrl { Id = Guid.NewGuid(), ShortCode = "def456", OriginalUrl = "https://test.com" }
            };

            SetupUserClaims(userId, new[] { "Admin" });

            _mockShortUrlService.Setup(x => x.GetAllAsync())
                .ReturnsAsync(allUrls);

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(allUrls);
        }

        [Fact]
        public async Task GetAll_WithoutUserId_ReturnsUnauthorized()
        {
            // Arrange
            SetupUserClaims(null, new string[0]);

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task GetById_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var shortUrl = new ShortUrl
            {
                Id = id,
                ShortCode = "abc123",
                OriginalUrl = "https://example.com"
            };

            _mockShortUrlService.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(shortUrl);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(shortUrl);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockShortUrlService.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync((ShortUrl?)null);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            var dto = new CreateShortUrlDto { OriginalUrl = "https://example.com" };
            var userId = "user123";
            var createdUrl = new ShortUrl
            {
                Id = Guid.NewGuid(),
                ShortCode = "abc123",
                OriginalUrl = "https://example.com",
                CreatedByUserId = userId
            };

            SetupUserClaims(userId, new[] { "User" });

            _mockShortUrlService.Setup(x => x.CreateShortUrlAsync(dto.OriginalUrl, userId))
                .ReturnsAsync(createdUrl);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.Value.Should().BeEquivalentTo(createdUrl);
        }

        [Fact]
        public async Task Create_WithExistingUrl_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateShortUrlDto { OriginalUrl = "https://example.com" };
            var userId = "user123";

            SetupUserClaims(userId, new[] { "User" });

            _mockShortUrlService.Setup(x => x.CreateShortUrlAsync(dto.OriginalUrl, userId))
                .ReturnsAsync((ShortUrl?)null);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("URL already exists");
        }

        [Fact]
        public async Task Create_WithoutUserId_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new CreateShortUrlDto { OriginalUrl = "https://example.com" };
            SetupUserClaims(null, new string[0]);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task Delete_WithValidIdAndOwner_ReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "user123";

            SetupUserClaims(userId, new[] { "User" });

            _mockShortUrlService.Setup(x => x.DeleteAsync(id, userId, false))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_WithAdminUser_ReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "admin123";

            SetupUserClaims(userId, new[] { "Admin" });

            _mockShortUrlService.Setup(x => x.DeleteAsync(id, userId, true))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_WithUnauthorizedAccess_ReturnsForbid()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "user123";

            SetupUserClaims(userId, new[] { "User" });

            _mockShortUrlService.Setup(x => x.DeleteAsync(id, userId, false))
                .ThrowsAsync(new UnauthorizedAccessException());

            // Act
            var result = await _controller.Delete(id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Delete_WithNotFound_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "user123";

            SetupUserClaims(userId, new[] { "User" });

            _mockShortUrlService.Setup(x => x.DeleteAsync(id, userId, false))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.Delete(id);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Delete_WithoutUserId_ReturnsUnauthorized()
        {
            // Arrange
            var id = Guid.NewGuid();
            SetupUserClaims(null, new string[0]);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        private void SetupUserClaims(string? userId, string[] roles)
        {
            var claims = new List<Claim>();
            
            if (userId != null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            }

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
                }
            };
        }
    }
}
