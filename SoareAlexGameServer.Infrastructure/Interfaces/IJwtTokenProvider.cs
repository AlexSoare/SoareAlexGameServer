using System.Security.Claims;

namespace SoareAlexGameServer.Infrastructure.Interfaces
{
    public interface IJwtTokenProvider
    {
        string GenerateToken(Claim[] claims);
    }
}
