using MediatR;
using System.Net;
using SoareAlexGameServer.Infrastructure.Entities.DB;
using SoareAlexGameServer.Infrastructure.Interfaces.Repositories;
using SoareAlexGameServer.Infrastructure.Interfaces.Cache;
using System.Net.WebSockets;
using SoareAlexGameServer.Infrastructure.Services;

namespace SoareAlexGameServer.WebAPI.Models.Gifts
{
    public class SendGift
    {
        public class QueryRequest : IRequest<QueryResponse>
        {
            public string FriendPlayerId { get; set; }
            public ResourceType ResourceType { get; set; }
            public double ResourceValue { get; set; }
        }

        public class QueryResponse
        {
            public HttpStatusCode Status { get; set; }
        }

        public class CommandHandler : IRequestHandler<QueryRequest, QueryResponse>
        {
            private IHttpContextAccessor httpContext;

            private IPlayerProfileRepository playersRepo;
            private IOnlinePlayersCacheService onlinePlayersCacheService;

            public CommandHandler(IHttpContextAccessor httpContext, IPlayerProfileRepository playersRepo, IOnlinePlayersCacheService onlinePlayersCacheService)
            {
                this.httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
                this.playersRepo = playersRepo ?? throw new ArgumentNullException(nameof(playersRepo));
                this.onlinePlayersCacheService = onlinePlayersCacheService ?? throw new ArgumentNullException(nameof(onlinePlayersCacheService));
            }

            public async Task<QueryResponse> Handle(QueryRequest request, CancellationToken cancellationToken)
            {
                var response = new QueryResponse();

                var deviceId = httpContext.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "DeviceId");
                if (deviceId == null)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                var thisPlayerProfile = await playersRepo.GetItemAsync(deviceId.Value);
                if (thisPlayerProfile == null)
                {
                    response.Status = HttpStatusCode.InternalServerError;
                    return response;
                }

                if(thisPlayerProfile.PlayerId == request.FriendPlayerId)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }
                 
                var friendProfile = await playersRepo.GetItemByPlayerIdAsync(request.FriendPlayerId);
                if (friendProfile == null)
                {
                    response.Status = HttpStatusCode.NotFound;
                    return response;
                }

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
                    resourceToUpdate.Add(request.ResourceValue);

                await playersRepo.UpdateItemAsync(request.FriendPlayerId, friendProfile);

                var onlineFiend = onlinePlayersCacheService.GetItem(friendProfile.DeviceId);
                if (onlineFiend != null)
                {
                    var wsMessage = new WebSocketMessage()
                    {
                        Event = WebSocketEvent.GiftEvent,
                        Message = $"Received {request.ResourceValue} {request.ResourceType} from {thisPlayerProfile.PlayerId}"
                    };

                    await onlineFiend.SendToWebSocket(wsMessage);
                }

                response.Status = HttpStatusCode.OK;

                return response;
            }
        }
    }
}
