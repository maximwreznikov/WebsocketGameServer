using System.Net;
using System.Net.WebSockets;
using System.Text;
using Game.Client;
using GameServer.Models;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug)
        .CreateLogger();

Log.Logger.Information("Start websocket game");

var cancelSource = new CancellationTokenSource();

Uri uri = new(Settings.WsUrl);

using var ws = new WsClient(Log.Logger.ForContext<WsClient>());
await ws.Open(uri, cancelSource.Token);

#pragma warning disable CS4014
ws.RunLoop(cancelSource.Token);
#pragma warning restore CS4014

using var client = new GameClient(ws, Log.Logger.ForContext<GameClient>());

bool continueGame = true;
while (continueGame)
{
    Console.WriteLine(@"Available commands: 
l {udid}   - login (default '1')
uc {value} - change coins amount (default 200)
ur {value}  - change rolls amount (default 100)
s {friend_udid} - send gift ");
    var pressKey = Console.ReadLine()?.Trim().ToLower();
    switch(pressKey)
    {
        case null:
            break;
        case var _ when pressKey.StartsWith('l'):
        {
            var cmdArgs = pressKey.Split(" ");
            var playerUdid = cmdArgs.Length > 1 ? cmdArgs[1] : "1";
            await client.Login(playerUdid, cancelSource.Token);
            break;
        }
        case var _ when pressKey.StartsWith("uc"):
        {
            var cmdArgs = pressKey.Split(" ");
            int value = cmdArgs.Length > 1 && int.TryParse(cmdArgs[1], out value) ? value : 200;
            await client.Update(ResourceType.Coins, value, cancelSource.Token);
            break;
        }
        case var _ when pressKey.StartsWith("ur"):
        {
            var cmdArgs = pressKey.Split(" ");
            int value = cmdArgs.Length > 1 && int.TryParse(cmdArgs[1], out value) ? value : 100;
            await client.Update(ResourceType.Rolls, value, cancelSource.Token);
            break;
        }
        case var cmd when cmd.StartsWith('s'):
        {
            var cmdArgs = cmd.Split(" ");
            var playerUdid = cmdArgs.Length > 1 ? cmdArgs[1] : "2";
            int value = cmdArgs.Length > 2 && int.TryParse(cmdArgs[2], out value) ? value : 50;
            await client.SendGift(playerUdid, ResourceType.Rolls, value, cancelSource.Token);
            break;
        }
        default:
            // if unknown command than exit game
            continueGame = false;
            await client.Close(cancelSource.Token);
            break;
    }
}

cancelSource.Cancel();

Log.Logger.Information("Exit websocket game");




