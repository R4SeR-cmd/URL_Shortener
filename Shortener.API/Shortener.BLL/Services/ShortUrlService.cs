using Shortener.BLL.Services.Interfaces;
using Shortener.DAL.Entity;
using Shortener.DAL.UnitOfWork.Interfaces;

namespace Shortener.BLL.Services
{
	public class ShortUrlService : IShortUrlService
	{
		private readonly IUnitOfWork _unitOfWork;

		public ShortUrlService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<IEnumerable<ShortUrl>> GetAllAsync()
		{
			return await _unitOfWork.ShortUrls.GetAllAsync();
		}

		public async Task<ShortUrl?> GetByIdAsync(Guid id)
		{
			return await _unitOfWork.ShortUrls.GetByIdAsync(id);
		}

		public async Task<ShortUrl?> GetByShortCodeAsync(string shortCode)
		{
			return await _unitOfWork.ShortUrls.GetByShortCodeAsync(shortCode);
		}

		public async Task<ShortUrl?> CreateShortUrlAsync(string originalUrl, string createdByUserId)
		{
			
			var existing = await _unitOfWork.ShortUrls.GetByOriginalUrlAsync(originalUrl);
			if (existing != null)
				return null;

			
			var shortCode = GenerateShortCode();

			var shortUrl = new ShortUrl
			{
				OriginalUrl = originalUrl,
				ShortCode = shortCode,
				CreatedByUserId = createdByUserId,
				CreatedAtUtc = DateTime.UtcNow,
				VisitCount = 0
			};

			await _unitOfWork.ShortUrls.AddAsync(shortUrl);
			await _unitOfWork.SaveChangesAsync();

			return shortUrl;
		}

		public async Task DeleteAsync(Guid id, string requestedByUserId, bool isAdmin)
		{
			var shortUrl = await _unitOfWork.ShortUrls.GetByIdAsync(id);
			if (shortUrl == null)
				throw new KeyNotFoundException("Short URL not found");

			if (!isAdmin && shortUrl.CreatedByUserId != requestedByUserId)
				throw new UnauthorizedAccessException("You can't delete this URL");

			_unitOfWork.ShortUrls.Delete(shortUrl);
			await _unitOfWork.SaveChangesAsync();
		}

		private string GenerateShortCode()
		{
			const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var random = new Random();

			var codeChars = new char[6];
			for (int i = 0; i < 6; i++)
			{
				codeChars[i] = chars[random.Next(chars.Length)];
			}

			return new string(codeChars);
		}
		public async Task UpdateAsync(ShortUrl shortUrl)
		{
			_unitOfWork.ShortUrls.Update(shortUrl);
			await _unitOfWork.SaveChangesAsync();
		}
		public async Task<IEnumerable<ShortUrl>> GetByUserIdAsync(string userId)
		{
			return await _unitOfWork.ShortUrls.GetByUserIdAsync(userId);
		}

	}
}
