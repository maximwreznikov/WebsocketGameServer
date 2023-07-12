using System.Net;
using System.Net.WebSockets;
using GameServerWebSocket.Infrastructure;

namespace GameServerWebSocket;

public class HubMiddleware : IMiddleware
{
    private const string Path = "/ws";
    
    private readonly CancellationTokenRegistration _shutdownHandler;
    private readonly IHub _hub;
    
    public HubMiddleware(IHub hub, IHostApplicationLifetime hostLifetime)
    {
        _hub = hub;
        // gracefully close all websockets during shutdown (only register on first instantiation)
        if (_shutdownHandler.Token.Equals(CancellationToken.None))
        {
            _shutdownHandler = hostLifetime.ApplicationStopping.Register(ApplicationShutdownHandler);
        }
    }
    
    /// <summary>
    /// Event-handlers are the sole case where async void is valid
    /// </summary>
    public async void ApplicationShutdownHandler()
    {
        await _hub.Close();
    }
    
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path.StartsWithSegments(Path))
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                await _hub.AcceptSocket(context, context.RequestAborted);
            }
            else
            {
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
            }
        }
        else
        {
            await next(context);
        }
    }
}