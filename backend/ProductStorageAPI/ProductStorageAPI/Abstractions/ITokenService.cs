using ProductStorageAPI.Models;

namespace ProductStorageAPI.Abstractions
{
    public interface ITokenService
    {
        TokenModel GenerateToken(User user);
    }
}
