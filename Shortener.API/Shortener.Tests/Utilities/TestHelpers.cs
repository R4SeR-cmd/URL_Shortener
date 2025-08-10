using Microsoft.AspNetCore.Identity;
using Moq;
using Shortener.DAL.Entity;
using System.Security.Claims;

namespace Shortener.Tests.Utilities
{
    public static class TestHelpers
    {
        public static Mock<UserManager<User>> CreateMockUserManager()
        {
            var userStore = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(
                userStore.Object,
                null!, null!, null!, null!, null!, null!, null!, null!);
        }

        public static Mock<RoleManager<IdentityRole>> CreateMockRoleManager()
        {
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(
                roleStore.Object,
                null!, null!, null!, null!);
        }

        public static ClaimsPrincipal CreateClaimsPrincipal(string? userId = null, string[]? roles = null)
        {
            var claims = new List<Claim>();
            
            if (userId != null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            }

            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }

        public static ShortUrl CreateTestShortUrl(
            Guid? id = null,
            string shortCode = "abc123",
            string originalUrl = "https://example.com",
            string createdByUserId = "user123",
            int visitCount = 0)
        {
            return new ShortUrl
            {
                Id = id ?? Guid.NewGuid(),
                ShortCode = shortCode,
                OriginalUrl = originalUrl,
                CreatedByUserId = createdByUserId,
                CreatedAtUtc = DateTime.UtcNow,
                VisitCount = visitCount
            };
        }

        public static User CreateTestUser(
            string? id = null,
            string email = "test@example.com",
            string userName = "test@example.com")
        {
            return new User
            {
                Id = id ?? Guid.NewGuid().ToString(),
                Email = email,
                UserName = userName
            };
        }

        public static List<ShortUrl> CreateTestShortUrls(int count = 3)
        {
            var urls = new List<ShortUrl>();
            for (int i = 0; i < count; i++)
            {
                urls.Add(CreateTestShortUrl(
                    shortCode: $"abc{i:D3}",
                    originalUrl: $"https://example{i}.com"
                ));
            }
            return urls;
        }

        public static string GenerateValidJwtToken()
        {
            // This is a mock JWT token for testing purposes
            // In real scenarios, you would use a proper JWT library
            return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        }

        public static bool IsValidShortCode(string shortCode)
        {
            return !string.IsNullOrEmpty(shortCode) && 
                   shortCode.Length == 6 && 
                   shortCode.All(c => char.IsLetterOrDigit(c));
        }

        public static bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) && 
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
