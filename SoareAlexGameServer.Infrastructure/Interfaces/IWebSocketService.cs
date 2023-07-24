using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SoareAlexGameServer.Infrastructure.Interfaces
{
    public interface IWebSocketService
    {
        Task HandleWebSocketConnection(HttpContext context, WebSocket webSocket);
    }
}
