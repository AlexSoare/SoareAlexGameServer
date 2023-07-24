using SoareAlexGameServer.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SoareAlexGameServer.Infrastructure.Entities
{
    public class OnlinePlayer
    {
        public string PlayerId { get; set; }

        public WebSocketState WebSocketState { get; set; } = WebSocketState.Closed;
        public WebSocket WebSocketConnection { get; set; }

        public async Task ListenToWebSocket(WebSocket webSocket)
        {
            WebSocketConnection = webSocket;

            try
            {
                var receiveTask = Task.Run(async () =>
                {
                    while (WebSocketConnection.State == WebSocketState.Open)
                    {
                        WebSocketState = WebSocketConnection.State;

                        var buffer = new ArraySegment<byte>(new byte[4096]);
                        var receiveResult = await WebSocketConnection.ReceiveAsync(buffer, CancellationToken.None);

                        if (receiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            WebSocketState = WebSocketState.Closed;
                            // Handle WebSocket closure request from the client
                            await WebSocketConnection.CloseAsync(WebSocketCloseStatus.NormalClosure, "WebSocket closed by the client.", CancellationToken.None);
                        }
                        else
                        {
                            // Handle the received message and respond to the client
                            // Here, you can add your custom message processing logic
                            // and respond to the client if needed
                        }
                    }
                });
                await receiveTask;
            }
            catch (WebSocketException)
            {
                // Handle any WebSocket-related exceptions
            }
        }

        public async Task SendToWebSocket(WebSocketMessage msg)
        {
            if (WebSocketConnection == null || WebSocketState != WebSocketState.Open)
                return;

            byte[] messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
            await WebSocketConnection.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
