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
        public class Login_QueryRequest : IRequest<QueryResponse>
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

        public class CommandHandler : IRequestHandler<Login_QueryRequest, QueryResponse>
        {
            private readonly ILogger<Login> logger;
            private readonly IPlayerProfileRepository playerProfilesRepo;
            private readonly IPlayerProfilesCacheService playerProfilesCacheService;
            private readonly IOnlinePlayersCacheService onlinePlayersCache;
            private readonly IJwtTokenProvider jwtProvider;

            public CommandHandler(ILogger<Login> logger, IPlayerProfileRepository playerProfilesRepo, IPlayerProfilesCacheService playerProfilesCacheService, IOnlinePlayersCacheService onlinePlayersCache, IJwtTokenProvider jwtProvider)
            {
                this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                this.playerProfilesRepo = playerProfilesRepo ?? throw new ArgumentNullException(nameof(playerProfilesRepo));
                this.playerProfilesCacheService = playerProfilesCacheService ?? throw new ArgumentNullException(nameof(playerProfilesCacheService));
                this.onlinePlayersCache = onlinePlayersCache ?? throw new ArgumentNullException(nameof(onlinePlayersCache));
                this.jwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));
            }

            public async Task<QueryResponse> Handle(Login_QueryRequest request, CancellationToken cancellationToken)
            {
                var response = new QueryResponse();

                try
                {
                    if (string.IsNullOrEmpty(request.DeviceId))
                    {

                        response.Status = HttpStatusCode.BadRequest;
                        return response;
                    }

                    // Search for profile in cache
                    var playerProfile = playerProfilesCacheService.GetItem(request.DeviceId);
                    if (playerProfile == null)
                    {
                        // Search in DB
                        playerProfile = await playerProfilesRepo.GetItemAsync(request.DeviceId);
                        if (playerProfile == null)
                        {
                            // New player
                            playerProfile = new PlayerProfile()
                            {
                                DeviceId = request.DeviceId,
                                PlayerId = Guid.NewGuid().ToString(),
                            };

                            await playerProfilesRepo.AddItemAsync(playerProfile);
                            logger.LogInformation($"New player account got created! {playerProfile.DeviceId} {playerProfile.PlayerId}");
                        }

                        // Add profile to cache
                        playerProfilesCacheService.SetCachedItem(playerProfile.DeviceId, playerProfile.PlayerId, playerProfile);
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
                        DeviceId = playerProfile.DeviceId,
                    };

                    onlinePlayersCache.SetCachedItem(playerProfile.DeviceId, onlinePlayer);

                    response.PlayerId = playerProfile.PlayerId;
                    response.AuthToken = jwtProvider.GenerateToken(new Claim[] { new Claim("DeviceId", playerProfile.DeviceId) });
                    response.Status = HttpStatusCode.OK;

                    return response;
                }
                catch (Exception ex)
                {
                    logger.LogError($"DeviceId {request.DeviceId} got some an unexpected error when trying to loggin: {ex.Message}");
                    response.Status = HttpStatusCode.InternalServerError;
                    return response;
                }
            }
        }
    }
}
