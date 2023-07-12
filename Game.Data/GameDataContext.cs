using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

[assembly: InternalsVisibleTo("Game.Tests")]
namespace Game.Data;

internal class GameDataContext : DbContext
{
    public DbSet<Player> Players { get; set; }
    public DbSet<Resource> Resources { get; set; }

    public GameDataContext(DbContextOptions options): base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSnakeCaseNamingConvention();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>()
            .ToTable(nameof(Players).ToLower())
            .HasMany(x => x.Resources)
            .WithOne(x => x.Player)
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Player>()
            .HasIndex(x => x.Udid)
            .IsUnique();
        
        modelBuilder.Entity<Resource>()
            .ToTable(nameof(Resources).ToLower())
            .HasKey(x => new { x.PlayerId, x.ResourceType});
    }
}