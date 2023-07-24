using System.Security.Claims;

namespace SoareAlexGameServer.Infrastructure.Interfaces
{
    public interface IJwtTokenProvider
    {
        string GenerateToken(Claim[] claims);
        bool ValidateToken(string token, out List<Claim> claims);
    }
}
