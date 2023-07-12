using System.Net;
using System.Net.WebSockets;
using Game.Data;
using GameServerWebSocket;
using GameServerWebSocket.Domain;
using GameServerWebSocket.Infrastructure;
using GameServerWebSocket.Services;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting game server");

    var builder = WebApplication.CreateBuilder(args);
    
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.Limits.MaxConcurrentConnections = 10000;
        serverOptions.Limits.MaxConcurrentUpgradedConnections = 10000;
    });
    
    // setup serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());
    
    builder.Services.AddDataLayer(builder.Configuration);

    builder.Services.AddScoped<IPlayerContext, PlayerContext>();
    builder.Services.AddScoped<IPlayerService, PlayerService>();
    builder.Services.AddScoped<IResourcesService, ResourcesService>();
    builder.Services.AddScoped<IGiftService, GiftService>();
    builder.Services.AddScoped<ICommandHandlers, CommandHandlers>();
    
    builder.Services.AddSingleton<ICommandFactory, CommandFactory>();
    builder.Services.AddSingleton<IHub, Hub>();
    builder.Services.AddSingleton<HubMiddleware>();

    var app = builder.Build();
    
    app.Services.Migrate();

    app.UseSerilogRequestLogging();

    app.UseWebSockets(new WebSocketOptions
    {
        KeepAliveInterval = TimeSpan.FromSeconds(60)
        
    });

    app.UseMiddleware<HubMiddleware>();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Game server terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}





