using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SoareAlexGameServer.Infrastructure.Entities;
using SoareAlexGameServer.Infrastructure.Interfaces;
using SoareAlexGameServer.Infrastructure.Interfaces.Cache;
using SoareAlexGameServer.Infrastructure.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace SoareAlexGameServer.Infrastructure.Services
{
    public enum WebSocketEvent
    {
        GiftEvent
    }

    public class WebSocketMessage
    {
        public WebSocketEvent Event { get; set; }
        public string Message { get; set; }
    }

    public class OnlinePlayersWebSocketsHandler : IWebSocketService
    {
        private readonly IOnlinePlayersCacheService onlinePlayersCacheService;

        public OnlinePlayersWebSocketsHandler(IOnlinePlayersCacheService onlinePlayersCacheService)
        {
            this.onlinePlayersCacheService = onlinePlayersCacheService ?? throw new ArgumentNullException(nameof(onlinePlayersCacheService));
        }

        public async Task HandleWebSocketConnection(HttpContext context, List<Claim> claims)
        {
            var deviceIdClaim = claims.FirstOrDefault(c => c.Type == "DeviceId");
            var deviceId = deviceIdClaim.Value;

            var onlinePlayer = onlinePlayersCacheService.GetItem(deviceId);

            // TO DELETE
            if (onlinePlayer == null)
            {
                var webSocketConnection = await context.WebSockets.AcceptWebSocketAsync();

                onlinePlayer = new OnlinePlayer()
                {
                    WebSocketConnection = webSocketConnection
                };

                await onlinePlayer.ListenToWebSocket(webSocketConnection);

                onlinePlayersCacheService.SetCachedItem(deviceId, onlinePlayer);
            }

            //if (onlinePlayer == null)
            //{
            //    context.Response.StatusCode = StatusCodes.Status404NotFound;
            //    await context.Response.WriteAsync("Player not found!");
            //    return;
            //}

            //if (onlinePlayer.WebSocketState != WebSocketState.Open)
            //{
            //    context.Response.StatusCode = StatusCodes.Status200OK;

            //    var webSocketConnection = await context.WebSockets.AcceptWebSocketAsync();
            //    await onlinePlayer.ListenToWebSocket(webSocketConnection);
            //}
            //else
            //{
            //    context.Response.StatusCode = StatusCodes.Status409Conflict;
            //    return;
            //}
        }
    }
}
