Basic WebSockets game server
- Used ASP Kestrel for networking.
- Used Dependency Injection to declare/consume services.
- API is easily extendable.
* Player state  saved in Sqlite.
* Game Server API:
- Login: accept DeviceId(UDID) and response with PlayerId. Make sure the player is not connected already, If so respond
accordingly.
- UpdateResources: accept ResourceType(coins, rolls), ResourceValue
and Response with new balance.
- SendGift: accept FriendPlayerId, ResourceType and ResourceValue
Update the Friend player state. If the friend is online then send a
GiftEvent with the relevant information to him.
Created a console Application to test the GameServer APIs

dotnet run --project .\Game.Server\Game.Server.csproj

dotnet run --project .\Game.Client\Game.Client.csproj