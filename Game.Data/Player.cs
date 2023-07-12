using GameServer.Models;

namespace Game.Data;

public class Player
{
    public Guid Id { get; set; }

    public string Udid { get; set; } = string.Empty;

    public ICollection<Resource> Resources { get; set; } = new List<Resource>();

    public static Player Create(string udid)
    {
        var player = new Player
        {
            Id = Guid.NewGuid(),
            Udid = udid,
            Resources = new List<Resource>
            {
                new()
                {
                    ResourceType = ResourceType.Coins,
                    Amount = 0
                },
                new()
                {
                    ResourceType = ResourceType.Rolls,
                    Amount = 0
                }
            }
        };
        return player;
    }
}