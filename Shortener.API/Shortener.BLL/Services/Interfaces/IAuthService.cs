using Shortener.BLL.DTO_s;

namespace Shortener.BLL.Services.Interfaces
{
	public interface IAuthService
	{
		Task<string> RegisterAsync(UserCredentialsDTO dto);
		Task<string> LoginAsync(UserCredentialsDTO dto);
	}
}
