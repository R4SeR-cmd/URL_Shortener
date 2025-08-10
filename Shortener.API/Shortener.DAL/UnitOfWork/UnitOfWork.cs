using Shortener.DAL.Context;
using Shortener.DAL.Repositories.Interfaces;
using Shortener.DAL.UnitOfWork.Interfaces;

namespace Shortener.DAL.UnitOfWork
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ShortenerDbContext _context;

		public IUserRepository Users { get; }
		public IShortUrlRepository ShortUrls { get; }

		public UnitOfWork(
			ShortenerDbContext context,
			IUserRepository userRepository,
			IShortUrlRepository shortUrlRepository)
		{
			_context = context;
			Users = userRepository;
			ShortUrls = shortUrlRepository;
		}

		public async Task<int> SaveChangesAsync()
		{
			return await _context.SaveChangesAsync();
		}
	}
}
