using Microsoft.IdentityModel.Tokens;
using SoareAlexGameServer.Infrastructure.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SoareAlexGameServer.Infrastructure.Services
{
    public class LocalJwtTokenProvider : IJwtTokenProvider
    {
        private SymmetricSecurityKey securityKey;
        private SigningCredentials credentials;

        private string issuer;
        private string audience;

        public LocalJwtTokenProvider(string encryptionKey,string issuer, string audience) {

            securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(encryptionKey));
            credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            this.issuer = issuer;
            this.audience = audience;
        }

        public string GenerateToken(Claim[] claims)
        {
            var token = new JwtSecurityToken(issuer,
                audience,
                claims,
                null,
                signingCredentials: credentials);


            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public bool ValidateToken(string token,out List<Claim> claims)
        {
            claims = new List<Claim>();

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                // Set the validation parameters for the token
                var validationParameters = new TokenValidationParameters
                {
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = securityKey,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                };

                // Validate the token
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                var jwtToken = tokenHandler.ReadJwtToken(token);

                claims = jwtToken.Claims.ToList();

                // The token is valid if no exception is thrown
                return true;
            }
            catch (SecurityTokenException)
            {
                // Token validation failed
                return false;
            }
            catch (Exception ex)
            {
                // Other exceptions (e.g., malformed token)
                // Handle as needed
                return false;
            }
        }
    }
}
