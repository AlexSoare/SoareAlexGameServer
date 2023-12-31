﻿using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SoareAlexGameServer.Infrastructure.Interfaces
{
    public interface IWebSocketService
    {
        Task HandleWebSocketConnection(HttpContext context, List<Claim> claims);
    }
}
