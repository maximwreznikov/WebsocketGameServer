using GameServer.Models;
using GameServerWebSocket.Domain;

namespace GameServerWebSocket.Services;

public interface IGiftService
{
    Task SendGift(IPlayerContext context, SendGiftRequest request, CancellationToken cancellationToken);
}