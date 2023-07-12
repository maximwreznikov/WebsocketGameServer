namespace GameServer.Models;

public record LoginRequest(string DeviceId) : 
    Command;

public record LoginResponse(bool Success, Guid PlayerId, string Udid) : 
    Command;