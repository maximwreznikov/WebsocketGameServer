using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Game.Data;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal class GameDataContextFactory : IDesignTimeDbContextFactory<GameDataContext>
{
    public GameDataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GameDataContext>();
        optionsBuilder.UseSqlite("Data Source=LocalDatabase.db");

        return new GameDataContext(optionsBuilder.Options);
    }

    public static void Migrate(IDbContextFactory<GameDataContext> factory)
    {
        using var context = factory.CreateDbContext();
        context.Database.Migrate();
    }
}