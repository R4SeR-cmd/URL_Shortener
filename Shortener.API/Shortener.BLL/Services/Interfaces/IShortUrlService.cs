using Shortener.DAL.Entity;

namespace Shortener.BLL.Services.Interfaces
{
    public interface IShortUrlService
    {
        Task<IEnumerable<ShortUrl>> GetAllAsync();
        Task<ShortUrl?> GetByIdAsync(Guid id);
        Task<ShortUrl?> GetByShortCodeAsync(string shortCode);
        Task<ShortUrl?> CreateShortUrlAsync(string originalUrl, string createdByUserId);
        Task DeleteAsync(Guid id, string requestedByUserId, bool isAdmin);
        Task UpdateAsync(ShortUrl shortUrl);
        Task<IEnumerable<ShortUrl>> GetByUserIdAsync(string userId);
    }
}
