namespace GameServer.Models;

public record UpdateResourcesRequest(ResourceType Type, int Value) : 
    Command;

public record ResourceResponse(ResourceType Type, int Value):
    Command;

public record ResourcesResponse(ResourceResponse[] Resources) :
    Command;