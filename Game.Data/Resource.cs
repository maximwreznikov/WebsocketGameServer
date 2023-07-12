using GameServer.Models;

namespace Game.Data;

public class Resource
{
    public Guid PlayerId { get; set; }
    
    public ResourceType ResourceType { get; set; }
    
    public int Amount { get; set; }
    
    public Player? Player { get; set; }

    public bool Change(int amount)
    {
        var value = Amount;
        value += amount;
        if (value < 0)
        {
            return false;
        } 
        
        Amount = value;
        return true;
    }
}