using Microsoft.IdentityModel.Tokens;
using SoareAlexGameServer.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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

    }
}
