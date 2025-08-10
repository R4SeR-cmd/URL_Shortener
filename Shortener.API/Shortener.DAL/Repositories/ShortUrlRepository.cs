using Microsoft.EntityFrameworkCore;
using Shortener.DAL.Context;
using Shortener.DAL.Entity;
using Shortener.DAL.Repositories.Interfaces;

namespace Shortener.DAL.Repositories
{
	public class ShortUrlRepository : IShortUrlRepository
	{
		private readonly ShortenerDbContext _context;

		public ShortUrlRepository(ShortenerDbContext context)
		{
			_context = context;
		}

		public async Task<ShortUrl?> GetByIdAsync(Guid id)
		{
			return await _context.ShortUrls.FindAsync(id);
		}

		public async Task<ShortUrl?> GetByShortCodeAsync(string shortCode)
		{
			return await _context.ShortUrls
				.FirstOrDefaultAsync(x => x.ShortCode == shortCode);
		}

		public async Task<ShortUrl?> GetByOriginalUrlAsync(string originalUrl)
		{
			return await _context.ShortUrls
				.FirstOrDefaultAsync(x => x.OriginalUrl == originalUrl);
		}

		public async Task AddAsync(ShortUrl entity)
		{
			await _context.ShortUrls.AddAsync(entity);
		}

		public void Update(ShortUrl entity)
		{
			_context.SaveChangesAsync();
		}

		

		public void Delete(ShortUrl entity)
		{
			_context.ShortUrls.Remove(entity);
		}

		public async Task<IEnumerable<ShortUrl>> GetAllAsync()
		{
			return await _context.ShortUrls
				.Include(x => x.CreatedByUser)
				.ToListAsync();
		}
		public async Task<IEnumerable<ShortUrl>> GetByUserIdAsync(string userId)
		{
			return await _context.ShortUrls
				.Include(x => x.CreatedByUser)
				.Where(x => x.CreatedByUserId == userId)
				.ToListAsync();
		}

	}
}
