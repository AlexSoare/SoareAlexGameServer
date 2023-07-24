using MediatR;
using Microsoft.IdentityModel.Tokens;
using SoareAlexGameServer.Infrastructure.Interfaces;
using SoareAlexGameServer.Infrastructure.Interfaces.Cache;
using SoareAlexGameServer.Infrastructure.Interfaces.Repositories;
using SoareAlexGameServer.Infrastructure.Entities.DB;
using SoareAlexGameServer.Infrastructure.Entities;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace SoareAlexGameServer.WebAPI.Models.UserAuthentification
{
    public class Login
    {
        public class QueryRequest : IRequest<QueryResponse>
        {
            public string DeviceId { get; set; }
        }

        public class QueryResponse
        {
            public string PlayerId { get; set; }
            public bool AlreadyOnline { get; set; }
            public string AuthToken { get; set; }
            public HttpStatusCode Status { get; set; }
        }

        public class CommandHandler : IRequestHandler<QueryRequest, QueryResponse>
        {

            private IPlayerProfileRepository playerProfilesRepo;
            private IOnlinePlayersCacheService onlinePlayersCache;
            private IJwtTokenProvider jwtProvider;

            public CommandHandler(IPlayerProfileRepository playerProfilesRepo, IOnlinePlayersCacheService onlinePlayersCache, IJwtTokenProvider jwtProvider)
            {
                this.playerProfilesRepo = playerProfilesRepo ?? throw new ArgumentNullException(nameof(playerProfilesRepo));
                this.onlinePlayersCache = onlinePlayersCache ?? throw new ArgumentNullException(nameof(onlinePlayersCache));
                this.jwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));
            }

            public async Task<QueryResponse> Handle(QueryRequest request, CancellationToken cancellationToken)
            {
                var response = new QueryResponse();

                // Search in DB
                var  playerProfile = await playerProfilesRepo.GetItemAsync(request.DeviceId);

                if (playerProfile == null)
                {
                    // New player
                    playerProfile = new PlayerProfile()
                    {
                        DeviceId = request.DeviceId,
                        PlayerId = Guid.NewGuid().ToString(),
                    };

                    await playerProfilesRepo.AddItemAsync(playerProfile);
                }

                // Search in online players
                var onlinePlayer = onlinePlayersCache.GetItem(request.DeviceId);

                if (onlinePlayer != null)
                {
                    response.PlayerId = onlinePlayer.PlayerId;
                    response.AlreadyOnline = true;
                    response.AuthToken = jwtProvider.GenerateToken(new Claim[] { new Claim("DeviceId", request.DeviceId) });
                    response.Status = HttpStatusCode.OK;

                    return response;
                }

                onlinePlayer = new OnlinePlayer()
                {
                    PlayerId = playerProfile.PlayerId,
                };

                onlinePlayersCache.SetCachedItem(playerProfile.DeviceId, onlinePlayer);

                response.PlayerId = playerProfile.PlayerId;
                response.AuthToken = jwtProvider.GenerateToken(new Claim[] { new Claim("DeviceId", playerProfile.DeviceId) });
                response.Status = HttpStatusCode.OK;

                return response;
            }
        }
    }
}
