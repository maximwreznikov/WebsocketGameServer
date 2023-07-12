using Game.Data.Abstractions;
using GameServer.Models;
using GameServerWebSocket.Domain;

namespace GameServerWebSocket.Services;

public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<PlayerService> _logger;
    
    public PlayerService(IPlayerRepository playerRepository, 
        ILogger<PlayerService> logger)
    {
        _playerRepository = playerRepository;
        _logger = logger;
    }

    public async Task Login(IPlayerContext context, LoginRequest request, CancellationToken cancellationToken)
    {
        if (context!.IsLoggedIn)
        {
            context.Send(new LoginResponse(false, context.PlayerId, context.Udid));
            _logger.LogWarning("Already login {Player} with udid {Udid}", context.PlayerId, context.Udid);
            return;
        }
        
        var player = await _playerRepository.GetOrCreate(request.DeviceId, cancellationToken);
        context.Login(player.Udid, player.Id);
        context.Send(new LoginResponse(true, player.Id, player.Udid));
    }
}