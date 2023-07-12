using GameServer.Models;

namespace GameServerWebSocket.Domain;

public interface ICommandHandlers
{
    Task Handle(IPlayerContext context, Command cmd, CancellationToken cancellationToken);
}