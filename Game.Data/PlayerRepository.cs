using Game.Data.Abstractions;
using GameServer.Models;
using Microsoft.EntityFrameworkCore;

namespace Game.Data;

internal class PlayerRepository : IPlayerRepository
{
    private readonly GameDataContext _context;
    
    public PlayerRepository(GameDataContext context)
    {
        _context = context;
    }
    
    public async Task<Player> GetOrCreate(string udid, CancellationToken cancellationToken)
    {
        var player = await _context.Players
            .FirstOrDefaultAsync(x => x.Udid == udid, cancellationToken);

        if (player == null)
        {
            player = Player.Create(udid);
            _context.Players.Add(player);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return player!;
    }
}