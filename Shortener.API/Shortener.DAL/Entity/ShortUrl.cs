using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Shortener.DAL.Entity
{
	public class ShortUrl
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required]
		[Url]
		public string OriginalUrl { get; set; } = null!;

		[Required]
		[MaxLength(10)]
		public string ShortCode { get; set; } = null!;

		[Required]
		public string CreatedByUserId { get; set; } = null!;

		[ForeignKey(nameof(CreatedByUserId))]
		public User CreatedByUser { get; set; } = null!;

		public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

		public int VisitCount { get; set; } = 0;
	}
}

