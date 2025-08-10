using Microsoft.EntityFrameworkCore;
using Moq;
using Shortener.BLL.Services;
using Shortener.BLL.Services.Interfaces;
using Shortener.DAL.Context;
using Shortener.DAL.Entity;
using Shortener.DAL.UnitOfWork;
using Shortener.DAL.UnitOfWork.Interfaces;
using FluentAssertions;
using Xunit;

namespace Shortener.Tests.Services
{
    public class ShortUrlServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly ShortUrlService _service;

        public ShortUrlServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _service = new ShortUrlService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllShortUrls()
        {
            // Arrange
            var expectedUrls = new List<ShortUrl>
            {
                new ShortUrl { Id = Guid.NewGuid(), ShortCode = "abc123", OriginalUrl = "https://example.com" },
                new ShortUrl { Id = Guid.NewGuid(), ShortCode = "def456", OriginalUrl = "https://test.com" }
            };

            _mockUnitOfWork.Setup(x => x.ShortUrls.GetAllAsync())
                .ReturnsAsync(expectedUrls);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedUrls);
            _mockUnitOfWork.Verify(x => x.ShortUrls.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsShortUrl()
        {
            // Arrange
            var id = Guid.NewGuid();
            var expectedUrl = new ShortUrl
            {
                Id = id,
                ShortCode = "abc123",
                OriginalUrl = "https://example.com"
            };

            _mockUnitOfWork.Setup(x => x.ShortUrls.GetByIdAsync(id))
                .ReturnsAsync(expectedUrl);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            result.Should().BeEquivalentTo(expectedUrl);
            _mockUnitOfWork.Verify(x => x.ShortUrls.GetByIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockUnitOfWork.Setup(x => x.ShortUrls.GetByIdAsync(id))
                .ReturnsAsync((ShortUrl?)null);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByShortCodeAsync_WithValidCode_ReturnsShortUrl()
        {
            // Arrange
            var shortCode = "abc123";
            var expectedUrl = new ShortUrl
            {
                Id = Guid.NewGuid(),
                ShortCode = shortCode,
                OriginalUrl = "https://example.com"
            };

            _mockUnitOfWork.Setup(x => x.ShortUrls.GetByShortCodeAsync(shortCode))
                .ReturnsAsync(expectedUrl);

            // Act
            var result = await _service.GetByShortCodeAsync(shortCode);

            // Assert
            result.Should().BeEquivalentTo(expectedUrl);
            _mockUnitOfWork.Verify(x => x.ShortUrls.GetByShortCodeAsync(shortCode), Times.Once);
        }

        [Fact]
        public async Task GetByShortCodeAsync_WithInvalidCode_ReturnsNull()
        {
            // Arrange
            var shortCode = "invalid";
            _mockUnitOfWork.Setup(x => x.ShortUrls.GetByShortCodeAsync(shortCode))
                .ReturnsAsync((ShortUrl?)null);

            // Act
            var result = await _service.GetByShortCodeAsync(shortCode);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateShortUrlAsync_WithNewUrl_ReturnsCreatedShortUrl()
        {
            // Arrange
            var originalUrl = "https://example.com";
            var userId = "user123";

            _mockUnitOfWork.Setup(x => x.ShortUrls.GetByOriginalUrlAsync(originalUrl))
                .ReturnsAsync((ShortUrl?)null);

            _mockUnitOfWork.Setup(x => x.ShortUrls.AddAsync(It.IsAny<ShortUrl>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateShortUrlAsync(originalUrl, userId);

            // Assert
            result.Should().NotBeNull();
            result!.OriginalUrl.Should().Be(originalUrl);
            result.CreatedByUserId.Should().Be(userId);
            result.ShortCode.Should().NotBeNullOrEmpty();
            result.ShortCode.Should().HaveLength(6);
            result.VisitCount.Should().Be(0);
            result.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

            _mockUnitOfWork.Verify(x => x.ShortUrls.GetByOriginalUrlAsync(originalUrl), Times.Once);
            _mockUnitOfWork.Verify(x => x.ShortUrls.AddAsync(It.IsAny<ShortUrl>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateShortUrlAsync_WithExistingUrl_ReturnsNull()
        {
            // Arrange
            var originalUrl = "https://example.com";
            var userId = "user123";
            var existingUrl = new ShortUrl
            {
                Id = Guid.NewGuid(),
                ShortCode = "abc123",
                OriginalUrl = originalUrl
            };

            _mockUnitOfWork.Setup(x => x.ShortUrls.GetByOriginalUrlAsync(originalUrl))
                .ReturnsAsync(existingUrl);

            // Act
            var result = await _service.CreateShortUrlAsync(originalUrl, userId);

            // Assert
            result.Should().BeNull();
            _mockUnitOfWork.Verify(x => x.ShortUrls.GetByOriginalUrlAsync(originalUrl), Times.Once);
            _mockUnitOfWork.Verify(x => x.ShortUrls.AddAsync(It.IsAny<ShortUrl>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WithValidIdAndOwner_DeletesSuccessfully()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "user123";
            var shortUrl = new ShortUrl
            {
                Id = id,
                ShortCode = "abc123",
                OriginalUrl = "https://example.com",
                CreatedByUserId = userId
            };

            _mockUnitOfWork.Setup(x => x.ShortUrls.GetByIdAsync(id))
                .ReturnsAsync(shortUrl);

            _mockUnitOfWork.Setup(x => x.ShortUrls.Delete(shortUrl))
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _service.DeleteAsync(id, userId, false);

            // Assert
            _mockUnitOfWork.Verify(x => x.ShortUrls.GetByIdAsync(id), Times.Once);
            _mockUnitOfWork.Verify(x => x.ShortUrls.Delete(shortUrl), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithAdminUser_DeletesSuccessfully()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "admin123";
            var shortUrl = new ShortUrl
            {
                Id = id,
                ShortCode = "abc123",
                OriginalUrl = "https://example.com",
                CreatedByUserId = "otheruser"
            };

            _mockUnitOfWork.Setup(x => x.ShortUrls.GetByIdAsync(id))
                .ReturnsAsync(shortUrl);

            _mockUnitOfWork.Setup(x => x.ShortUrls.Delete(shortUrl))
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _service.DeleteAsync(id, userId, true);

            // Assert
            _mockUnitOfWork.Verify(x => x.ShortUrls.GetByIdAsync(id), Times.Once);
            _mockUnitOfWork.Verify(x => x.ShortUrls.Delete(shortUrl), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "user123";

            _mockUnitOfWork.Setup(x => x.ShortUrls.GetByIdAsync(id))
                .ReturnsAsync((ShortUrl?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.DeleteAsync(id, userId, false));
            exception.Message.Should().Be("Short URL not found");
        }

        [Fact]
        public async Task DeleteAsync_WithUnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "user123";
            var shortUrl = new ShortUrl
            {
                Id = id,
                ShortCode = "abc123",
                OriginalUrl = "https://example.com",
                CreatedByUserId = "otheruser"
            };

            _mockUnitOfWork.Setup(x => x.ShortUrls.GetByIdAsync(id))
                .ReturnsAsync(shortUrl);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.DeleteAsync(id, userId, false));
            exception.Message.Should().Be("You can't delete this URL");
        }

        [Fact]
        public async Task UpdateAsync_WithValidShortUrl_UpdatesSuccessfully()
        {
            // Arrange
            var shortUrl = new ShortUrl
            {
                Id = Guid.NewGuid(),
                ShortCode = "abc123",
                OriginalUrl = "https://example.com",
                VisitCount = 5
            };

            _mockUnitOfWork.Setup(x => x.ShortUrls.Update(shortUrl))
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _service.UpdateAsync(shortUrl);

            // Assert
            _mockUnitOfWork.Verify(x => x.ShortUrls.Update(shortUrl), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByUserIdAsync_WithValidUserId_ReturnsUserUrls()
        {
            // Arrange
            var userId = "user123";
            var expectedUrls = new List<ShortUrl>
            {
                new ShortUrl { Id = Guid.NewGuid(), ShortCode = "abc123", OriginalUrl = "https://example.com", CreatedByUserId = userId },
                new ShortUrl { Id = Guid.NewGuid(), ShortCode = "def456", OriginalUrl = "https://test.com", CreatedByUserId = userId }
            };

            _mockUnitOfWork.Setup(x => x.ShortUrls.GetByUserIdAsync(userId))
                .ReturnsAsync(expectedUrls);

            // Act
            var result = await _service.GetByUserIdAsync(userId);

            // Assert
            result.Should().BeEquivalentTo(expectedUrls);
            _mockUnitOfWork.Verify(x => x.ShortUrls.GetByUserIdAsync(userId), Times.Once);
        }


    }
}
