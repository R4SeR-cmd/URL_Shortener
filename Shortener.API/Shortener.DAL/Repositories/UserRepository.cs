using Shortener.DAL.Context;
using Shortener.DAL.Entity;
using Shortener.DAL.Repositories.Interfaces;

namespace Shortener.DAL.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly ShortenerDbContext _context;

		public UserRepository(ShortenerDbContext context)
		{
			_context = context;
		}


		public void Delete(User user)
		{
			_context.Users.Remove(user);
		}

		public async Task AddAsync(User user)
		{
			await _context.Users.AddAsync(user);
		}

		public void Update(User user)
		{
			_context.Users.Update(user);
		}
	}
}
