using GameServer.Models;
using GameServerWebSocket.Domain;

namespace GameServerWebSocket.Services;

public interface IResourcesService
{
    Task ChangeAmount(IPlayerContext context, UpdateResourcesRequest request, CancellationToken cancellationToken);
}