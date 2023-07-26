using MediatR;
using System.Net;
using SoareAlexGameServer.Infrastructure.Entities.DB;
using SoareAlexGameServer.Infrastructure.Interfaces.Repositories;
using SoareAlexGameServer.Infrastructure.Interfaces.Cache;
using SoareAlexGameServer.Infrastructure.WebSockets.Events;
using SoareAlexGameServer.Infrastructure.WebSockets;

namespace SoareAlexGameServer.WebAPI.Models.Gifts
{
    public class SendGift
    {
        public class SendGift_QueryRequest : IRequest<QueryResponse>
        {
            public string FriendPlayerId { get; set; }
            public ResourceType ResourceType { get; set; }
            public double ResourceValue { get; set; }
        }

        public class QueryResponse
        {
            public HttpStatusCode Status { get; set; }
        }

        public class CommandHandler : IRequestHandler<SendGift_QueryRequest, QueryResponse>
        {
            private readonly ILogger<SendGift> logger;
            private readonly IHttpContextAccessor httpContext;
            private readonly IPlayerProfileRepository playersRepo;
            private readonly IPlayerProfilesCacheService playerProfilesCacheService;
            private readonly IOnlinePlayersCacheService onlinePlayersCacheService;

            public CommandHandler(ILogger<SendGift> logger, IHttpContextAccessor httpContext, IPlayerProfileRepository playersRepo,IPlayerProfilesCacheService playerProfilesCacheService, IOnlinePlayersCacheService onlinePlayersCacheService)
            {
                this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                this.httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
                this.playersRepo = playersRepo ?? throw new ArgumentNullException(nameof(playersRepo));
                this.playerProfilesCacheService = playerProfilesCacheService ?? throw new ArgumentNullException(nameof(playerProfilesCacheService));
                this.onlinePlayersCacheService = onlinePlayersCacheService ?? throw new ArgumentNullException(nameof(onlinePlayersCacheService));
            }

            public async Task<QueryResponse> Handle(SendGift_QueryRequest request, CancellationToken cancellationToken)
            {
                var response = new QueryResponse();

                try
                {
                    if (!Enum.IsDefined(typeof(ResourceType), request.ResourceType))
                    {
                        response.Status = HttpStatusCode.BadRequest;
                        return response;
                    }

                    if (request.ResourceValue < 1)
                    {
                        response.Status = HttpStatusCode.BadRequest;
                        return response;
                    }

                    var deviceId = httpContext.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "DeviceId");
                    if (deviceId == null)
                    {
                        logger.LogError($"Strange behaviour, request passed JWT validation, but the DeviceId claim is empty!");
                        response.Status = HttpStatusCode.BadRequest;
                        return response;
                    }

                    // Search for profile in cache
                    var callerPlayerProfile = playerProfilesCacheService.GetItem(deviceId.Value);

                    if (callerPlayerProfile == null)
                    {
                        // Search in DB
                        callerPlayerProfile = await playersRepo.GetItemAsync(deviceId.Value);
                        if (callerPlayerProfile == null)
                        {
                            logger.LogError($"Strange behaviour, request passed JWT validation, but the player profile doesn't exists in DB, DeviceId: {deviceId.Value}");
                            response.Status = HttpStatusCode.InternalServerError;
                            return response;
                        }
                    }

                    if (callerPlayerProfile.PlayerId == request.FriendPlayerId)
                    {
                        // You are trying to send a gift to yourself
                        response.Status = HttpStatusCode.BadRequest;
                        return response;
                    }

                    var resourceToDebitFrom = callerPlayerProfile.Resources.FirstOrDefault(r => r.ResourceType == request.ResourceType);
                    if (resourceToDebitFrom == null || !resourceToDebitFrom.IsDebitable(request.ResourceValue))
                    {
                        // You don't have enough to send
                        response.Status = HttpStatusCode.BadRequest;
                        return response;
                    }

                    // Search for profile in cache
                    var friendProfile = playerProfilesCacheService.GetItemByPlayerId(request.FriendPlayerId);
                    if (friendProfile == null)
                    {
                        // Search in DB
                        friendProfile = await playersRepo.GetItemByPlayerIdAsync(request.FriendPlayerId);
                        if (friendProfile == null)
                        {
                            // Your friend doesn't exist
                            response.Status = HttpStatusCode.NotFound;
                            return response;
                        }
                    }

                    resourceToDebitFrom.Debit(request.ResourceValue);

                    var resourceToUpdate = friendProfile.Resources.FirstOrDefault(r => r.ResourceType == request.ResourceType);
                    if (resourceToUpdate == null)
                    {
                        resourceToUpdate = new Resource()
                        {
                            ResourceType = request.ResourceType,
                            Value = request.ResourceValue,
                        };

                        friendProfile.Resources.Add(resourceToUpdate);
                    }
                    else
                        resourceToUpdate.Credit(request.ResourceValue);

                    await playersRepo.UpdateItemAsync(callerPlayerProfile.PlayerId, callerPlayerProfile);
                    await playersRepo.UpdateItemAsync(request.FriendPlayerId, friendProfile);

                    // Add profiles to cache
                    playerProfilesCacheService.SetCachedItem(callerPlayerProfile.DeviceId, callerPlayerProfile.PlayerId, callerPlayerProfile);
                    playerProfilesCacheService.SetCachedItem(friendProfile.DeviceId, friendProfile.PlayerId, friendProfile);

                    // Send Gift event if friend is online
                    var onlineFriend = onlinePlayersCacheService.GetItem(friendProfile.DeviceId);
                    if (onlineFriend != null)
                    {
                        var wsMessage = new GiftEvent()
                        {
                            SenderId = callerPlayerProfile.PlayerId,
                            ResourceType = request.ResourceType,
                            ResourceValue = request.ResourceValue,
                        };

                        await onlineFriend.SendToWebSocket(WebSocketEventType.Gift, wsMessage);
                    }

                    response.Status = HttpStatusCode.OK;

                    return response;
                }
                catch
                {
                    response.Status = HttpStatusCode.InternalServerError;
                    return response;
                }
            }
        }
    }
}
