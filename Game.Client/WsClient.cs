using System.Net;
using System.Net.WebSockets;
using System.Text;
using GameServer.Models;
using Serilog;

namespace Game.Client;

public class WsClient : IDisposable
{
    private readonly ClientWebSocket _ws;
    private readonly ILogger _logger;

    public WsClient(ILogger logger)
    {
        _ws = new();
        _ws.Options.HttpVersion = HttpVersion.Version20;
        _ws.Options.HttpVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
        _logger = logger;
    }
    
    public async Task Open(Uri uri, CancellationToken cancellationToken)
    {
        var handler = new HttpClientHandler();
        var invoker = new HttpMessageInvoker(handler);
        await _ws.ConnectAsync(uri, invoker, cancellationToken);
    }

    public async Task Close(CancellationToken cancellationToken)
    {
        if (_ws.State == WebSocketState.Aborted)
        {
            return;
        }
        
        await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", cancellationToken);
    }

    public async Task Send(byte[] bytes, CancellationToken cancellationToken)
    {
        await _ws.SendAsync(bytes, WebSocketMessageType.Binary, WebSocketMessageFlags.EndOfMessage, cancellationToken);
    }

    public Task RunLoop(CancellationToken cancellationToken)
    {
        return Task.Run(() => ReceiveLoopAsync(cancellationToken), cancellationToken);
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = WebSocket.CreateClientBuffer(Settings.BUFFER_SIZE, Settings.BUFFER_SIZE);
        while (_ws.State != WebSocketState.Closed
               && _ws.State != WebSocketState.Aborted
               && !cancellationToken.IsCancellationRequested)
        {
            var receiveResult = await _ws.ReceiveAsync(buffer, cancellationToken);
            if (receiveResult.Count == 0 || buffer.Array == null)
            {
                continue;
            }

            var data = Encoding.UTF8.GetString(buffer.Array.Take(receiveResult.Count).ToArray());
            _logger.Information("Get from socket: {data}", data);
        }
    }

    public void Dispose()
    {
        _ws.Dispose();
        GC.SuppressFinalize(this);
    }
}