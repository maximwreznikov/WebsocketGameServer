namespace GameServer.Models;

public interface IServerEvents
{
    Task Login(string udid, CancellationToken cancellationToken);

    Task Update(ResourceType type, int value, CancellationToken cancellationToken);

    Task SendGift(string friendUdid, ResourceType type, int value, CancellationToken cancellationToken);
}