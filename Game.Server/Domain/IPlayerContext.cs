using System.Net.WebSockets;
using GameServer.Models;

namespace GameServerWebSocket.Domain;

public interface IPlayerContext : IDisposable
{
    int SocketId { get; }
    Guid PlayerId { get; }
    string Udid { get; }

    bool IsLoggedIn { get; }

    Task Run(int socketId, WebSocket socket, CancellationToken cancellationToken);

    void Login(string udid, Guid playerId);

    void Send(Command message);

    Task CloseContext();
}