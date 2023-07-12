using GameServer.Models;
using GameServerWebSocket.Domain;

namespace GameServerWebSocket.Infrastructure;

public interface ICommandFactory
{
    void Register<T>(Func<IPlayerContext, T, CancellationToken, Task> handler) where T : Command;
    
    void Register(Type type, CommandHandler handler);

    Task Handle(IPlayerContext context, Command cmd, CancellationToken cancellationToken);
}