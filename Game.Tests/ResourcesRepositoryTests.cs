using System.Runtime.CompilerServices;
using Game.Data;
using GameServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Game.Tests;

public class ResourcesRepositoryTests
{
    private readonly DbContextOptions<GameDataContext> _contextOptions;
    
    public ResourcesRepositoryTests()
    {
        _contextOptions = new DbContextOptionsBuilder<GameDataContext>()
            .UseInMemoryDatabase(nameof(ResourcesRepositoryTests))
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    [Fact]
    public async Task Change_Failed_NoResources()
    {
        // arrange
        var playerId = Guid.Parse("1356EFC0-5517-4271-A20E-4557A3E39E73");
        await using var context = new GameDataContext(_contextOptions);
        var repository = new ResourceRepository(context);
        
        // act
        var result = await repository.Change(playerId,  ResourceType.Coins, 123, default);
        
        //assert
        Assert.Null(result);
    }
    
    
    [Theory]
    [InlineData(100, 22, 122)]
    [InlineData(100, -100, 0)]
    [InlineData(100, -101, 100)]
    public async Task Change_Ok(int startAmount, int change, int expectedAmount)
    {
        // arrange
        var playerId = Guid.Parse("1356EFC0-5517-4271-A20E-4557A3E39E73");
        await using var context = new GameDataContext(_contextOptions);
        context.Resources.Add(new Resource
        {
            PlayerId = playerId,
            ResourceType = ResourceType.Coins,
            Amount = startAmount
        });
        await context.SaveChangesAsync();
        var repository = new ResourceRepository(context);
        
        // act
        var result = await repository.Change(playerId,  ResourceType.Coins, change, default);
        
        //assert
        var resources = await repository.GetResources(playerId, default);

        var coins = resources.Single(x => x.ResourceType == ResourceType.Coins);
        Assert.Equal(expectedAmount, coins.Amount);
        Assert.Equal(startAmount + change, result?.Amount);
        
        // clear 
        context.RemoveRange(context.Resources.ToList());
        await context.SaveChangesAsync();
    }
}