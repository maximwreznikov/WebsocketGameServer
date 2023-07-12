using Game.Data.Abstractions;

namespace Game.Data;

internal class UnitOfWork : IUnitOfWork
{
    private readonly GameDataContext _context;
    
    public UnitOfWork(GameDataContext context)
    {
        _context = context;
    }
    
    public Task SaveAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}