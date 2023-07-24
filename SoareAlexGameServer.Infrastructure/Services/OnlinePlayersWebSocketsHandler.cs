using Microsoft.AspNetCore.Http;
using SoareAlexGameServer.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SoareAlexGameServer.Infrastructure.Services
{
    public class OnlinePlayersWebSocketsHandler : IWebSocketService
    {
        private WebSocket webSocket;

        public async Task HandleWebSocketConnection(HttpContext context, WebSocket webSocket)
        {
            var queryParameters = context.Request.Query;

            var playerId = queryParameters.FirstOrDefault(p => p.Key == "playerId");
            var value = playerId.Value.ToString();

            this.webSocket = webSocket;
            // Your WebSocket handling logic goes here.
            byte[] buffer = new byte[1024];

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    // Handle incoming text message.
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var nr = int.Parse(message);
                    nr++;
                    Console.WriteLine(nr);
                    await SendTextMessage(nr.ToString());
                    // Your custom handling logic for the received message.
                    // For example, you can broadcast the message to all connected clients.
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    // Handle WebSocket close message.
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
            }
        }

        public async Task SendTextMessage(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
