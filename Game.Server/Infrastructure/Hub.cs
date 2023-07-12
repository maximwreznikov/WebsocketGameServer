using System.Collections.Concurrent;
using GameServer.Models;
using GameServerWebSocket.Domain;

namespace GameServerWebSocket.Infrastructure;

public class Hub : IHub
{
    // The key is a socket id
    private readonly ConcurrentDictionary<int, IPlayerContext> _clients = new ();
    
    private int _socketCounter = 0;
    
    private readonly CancellationTokenSource _hubTokenSource = new CancellationTokenSource();
    
    private readonly ICommandFactory _commandFactory;
    private readonly ILogger<Hub> _logger;
    
    public Hub(ICommandFactory commandFactory, ILogger<Hub> logger)
    {
        _commandFactory = commandFactory;
        _logger = logger;
    }

    public async Task AcceptSocket(HttpContext context, CancellationToken cancellationToken)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var socketId = Interlocked.Increment(ref _socketCounter);
        _logger.LogInformation("Socket {SocketId}: New connection.", socketId);
        var hubContext = context.RequestServices.GetRequiredService<IPlayerContext>();
        _clients.TryAdd(socketId, hubContext);
        await hubContext.Run(socketId, webSocket, 
            CancellationTokenSource.CreateLinkedTokenSource(_hubTokenSource.Token, cancellationToken).Token);
        // by this point the socket is closed or aborted, the ConnectedClient object is useless
        if (_clients.TryRemove(socketId, out hubContext))
        {
            hubContext.Dispose();
        }
        _logger.LogInformation("Disconnect {SocketId}", socketId);
    }
    
    public async Task Close()
    {
        // We can't dispose the sockets until the processing loops are terminated,
        // but terminating the loops will abort the sockets, preventing graceful closing.
        var disposeQueue = new List<IPlayerContext>(_clients.Count);
        
        while (!_clients.IsEmpty)
        {
            var client = _clients.ElementAt(0).Value;

            await client.CloseContext();
            if (_clients.TryRemove(client.SocketId, out _))
            {
                // only safe to Dispose once, so only add it if this loop can't process it again
                disposeQueue.Add(client);
            }
        
            _logger.LogInformation("Closed Socket {SocketId}", client.SocketId);
        }
        
        // now that they're all closed, terminate the blocking ReceiveAsync calls in the SocketProcessingLoop threads
        _hubTokenSource.Cancel();
        
        // dispose all resources
        foreach (var socket in disposeQueue)
        {
            socket.Dispose();
        } 
    }

    public void Send(Command message, Guid[] playerIds)
    {
        var clients = _clients.Values
            .Where(x => playerIds.Contains(x.PlayerId))
            .ToList();
        
        foreach (var client in clients)
        {
            client.Send(message);
        }
    }
}