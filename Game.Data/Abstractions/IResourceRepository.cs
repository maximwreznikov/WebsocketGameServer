using GameServer.Models;

namespace Game.Data.Abstractions;

public interface IResourceRepository
{
    Task<Resource?> Change(Guid playerId, ResourceType type, int amount,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Resource>> GetResources(Guid playerId, CancellationToken cancellationToken);
}