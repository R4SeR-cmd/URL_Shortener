using Shortener.DAL.Repositories.Interfaces;

namespace Shortener.DAL.UnitOfWork.Interfaces
{
	public interface IUnitOfWork
	{
		IUserRepository Users { get; }
		IShortUrlRepository ShortUrls { get; }
		Task<int> SaveChangesAsync();
	}
}
