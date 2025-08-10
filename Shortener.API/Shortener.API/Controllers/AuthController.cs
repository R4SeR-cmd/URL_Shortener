﻿using Microsoft.AspNetCore.Mvc;
using Shortener.BLL.DTO_s;
using Shortener.BLL.Services.Interfaces;

namespace Shortener.API.Controllers
{
	[ApiController]
	[Route("api/auth")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] UserCredentialsDTO dto)
		{
			var token = await _authService.RegisterAsync(dto);
			return Ok(new { token });
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] UserCredentialsDTO dto)
		{
			var token = await _authService.LoginAsync(dto);
			return Ok(new { token });
		}
		

	}
}
