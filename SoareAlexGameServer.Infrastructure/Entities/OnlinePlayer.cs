using Newtonsoft.Json;
using SoareAlexGameServer.Infrastructure.WebSockets;
using System.Net.WebSockets;
using System.Text;

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
                        WebSocketState = WebSocketConnection.State;

                        var buffer = new ArraySegment<byte>(new byte[4096]);
                        var receiveResult = await WebSocketConnection.ReceiveAsync(buffer, CancellationToken.None);

                        if (receiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            WebSocketState = WebSocketState.Closed;

                            await WebSocketConnection.CloseAsync(WebSocketCloseStatus.NormalClosure, "WebSocket closed by the client.", CancellationToken.None);
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

        public async Task SendToWebSocket<T>(WebSocketEventType eventType, T wsEvent)
        {
            if (WebSocketConnection == null || WebSocketState != WebSocketState.Open)
                return;

            var msgToSend = new RawWebSocketEvent<T>(eventType, wsEvent);

            var json = JsonConvert.SerializeObject(msgToSend);

            byte[] eventBytes = Encoding.UTF8.GetBytes(json);

            await WebSocketConnection.SendAsync(new ArraySegment<byte>(eventBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
