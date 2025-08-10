using Shortener.DAL.Entity;

namespace Shortener.DAL.Repositories.Interfaces
{
	public interface IShortUrlRepository
	{
		Task<ShortUrl?> GetByIdAsync(Guid id);
		Task<ShortUrl?> GetByShortCodeAsync(string shortCode);
		Task<ShortUrl?> GetByOriginalUrlAsync(string originalUrl);
		Task AddAsync(ShortUrl entity);
		void Update(ShortUrl entity);
		void Delete(ShortUrl entity);
		Task<IEnumerable<ShortUrl>> GetAllAsync();
		Task<IEnumerable<ShortUrl>> GetByUserIdAsync(string userId);


	}
}
