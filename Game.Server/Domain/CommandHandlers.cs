using GameServer.Models;
using GameServerWebSocket.Infrastructure;
using GameServerWebSocket.Services;

namespace GameServerWebSocket.Domain;

public class CommandHandlers : ICommandHandlers
{
    private readonly IPlayerService _playerService;
    private readonly IResourcesService _resourcesService;
    private readonly IGiftService _giftService;
    private readonly ICommandFactory _commandFactory;

    public CommandHandlers(ICommandFactory commandFactory,
        IPlayerService playerService, 
        IResourcesService resourcesService,
        IGiftService giftService)
    {
        _playerService = playerService;
        _resourcesService = resourcesService;
        _giftService = giftService;
        _commandFactory = commandFactory;

        commandFactory.Register<LoginRequest>(_playerService.Login);
        commandFactory.Register<UpdateResourcesRequest>(_resourcesService.ChangeAmount);
        commandFactory.Register<SendGiftRequest>(_giftService.SendGift);
    }

    public Task Handle(IPlayerContext context, Command cmd, CancellationToken cancellationToken)
    {
        return _commandFactory.Handle(context, cmd, cancellationToken);
    }
}