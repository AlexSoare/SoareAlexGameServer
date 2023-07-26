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
using System.Security.Claims;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

var test = builder.Configuration.GetSection("JwtSettings:Issuer").Value;

// Add JWT authentification services
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

// Add loggin services
Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();

// Add controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});
builder.Services.AddMediatR(typeof(Program));

// Add http context accessor for validating JWT tokens for Web socket connections
builder.Services.AddHttpContextAccessor();

// Add DB context
builder.Services.AddDbContext<SqliteDbContext>();

/// Add In Memory caching
/// In a horizontal scaling environment, these services should be changed with Distributed Cache services,
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IOnlinePlayersCacheService, OnlinePlayersInMemoryCacheService>();
builder.Services.AddSingleton<IPlayerProfilesCacheService, PlayerProfilesInMemoryCacheService>();

builder.Services.AddSingleton<IWebSocketService, OnlinePlayersWebSocketsHandler>();
builder.Services.AddScoped<IPlayerProfileRepository, PlayerProfileSqliteRepository>();
builder.Services.AddScoped<IJwtTokenProvider, LocalJwtTokenProvider>(p =>
{
    var securityKey = builder.Configuration.GetSection("JwtSettings:Key")?.Value;
    var issuer = builder.Configuration.GetSection("JwtSettings:Issuer")?.Value;
    var audience = builder.Configuration.GetSection("JwtSettings:Audience")?.Value;

    return new LocalJwtTokenProvider(securityKey, issuer, audience);
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAuthorization();

app.UseWebSockets();

app.MapControllers();

app.MapGet("/", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var token = context.Request.Headers["Authorization"];

        var tokenService = context.RequestServices.GetRequiredService<IJwtTokenProvider>();
        List<Claim> claims;

        if (string.IsNullOrEmpty(token) || !tokenService.ValidateToken(token.ToString(), out claims))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var webSocketService = context.RequestServices.GetRequiredService<IWebSocketService>();
        await webSocketService.HandleWebSocketConnection(context, claims);
    }
});

app.Run();

Log.CloseAndFlush();