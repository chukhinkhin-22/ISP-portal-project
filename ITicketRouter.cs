using ISP_Portal.API.Models;

namespace ISP_Portal.API.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}
