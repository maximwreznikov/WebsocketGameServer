namespace Game.Data.Abstractions;

public interface IUnitOfWork
{
    public Task SaveAsync(CancellationToken cancellationToken);
}