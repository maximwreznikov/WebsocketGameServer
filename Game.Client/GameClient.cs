using System.Text;
using GameServer.Models;
using Serilog;

namespace Game.Client;

public class GameClient : IDisposable, IServerEvents
{
    private readonly WsClient _ws;
    private readonly ILogger _logger;
    
    public GameClient(WsClient ws, ILogger logger)
    {
        _ws = ws;
        _logger = logger;
    }
    
    public async Task Login(string udid, CancellationToken cancellationToken)
    {
        var request = new LoginRequest(udid);
        await Send(request, cancellationToken);
        _logger.Information(nameof(Login));
    }

    public async Task Update(ResourceType type, int value, CancellationToken cancellationToken)
    {
        var request = new UpdateResourcesRequest(type, value);
        await Send(request, cancellationToken);
        _logger.Information(nameof(Update));
    }

    public async Task SendGift(string friendUdid, ResourceType type, int value, CancellationToken cancellationToken)
    {
        var request = new SendGiftRequest(friendUdid, type, value);
        await Send(request, cancellationToken);
        _logger.Information(nameof(SendGift));
    }
    
    public async Task Send<T>(T request, CancellationToken cancellationToken) where T : Command
    {
        var msg = CommandSerializer.Pack(request);
        await _ws.Send(Encoding.UTF8.GetBytes(msg), cancellationToken);
    }

    public Task Close(CancellationToken cancellationToken)
    {
        return _ws.Close(cancellationToken);
    }

    public void Dispose()
    {
        _ws.Dispose();
        GC.SuppressFinalize(this);
    }
}