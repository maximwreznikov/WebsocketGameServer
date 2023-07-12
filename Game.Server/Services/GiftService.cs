using Game.Data.Abstractions;
using GameServer.Models;
using GameServerWebSocket.Domain;
using GameServerWebSocket.Infrastructure;

namespace GameServerWebSocket.Services;

public class GiftService : IGiftService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHub _hub;
    private readonly ILogger<GiftService> _logger;
    
    public GiftService(IPlayerRepository playerRepository, 
        IResourceRepository resourceRepository, 
        IUnitOfWork unitOfWork, 
        IHub hub, 
        ILogger<GiftService> logger) 
    {
        _playerRepository = playerRepository;
        _resourceRepository = resourceRepository;
        _unitOfWork = unitOfWork;
        _hub = hub;
        _logger = logger;
    }

    public async Task SendGift(IPlayerContext context, SendGiftRequest request, CancellationToken cancellationToken)
    {
        if (!context.IsLoggedIn)
        {
            _logger.LogWarning("Unauthorized player on socket {SocketId}", context.SocketId);
            return;
        }
        
        var friend = await _playerRepository.GetOrCreate(request.FriendPlayerUdid, cancellationToken);
        
        var resource = await _resourceRepository.Change(context.PlayerId, 
            request.ResourceType, -request.ResourceValue, cancellationToken);
        
        if (resource == null)
        {
            _logger.LogError("Player {Player} have no such type of resource {Type}", 
                context.PlayerId, request.ResourceType);
            return;
        }

        if (resource.Amount < 0)
        {
            _logger.LogWarning("Player {Player} have no enough resource {Type} {Value}", 
                context.PlayerId, request.ResourceType, request.ResourceValue);
            return;
        }

        await _resourceRepository.Change(friend.Id, request.ResourceType, request.ResourceValue, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);
        
        var response = new SendGiftResponse(context.Udid, request.FriendPlayerUdid, 
            request.ResourceType, request.ResourceValue);
        
        _hub.Send(response, new []{ context.PlayerId, friend.Id});
    }
}