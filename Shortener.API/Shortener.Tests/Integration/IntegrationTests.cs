using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shortener.DAL.Context;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Shortener.Tests.Integration
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public IntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ShortenerDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database for testing
                    services.AddDbContext<ShortenerDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Register_WithValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var registerData = new
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Register_WithInvalidEmail_ReturnsBadRequest()
        {
            // Arrange
            var registerData = new
            {
                Email = "invalid-email",
                Password = "TestPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_WithWeakPassword_ReturnsBadRequest()
        {
            // Arrange
            var registerData = new
            {
                Email = "test@example.com",
                Password = "weak"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var registerData = new
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            // Register first
            await _client.PostAsJsonAsync("/api/auth/register", registerData);

            // Act - Login
            var response = await _client.PostAsJsonAsync("/api/auth/login", registerData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            var loginData = new
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateShortUrl_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            var urlData = new
            {
                OriginalUrl = "https://example.com"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/urls", urlData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetUrls_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/urls");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateAndGetUrls_WithAuthentication_ReturnsSuccess()
        {
            // Arrange
            var registerData = new
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            // Register and get token
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerData);
            var registerResult = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
            var token = registerResult.GetProperty("token").GetString();

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create short URL
            var urlData = new
            {
                OriginalUrl = "https://example.com"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/urls", urlData);

            // Assert create
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Get URLs
            var getResponse = await _client.GetAsync("/api/urls");

            // Assert get
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var urls = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
            urls.GetArrayLength().Should().Be(1);
        }

        [Fact]
        public async Task DeleteUrl_WithAuthentication_ReturnsSuccess()
        {
            // Arrange
            var registerData = new
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            // Register and get token
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerData);
            var registerResult = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
            var token = registerResult.GetProperty("token").GetString();

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create short URL
            var urlData = new
            {
                OriginalUrl = "https://example.com"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/urls", urlData);
            var createdUrl = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
            var urlId = createdUrl.GetProperty("id").GetString();

            // Act - Delete URL
            var deleteResponse = await _client.DeleteAsync($"/api/urls/{urlId}");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify URL is deleted
            var getResponse = await _client.GetAsync("/api/urls");
            var urls = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
            urls.GetArrayLength().Should().Be(0);
        }

        [Fact]
        public async Task RedirectToOriginal_WithValidShortCode_ReturnsRedirect()
        {
            // Arrange
            var registerData = new
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            // Register and get token
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerData);
            var registerResult = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
            var token = registerResult.GetProperty("token").GetString();

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create short URL
            var urlData = new
            {
                OriginalUrl = "https://example.com"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/urls", urlData);
            var createdUrl = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
            var shortCode = createdUrl.GetProperty("shortCode").GetString();

            // Remove authorization header for redirect test
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var redirectResponse = await _client.GetAsync($"/api/urls/{shortCode}");

            // Assert
            redirectResponse.StatusCode.Should().Be(HttpStatusCode.Found);
            redirectResponse.Headers.Location?.ToString().Should().Be("https://example.com");
        }

        [Fact]
        public async Task RedirectToOriginal_WithInvalidShortCode_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/urls/invalid");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetUrlById_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var registerData = new
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            // Register and get token
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerData);
            var registerResult = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
            var token = registerResult.GetProperty("token").GetString();

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create short URL
            var urlData = new
            {
                OriginalUrl = "https://example.com"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/urls", urlData);
            var createdUrl = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
            var urlId = createdUrl.GetProperty("id").GetString();

            // Act
            var response = await _client.GetAsync($"/api/urls/{urlId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("originalUrl").GetString().Should().Be("https://example.com");
        }

        [Fact]
        public async Task GetUrlById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var registerData = new
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            // Register and get token
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerData);
            var registerResult = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
            var token = registerResult.GetProperty("token").GetString();

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var invalidId = Guid.NewGuid().ToString();

            // Act
            var response = await _client.GetAsync($"/api/urls/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
