using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SoareAlexGameServer.Infrastructure.WebSockets
{
    public enum WebSocketEventType
    {
        Gift
    }

    public class RawWebSocketEvent<T>
    {
        public string Type { get; set; }
        public string Event { get; set; }

        public RawWebSocketEvent(WebSocketEventType type, T Event)
        {
            Type = type.ToString();
            this.Event = JsonConvert.SerializeObject(Event);
        }
    }
}
