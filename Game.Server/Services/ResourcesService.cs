using Game.Data.Abstractions;
using GameServer.Models;
using GameServerWebSocket.Domain;

namespace GameServerWebSocket.Services;

public class ResourcesService : IResourcesService
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResourcesService> _logger;
    
    public ResourcesService(IResourceRepository resourceRepository, 
        IUnitOfWork unitOfWork,
        ILogger<ResourcesService> logger)
    {
        _resourceRepository = resourceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ChangeAmount(IPlayerContext context, UpdateResourcesRequest request, CancellationToken cancellationToken)
    {
        if (!context.IsLoggedIn)
        {
            _logger.LogWarning("Unauthorized player on socket {SocketId}", context.SocketId);
            return;
        }
        
        await _resourceRepository.Change(context.PlayerId, request.Type, request.Value, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);
        
        var resourcesData = await _resourceRepository.GetResources(context.PlayerId, cancellationToken);
        
        var resources = resourcesData
            .Select(x => new ResourceResponse(x.ResourceType, x.Amount));
        context.Send(new ResourcesResponse(resources.ToArray()));
    }
}