using System;
using System.Threading.Tasks;

using Impostor.Api.Plugins;
using Impostor.Api.Events.Managers;

using Microsoft.Extensions.Logging;

namespace LobbyCommands
{
    [ImpostorPlugin(
        package: "uk.ol48.lobbycommands",
        name: "Lobby Commands",
        author: "oliver4888",
        version: "1.0.1")]
    public class LobbyCommandsPlugin : PluginBase
    {
        readonly ILogger<LobbyCommandsPlugin> _logger;
        readonly IEventManager _eventManager;
        IDisposable _unregister;

        public LobbyCommandsPlugin(ILogger<LobbyCommandsPlugin> logger, IEventManager eventManager)
        {
            _logger = logger;
            _eventManager = eventManager;
        }

        public override ValueTask EnableAsync()
        {
            _unregister = _eventManager.RegisterListener(new GameEventListener(_logger));
            return default;
        }

        public override ValueTask DisableAsync()
        {
            _unregister.Dispose();
            return default;
        }
    }
}
