namespace GameServer.Models;

public record SendGiftRequest(string FriendPlayerUdid, 
    ResourceType ResourceType, 
    int ResourceValue) : Command;

public record SendGiftResponse(
    string SenderPlayerId, 
    string FriendPlayerId, 
    ResourceType ResourceType, 
    int ResourceValue): Command;
