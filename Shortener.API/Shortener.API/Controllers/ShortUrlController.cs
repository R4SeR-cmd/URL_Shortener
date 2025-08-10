using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shortener.BLL.DTO_s;
using Shortener.BLL.Services.Interfaces;
using Shortener.DAL.Entity;
using System.Security.Claims;

namespace Shortener.API.Controllers
{
	[ApiController]
	[Route("api/urls")]
	public class ShortUrlController : ControllerBase
	{
		private readonly IShortUrlService _shortUrlService;

		public ShortUrlController(IShortUrlService shortUrlService)
		{
			_shortUrlService = shortUrlService;
		}
		
		[HttpGet("{shortCode}")]
		public async Task<IActionResult> RedirectToOriginal(string shortCode)
		{
			var shortUrl = await _shortUrlService.GetByShortCodeAsync(shortCode);
			if (shortUrl == null)
				return NotFound();

			shortUrl.VisitCount++;
			_shortUrlService.UpdateAsync(shortUrl);
				
			return Redirect(shortUrl.OriginalUrl);
		}

		[Authorize]
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
				return Unauthorized();

			bool isAdmin = User.IsInRole("Admin");

			IEnumerable<ShortUrl> list;
			if (isAdmin)
			{
				list = await _shortUrlService.GetAllAsync();
			}
			else
			{
				list = await _shortUrlService.GetByUserIdAsync(userId);
			}

			return Ok(list);
		}



		[Authorize]
		[HttpGet("{id:guid}")]
		public async Task<IActionResult> GetById(Guid id)
		{
			var url = await _shortUrlService.GetByIdAsync(id);
			if (url == null)
				return NotFound();

			return Ok(url);
		}

		
		[Authorize]
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateShortUrlDto dto)
		{
			var originalUrl = dto.OriginalUrl;
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
				return Unauthorized();

			var created = await _shortUrlService.CreateShortUrlAsync(originalUrl, userId);
			if (created == null)
				return BadRequest("URL already exists");

			return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
		}

		[Authorize]
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
				return Unauthorized();

			bool isAdmin = User.IsInRole("Admin");

			try
			{
				await _shortUrlService.DeleteAsync(id, userId, isAdmin);
				return NoContent();
			}
			catch (UnauthorizedAccessException)
			{
				return Forbid();
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
		}
	}
}
