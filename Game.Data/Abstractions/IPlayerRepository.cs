namespace Game.Data.Abstractions;

public interface IPlayerRepository
{
    Task<Player> GetOrCreate(string udid, CancellationToken cancellationToken);
}