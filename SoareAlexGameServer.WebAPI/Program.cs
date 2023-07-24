using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SoareAlexGameServer.Infrastructure.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SoareAlexGameServer.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using SoareAlexGameServer.WebAPI.Swagger;
using SoareAlexGameServer.Infrastructure.Interfaces.Cache;
using SoareAlexGameServer.Infrastructure.Services.Cache;
using SoareAlexGameServer.Infrastructure.Services.Repositories;
using SoareAlexGameServer.Infrastructure.Interfaces.Repositories;
using SoareAlexGameServer.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidIssuer = builder.Configuration.GetSection("JwtSettings:Issuer").Value,
        ValidAudience = builder.Configuration.GetSection("JwtSettings:Audience").Value,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JwtSettings:Key").Value!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
    };
});
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<SqliteDbContext>();
builder.Services.AddMediatR(typeof(Program));
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IOnlinePlayersCacheService, OnlinePlayersInMemoryCacheService>();
//builder.Services.AddSingleton<IPlayerProfilesCacheService, PlayerProfilesInMemoryCacheService>();

builder.Services.AddScoped<IPlayerProfileRepository, PlayerProfileSqliteRepository>();
builder.Services.AddScoped<IJwtTokenProvider, LocalJwtTokenProvider>(p =>
{
    var securityKey = builder.Configuration.GetSection("JwtSettings:Key")?.Value;
    var issuer = builder.Configuration.GetSection("JwtSettings:Issuer")?.Value;
    var audience = builder.Configuration.GetSection("JwtSettings:Audience")?.Value;

    return new LocalJwtTokenProvider(securityKey, issuer, audience);
});
builder.Services.AddScoped<IWebSocketService, OnlinePlayersWebSocketsHandler>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAuthorization();

app.MapControllers();

app.UseWebSockets();
app.MapGet("/", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var webSocketService = context.RequestServices.GetRequiredService<IWebSocketService>();
        await webSocketService.HandleWebSocketConnection(context, webSocket);
    }
});

app.Run();