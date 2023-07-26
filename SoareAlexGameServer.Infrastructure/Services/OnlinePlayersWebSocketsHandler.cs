using SoareAlexGameServer.Infrastructure.Entities;
using SoareAlexGameServer.Infrastructure.Interfaces.Cache;
using System.Net.WebSockets;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Security.AccessControl;
using SoareAlexGameServer.Infrastructure.Interfaces;

namespace SoareAlexGameServer.Infrastructure.Services
{
    public class OnlinePlayersWebSocketsHandler : IWebSocketService
    {
        private readonly ILogger<OnlinePlayersWebSocketsHandler> logger;
        private readonly IOnlinePlayersCacheService onlinePlayersCacheService;


        public OnlinePlayersWebSocketsHandler(ILogger<OnlinePlayersWebSocketsHandler> logger, IOnlinePlayersCacheService onlinePlayersCacheService)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.onlinePlayersCacheService = onlinePlayersCacheService ?? throw new ArgumentNullException(nameof(onlinePlayersCacheService));
        }

        public async Task HandleWebSocketConnection(HttpContext context, List<Claim> claims)
        {
            try
            {
                var deviceIdClaim = claims.FirstOrDefault(c => c.Type == "DeviceId");
                var deviceId = deviceIdClaim.Value;

                var onlinePlayer = onlinePlayersCacheService.GetItem(deviceId);

                if (onlinePlayer == null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Player not found!");
                    return;
                }

                if (onlinePlayer.WebSocketState != WebSocketState.Open)
                {
                    context.Response.StatusCode = StatusCodes.Status200OK;

                    var webSocketConnection = await context.WebSockets.AcceptWebSocketAsync();
                    await onlinePlayer.ListenToWebSocket(webSocketConnection, HandleWebSocketConnectioClosed);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    return;
                }

            }
            catch (WebSocketException ex)
            {
                logger.LogError($"Incoming web socket connection failed with error: {ex.Message}");
            }
        }

        private void HandleWebSocketConnectioClosed(OnlinePlayer forPlayer)
        {
            onlinePlayersCacheService.DeleteItem(forPlayer.DeviceId);
        }
    }
}
