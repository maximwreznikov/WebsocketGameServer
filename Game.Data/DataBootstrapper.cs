using Game.Data.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Data;

public static class DataBootstrapper
{
    public static void AddDataLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // setup database
        services.AddDbContextFactory<GameDataContext>(opt =>
            opt.UseSqlite(configuration.GetConnectionString(nameof(GameDataContext))));
        
        
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    public static void Migrate(this IServiceProvider services)
    {
        GameDataContextFactory.Migrate(services.GetRequiredService<IDbContextFactory<GameDataContext>>());

    }
}