using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using GameServer.Models;

namespace GameServerWebSocket.Domain;

public class PlayerContext : IPlayerContext
{
    private const int CLOSE_SOCKET_TIMEOUT_MS = 2500;
    private const int BROADCAST_TRANSMIT_INTERVAL_MS = 5;
    
    private readonly CancellationTokenSource _sendingLoopTokenSource = new CancellationTokenSource();
    private readonly ConcurrentQueue<string> _sendQueue = new ();

    private readonly ICommandHandlers _handlers;
    private readonly ILogger<PlayerContext> _logger;
    
    private WebSocket? _socket;
    public int SocketId { get; private set;  }
    public Guid PlayerId { get; private set; }
    public string Udid { get; private set; } = string.Empty;

    public bool IsLoggedIn => !string.IsNullOrEmpty(Udid);

    public PlayerContext(ICommandHandlers handlers, ILogger<PlayerContext> logger)
    {
        _handlers = handlers;
        _logger = logger;
    }

    public async Task CloseContext()
    {
        _logger.LogInformation("Closing _socket {SocketId}", SocketId);
        
        _logger.LogInformation("Ending broadcast loop");
        _sendingLoopTokenSource.Cancel();

        if (_socket == null)
        {
            _logger.LogError("Impossible to run {Function} due null of {Variable}", nameof(CloseContext), nameof(_socket));
            return;
        }
        
        if (_socket.State != WebSocketState.Open)
        {
            _logger.LogInformation("Socket not open, state = {SocketState}", _socket?.State);
        }
        else
        {
            var timeout = new CancellationTokenSource(CLOSE_SOCKET_TIMEOUT_MS);
            try
            {
                _logger.LogInformation("Starting close handshake");
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeout.Token);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Exception {Name}: {Error}", ex.GetType().Name, ex.Message);
                // normal upon task/token cancellation, disregard
            }
        }
    }

    public Task Run(int socketId, WebSocket socket, CancellationToken cancellationToken)
    {
        SocketId = socketId;
        _socket = socket ?? throw new ArgumentNullException(nameof(socket));
        return Task.WhenAll(
            ReceiveLoopAsync(cancellationToken), 
            SendLoopAsync(CancellationTokenSource.CreateLinkedTokenSource(_sendingLoopTokenSource.Token, cancellationToken).Token));
    }
    
    public void Login(string udid, Guid playerId)
    {
        Udid = udid;
        PlayerId = playerId;
    }
    
    public void Send(Command message)
    {
        _sendQueue.Enqueue(CommandSerializer.Pack(message));
    }
    
    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        if (_socket == null)
        {
            _logger.LogError("Impossible to run {Function} due empty {Variable}", nameof(ReceiveLoopAsync), nameof(_socket));
            return;
        }
        
        var loopToken = cancellationToken;
        var broadcastTokenSource = _sendingLoopTokenSource; // store a copy for use in finally block
        try
        {
            var buffer = WebSocket.CreateServerBuffer(Settings.BUFFER_SIZE);
            while (_socket.State != WebSocketState.Closed 
                   && _socket.State != WebSocketState.Aborted 
                   && !loopToken.IsCancellationRequested)
            {
                var receiveResult = await _socket.ReceiveAsync(buffer, loopToken);

                // the client is notifying us that the connection will close; send acknowledgement
                if (_socket.State == WebSocketState.CloseReceived 
                    && receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("Socket {SocketId}: Acknowledging Close frame received from client", SocketId);
                    broadcastTokenSource.Cancel();
                    await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Acknowledge Close frame", loopToken);
                    // the socket state changes to closed at this point
                }
                else
                {
                    if (receiveResult.Count == 0 || buffer.Array == null)
                    {
                        continue;
                    }

                    _logger.LogInformation(
                        "Socket {SocketId}: Received {ReceiveResultMessageType} frame ({ReceiveResultCount} bytes).",
                        SocketId, receiveResult.MessageType, receiveResult.Count);
                    var command = CommandSerializer.Unpack(buffer.Array.Take(receiveResult.Count).ToArray());
                    _logger.LogInformation("Get message {Command}", command);
                    await HandleCommand(command, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // normal upon task/token cancellation, disregard
            _logger.LogInformation("Canceled token {SocketId}", SocketId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Socket {SocketId}: {Error}", SocketId, ex.Message);
        }
        finally
        {
            broadcastTokenSource.Cancel();

            _logger.LogInformation("Socket {SocketId}: Ended processing loop in state {SocketState}", SocketId, _socket.State);

            // don't leave the socket in any potentially connected state
            if (_socket.State != WebSocketState.Closed)
            {
                _socket.Abort();
            }
        }
    }

    private async Task SendLoopAsync(CancellationToken cancellationToken)
    {
        if (_socket == null)
        {
            _logger.LogError("Impossible to run {Function} due empty {Variable}", nameof(SendLoopAsync), nameof(_socket));
            return;
        }
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(BROADCAST_TRANSMIT_INTERVAL_MS, cancellationToken);
                if (!cancellationToken.IsCancellationRequested 
                    && _socket.State == WebSocketState.Open 
                    && _sendQueue.TryDequeue(out var message))
                {
                    _logger.LogInformation("Socket {SocketId}: Sending from queue.", SocketId);
                    var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                    await _socket.SendAsync(buffer, WebSocketMessageType.Text, endOfMessage: true, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // normal upon task/token cancellation, disregard
                _logger.LogInformation("{SocketId} stopped gracefully", SocketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Got error {Name}: {Error}", ex.GetType().Name, ex.Message);
            }
        }
    }
    
    private async Task HandleCommand(Command command, CancellationToken cancellationToken)
    {
        try
        {
            await _handlers.Handle(this, command, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Got error on handle with command {Command}", command);
        }
    }

    public void Dispose()
    {
        _socket?.Dispose();
        _sendingLoopTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }
}