using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Shortener.BLL.DTO_s;
using Shortener.BLL.Services.Interfaces;
using Shortener.DAL.Entity;
using System.Security.Claims;
using System.Text;
using Shortener.BLL.Options;
using Microsoft.IdentityModel.Tokens;

namespace Shortener.BLL.Services
{
	public class AuthService : IAuthService
	{
		private readonly UserManager<User> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly JwtOptions _jwtOptions;
		public AuthService(
			UserManager<User> userManager,
			RoleManager<IdentityRole> roleManager,
			IOptions<JwtOptions> options
		)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_jwtOptions = options.Value;
		}

		public async Task<string> RegisterAsync(UserCredentialsDTO dto)
		{
			var user = new User
			{
				UserName = dto.Email,
				Email = dto.Email,
			};

			var result = await _userManager.CreateAsync(user, dto.Password);

			if (!result.Succeeded)
				throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

			var role = "User";
			if (dto.Email.Equals("rostukvaso@gmail.com"))
			{
				role = "Admin";
			}

			if (!await _roleManager.RoleExistsAsync(role))
				throw new Exception("Role does not exist");

			await _userManager.AddToRoleAsync(user, role);

			return GenerateToken(user);
		}


		public async Task<string> LoginAsync(UserCredentialsDTO dto)
		{
			var user = await _userManager.FindByEmailAsync(dto.Email);
			if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
				throw new Exception("Invalid credentials");

			return GenerateToken(user);
		}

		public string GenerateToken(User user)
		{
			var userRoles =  _userManager.GetRolesAsync(user).Result;

			var claims = new List<Claim>
			{
				new(ClaimTypes.NameIdentifier, user.Id),
				new(ClaimTypes.Email, user.Email)
			};

			claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));

			var jwtToken = new JwtSecurityToken(
				issuer: _jwtOptions.Issuer,
				audience: _jwtOptions.Audience,
				claims: claims,
				expires: DateTime.Now.AddHours(1),
				signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
			);
			return new JwtSecurityTokenHandler().WriteToken(jwtToken);
		}

	}
}
