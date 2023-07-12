using GameServer.Models;
using GameServerWebSocket.Domain;

namespace GameServerWebSocket.Services;

public interface IPlayerService
{
    Task Login(IPlayerContext context, LoginRequest request, CancellationToken cancellationToken);
}