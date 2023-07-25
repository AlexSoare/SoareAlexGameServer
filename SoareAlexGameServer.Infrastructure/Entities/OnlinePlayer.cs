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
        public string DeviceId { get; set; }
        public string PlayerId { get; set; }

        public WebSocketState WebSocketState { get; set; } = WebSocketState.Closed;

        private WebSocket WebSocketConnection { get; set; }
        private Action<OnlinePlayer> webSocketconnectionClosedCallback;

        public async Task ListenToWebSocket(WebSocket webSocket, Action<OnlinePlayer> onCloseCallback)
        {
            WebSocketConnection = webSocket;
            webSocketconnectionClosedCallback = onCloseCallback;

            try
            {
                var receiveTask = Task.Run(async () =>
                {
                    while (WebSocketConnection.State == WebSocketState.Open)
                    {
                        try
                        {
                            WebSocketState = WebSocketConnection.State;

                            var buffer = new ArraySegment<byte>(new byte[4096]);
                            var receiveResult = await WebSocketConnection.ReceiveAsync(buffer, CancellationToken.None);

                            if (receiveResult.MessageType == WebSocketMessageType.Close)
                            {
                                WebSocketState = WebSocketState.Closed;
                                // Handle WebSocket closure request from the client
                                await WebSocketConnection.CloseAsync(WebSocketCloseStatus.NormalClosure, "WebSocket closed by the client.", CancellationToken.None);
                                webSocketconnectionClosedCallback?.Invoke(this);
                            }
                        }catch (WebSocketException ex)
                        {
                            webSocketconnectionClosedCallback?.Invoke(this);
                        }
                    }
                });
                await receiveTask;
            }
            catch (WebSocketException)
            {
                webSocketconnectionClosedCallback?.Invoke(this);
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
