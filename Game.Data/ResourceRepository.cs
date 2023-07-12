using Game.Data.Abstractions;
using GameServer.Models;
using Microsoft.EntityFrameworkCore;

namespace Game.Data;

internal class ResourceRepository : IResourceRepository
{
    private readonly GameDataContext _context;
    
    public ResourceRepository(GameDataContext context)
    {
        _context = context;
    }

    public async Task<Resource?> Change(Guid playerId, ResourceType type, int amount,
        CancellationToken cancellationToken)
    {
        var resource = await _context.Resources
            .FirstOrDefaultAsync(x => x.PlayerId == playerId 
                                      && x.ResourceType == type, cancellationToken);

        if (resource == null)
        {
            return null;
        }
        
        if (!resource.Change(amount))
        {
            return new Resource
            {
                PlayerId = resource.PlayerId,
                Amount = resource.Amount + amount,
                ResourceType = resource.ResourceType
            };
        }

        return resource;
    }

    public async Task<IReadOnlyList<Resource>> GetResources(Guid playerId, CancellationToken cancellationToken)
    {
        var resources = await _context.Resources
            .Where(x => x.PlayerId == playerId)
            .ToListAsync(cancellationToken);

        return resources;
    }
}