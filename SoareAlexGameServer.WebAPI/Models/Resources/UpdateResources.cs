using MediatR;
using System.Net;
using SoareAlexGameServer.Infrastructure.Entities.DB;
using SoareAlexGameServer.Infrastructure.Interfaces.Repositories;
using SoareAlexGameServer.Infrastructure.Interfaces.Cache;

namespace SoareAlexGameServer.WebAPI.Models.Resources
{
    public class UpdateResources
    {
        public class UpdateResources_QueryRequest : IRequest<QueryResponse>
        {
            public ResourceType ResourceType { get; set; }
            public double ResourceValue { get; set; }
        }

        public class QueryResponse
        {
            public List<Resource> UpdatedResources { get; set; }
            public HttpStatusCode Status { get; set; }
        }

        public class CommandHandler : IRequestHandler<UpdateResources_QueryRequest, QueryResponse>
        {
            private readonly ILogger<UpdateResources> logger;
            private readonly IHttpContextAccessor httpContextAccessor;
            private readonly IPlayerProfileRepository playersRepo;
            private readonly IPlayerProfilesCacheService playerProfilesCacheService;

            public CommandHandler(ILogger<UpdateResources> logger, IHttpContextAccessor httpContextAccessor, IPlayerProfileRepository playersRepo, IPlayerProfilesCacheService playerProfilesCacheService)
            {
                this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
                this.playersRepo = playersRepo ?? throw new ArgumentNullException(nameof(playersRepo));
                this.playerProfilesCacheService = playerProfilesCacheService ?? throw new ArgumentNullException(nameof(playerProfilesCacheService));
            }

            public async Task<QueryResponse> Handle(UpdateResources_QueryRequest request, CancellationToken cancellationToken)
            {
                var response = new QueryResponse();

                try
                {
                    if (!Enum.IsDefined(typeof(ResourceType), request.ResourceType))
                    {
                        response.Status = HttpStatusCode.BadRequest;
                        return response;
                    }

                    var deviceId = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "DeviceId");
                    if (deviceId == null)
                    {
                        logger.LogError($"Strange behaviour, request passed JWT validation, but the DeviceId claim is empty!");
                        response.Status = HttpStatusCode.BadRequest;
                        return response;
                    }

                    // Search for profile in cache
                    var playerProfile = playerProfilesCacheService.GetItem(deviceId.Value);
                    if (playerProfile == null)
                    {
                        // Search in DB
                        playerProfile = await playersRepo.GetItemAsync(deviceId.Value);
                        if (playerProfile == null)
                        {
                            logger.LogError($"Strange behaviour, request passed JWT validation, but the player profile doesn't exists in DB, DeviceId: {deviceId.Value}");
                            response.Status = HttpStatusCode.InternalServerError;
                            return response;
                        }
                    }


                    var resourceToUpdate = playerProfile.Resources.FirstOrDefault(r => r.ResourceType == request.ResourceType);
                    if (resourceToUpdate == null)
                    {
                        resourceToUpdate = new Resource()
                        {
                            ResourceType = request.ResourceType,
                            Value = request.ResourceValue,
                        };

                        playerProfile.Resources.Add(resourceToUpdate);
                    }
                    else
                        resourceToUpdate.Update(request.ResourceValue);

                    await playersRepo.UpdateItemAsync(deviceId.Value, playerProfile);

                    // Add profile to cache
                    playerProfilesCacheService.SetCachedItem(playerProfile.DeviceId, playerProfile.PlayerId, playerProfile);

                    response.UpdatedResources = playerProfile.Resources;
                    response.Status = HttpStatusCode.OK;

                    return response;
                }
                catch (Exception ex)
                {
                    logger.LogError($"Updateting resource {request.ResourceType} to {request.ResourceValue} got an unexpected error: {ex.Message}");
                    response.Status = HttpStatusCode.InternalServerError;
                    return response;
                }
            }
        }
    }
}
