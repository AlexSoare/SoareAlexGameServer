using MediatR;
using System.Net;
using SoareAlexGameServer.Infrastructure.Entities.DB;
using SoareAlexGameServer.Infrastructure.Interfaces.Repositories;
using SoareAlexGameServer.Infrastructure.Interfaces.Cache;

namespace SoareAlexGameServer.WebAPI.Models.Resources
{
    public class GetResource
    {
        public class QueryRequest : IRequest<QueryResponse>
        {
            public ResourceType ResourceType { get; set; }
        }

        public class QueryResponse
        {
            public double ResourceValue { get; set; }
            public HttpStatusCode Status { get; set; }
        }

        public class CommandHandler : IRequestHandler<QueryRequest, QueryResponse>
        {
            private IHttpContextAccessor httpContext;
            private IPlayerProfileRepository playersRepo;

            public CommandHandler(IHttpContextAccessor httpContext, IPlayerProfileRepository playersRepo)
            {
                this.httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
                this.playersRepo = playersRepo ?? throw new ArgumentNullException(nameof(playersRepo));
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

                var playerProfile = await playersRepo.GetItemAsync(deviceId.Value);
                if (playerProfile == null)
                {
                    response.Status = HttpStatusCode.InternalServerError;
                    return response;
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
        }
    }
}
