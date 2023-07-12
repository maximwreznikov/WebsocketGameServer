using GameServer.Models;

namespace GameServerWebSocket.Infrastructure;

public interface IHub
{
    Task AcceptSocket(HttpContext context, CancellationToken cancellationToken);

    Task Close();

    void Send(Command message, Guid[] playerIds);
}