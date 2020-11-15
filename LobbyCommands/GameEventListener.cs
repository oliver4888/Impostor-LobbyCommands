using System;
using System.Linq;
using System.Threading.Tasks;

using Impostor.Api.Events;
using Impostor.Api.Innersloth;
using Impostor.Api.Events.Player;

using Microsoft.Extensions.Logging;

namespace LobbyCommands
{
    public class GameEventListener : IEventListener
    {
        readonly ILogger<LobbyCommandsPlugin> _logger;
        readonly string[] _mapNames = Enum.GetNames(typeof(MapTypes));

        public GameEventListener(ILogger<LobbyCommandsPlugin> logger) => _logger = logger;

        [EventListener]
        public void OnPlayerChat(IPlayerChatEvent e)
        {
            if (e.Game.GameState != GameStates.NotStarted || !e.Message.StartsWith("/") || !e.ClientPlayer.IsHost)
                return;

            Task.Run(async () => await DoCommands(e));
        }

        private async Task DoCommands(IPlayerChatEvent e)
        {
            _logger.LogDebug($"Attempting to evaluate command from {e.PlayerControl.PlayerInfo.PlayerName} on {e.Game.Code.Code}. Message was: {e.Message}");

            string[] parts = e.Message.ToLowerInvariant()[1..].Split(" ");

            switch (parts[0])
            {
                case "impostors":
                    if (parts.Length == 1)
                    {
                        await e.PlayerControl.SendChatAsync($"Please specify the number of impostors.");
                        return;
                    }

                    if (int.TryParse(parts[1], out int num))
                    {
                        num = Math.Clamp(num, 1, 3);

                        await e.PlayerControl.SendChatAsync($"Setting the number of impostors to {num}");

                        e.Game.Options.NumImpostors = num;
                        await e.Game.SyncSettingsAsync();
                    }
                    else
                        await e.PlayerControl.SendChatAsync($"Unable to convert '{parts[1]}' to a number!");
                    break;
                case "map":
                    if (parts.Length == 1)
                    {
                        await e.PlayerControl.SendChatAsync($"Please specify the map. Accepted values: {string.Join(", ", _mapNames)}");
                        return;
                    }

                    if (!_mapNames.Any(name => name.ToLowerInvariant() == parts[1]))
                    {
                        await e.PlayerControl.SendChatAsync($"Unknown map. Accepted values: {string.Join(", ", _mapNames)}");
                        return;
                    }

                    MapTypes map = Enum.Parse<MapTypes>(parts[1], true);

                    await e.PlayerControl.SendChatAsync($"Setting map to {map}");

                    e.Game.Options.Map = map;
                    await e.Game.SyncSettingsAsync();
                    break;
                default:
                    _logger.LogInformation($"Unknown command {parts[0]} from {e.PlayerControl.PlayerInfo.PlayerName} on {e.Game.Code.Code}.");
                    break;
            }
        }
    }
}
