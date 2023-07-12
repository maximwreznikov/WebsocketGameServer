using GameServer.Models;
using GameServerWebSocket.Domain;

namespace GameServerWebSocket.Infrastructure;

public class CommandFactory : ICommandFactory
{
    private readonly Dictionary<Type, CommandHandler> _handlers;

    private readonly ILogger<ICommandFactory> _logger;
    

    public CommandFactory(ILogger<CommandFactory> logger)
    {
        _logger = logger;
        
        _handlers = new Dictionary<Type, CommandHandler>();
    }
    
    public void Register<T>(Func<IPlayerContext, T, CancellationToken, Task> handler) where T: Command
    {
        _handlers[typeof(T)] = (IPlayerContext context, Command cmd, CancellationToken cancellationToken) 
            => handler.Invoke(context, (T)cmd, cancellationToken);
    }

    public void Register(Type type, CommandHandler handler)
    {
        _handlers[type] = handler;
    }

    public Task Handle(IPlayerContext context, Command cmd, CancellationToken cancellationToken)
    {
        var type = cmd.GetType();
        if (_handlers.TryGetValue(type, out var handler))
        {
            return handler.Invoke(context, cmd, cancellationToken);
        }
        
        _logger.LogWarning("Unknown handler for {Type}", type);
        
        return Task.CompletedTask;
    }
}