using MediatR;
using System.Net;
using SoareAlexGameServer.Infrastructure.Entities.DB;
using SoareAlexGameServer.Infrastructure.Interfaces.Repositories;
using SoareAlexGameServer.Infrastructure.Interfaces.Cache;

namespace SoareAlexGameServer.WebAPI.Models.Resources
{
    public class GetResource
    {
        public class GetResource_QueryRequest : IRequest<QueryResponse>
        {
            public ResourceType ResourceType { get; set; }
        }

        public class QueryResponse
        {
            public double ResourceValue { get; set; }
            public HttpStatusCode Status { get; set; }
        }

        public class CommandHandler : IRequestHandler<GetResource_QueryRequest, QueryResponse>
        {
            private readonly ILogger<GetResource> logger;
            private readonly IHttpContextAccessor httpContext;
            private readonly IPlayerProfileRepository playersRepo;
            private readonly IPlayerProfilesCacheService playerProfilesCacheService;

            public CommandHandler(ILogger<GetResource> logger, IHttpContextAccessor httpContext, IPlayerProfileRepository playersRepo, IPlayerProfilesCacheService playerProfilesCacheService)
            {
                this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                this.httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
                this.playersRepo = playersRepo ?? throw new ArgumentNullException(nameof(playersRepo));
                this.playerProfilesCacheService = playerProfilesCacheService ?? throw new ArgumentNullException(nameof(playerProfilesCacheService));
            }

            public async Task<QueryResponse> Handle(GetResource_QueryRequest request, CancellationToken cancellationToken)
            {
                var response = new QueryResponse();

                try
                {
                    if (!Enum.IsDefined(typeof(ResourceType), request.ResourceType))
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
                    var playerProfile = playerProfilesCacheService.GetItem(deviceId.Value);
                    if(playerProfile == null)
                    {
                        // Search in DB
                        playerProfile = await playersRepo.GetItemAsync(deviceId.Value);
                        if (playerProfile == null)
                        {
                            logger.LogError($"Strange behaviour, request passed JWT validation, but the player profile doesn't exists in DB, DeviceId: {deviceId.Value}");
                            response.Status = HttpStatusCode.InternalServerError;
                            return response;
                        }

                        // Add profile to cache
                        playerProfilesCacheService.SetCachedItem(playerProfile.DeviceId, playerProfile.PlayerId, playerProfile);
                    }

                    var resourceToFind = playerProfile.Resources.FirstOrDefault(r => r.ResourceType == request.ResourceType);
                    if (resourceToFind != null)
                    {
                        response.ResourceValue = resourceToFind.Value;
                        response.Status = HttpStatusCode.OK;
                    }
                    else
                        response.Status = HttpStatusCode.NotFound;

                    return response;

                }
                catch (Exception ex)
                {
                    logger.LogError($"Getting resource {request.ResourceType} got an unexpected error: {ex.Message}");
                    response.Status = HttpStatusCode.InternalServerError;
                    return response;
                }
            }
        }
    }
}
