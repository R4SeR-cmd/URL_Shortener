using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shortener.DAL.Entity;

namespace Shortener.DAL.Context
{
	public class ShortenerDbContext :IdentityDbContext<User>
	{
		public ShortenerDbContext(DbContextOptions<ShortenerDbContext> options) :
			base(options)
		{
			
		}
		public DbSet<ShortUrl> ShortUrls { get; set; } = null!;
	}
}
