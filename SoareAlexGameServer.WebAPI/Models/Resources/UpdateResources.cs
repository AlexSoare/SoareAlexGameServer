using MediatR;
using System.Net;
using SoareAlexGameServer.Infrastructure.Entities.DB;
using SoareAlexGameServer.Infrastructure.Interfaces.Repositories;
using SoareAlexGameServer.Infrastructure.Interfaces.Cache;

namespace SoareAlexGameServer.WebAPI.Models.Resources
{
    public class UpdateResources
    {
        public class QueryRequest : IRequest<QueryResponse>
        {
            public ResourceType ResourceType { get; set; }
            public double ResourceValue { get; set; }
        }

        public class QueryResponse
        {
            public double UpdatedResourceValue { get; set; }
            public HttpStatusCode Status { get; set; }
        }

        public class CommandHandler : IRequestHandler<QueryRequest, QueryResponse>
        {
            private IHttpContextAccessor httpContextAccessor;

            private IPlayerProfileRepository playersRepo;

            public CommandHandler(IHttpContextAccessor httpContextAccessor, IPlayerProfileRepository playersRepo)
            {
                this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
                this.playersRepo = playersRepo ?? throw new ArgumentNullException(nameof(playersRepo));
            }

            public async Task<QueryResponse> Handle(QueryRequest request, CancellationToken cancellationToken)
            {
                var response = new QueryResponse();

                var deviceId = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "DeviceId");
                if (deviceId == null)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                var playerProfile = await playersRepo.GetItemAsync(deviceId.Value);
                if (playerProfile == null)
                {
                    response.Status = HttpStatusCode.NotFound;
                    return response;
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

                response.UpdatedResourceValue = resourceToUpdate.Value;
                response.Status = HttpStatusCode.OK;

                return response;
            }
        }
    }
}
