using Shortener.DAL.Entity;

namespace Shortener.DAL.Repositories.Interfaces
{
	public interface IUserRepository
	{
		void Delete(User user);
		Task AddAsync(User user);
		void Update(User user);
	}
}
