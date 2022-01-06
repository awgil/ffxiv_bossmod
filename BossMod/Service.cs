using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;

namespace BossMod
{
    public class Service
    {
#pragma warning disable CS8618
        [PluginService] public static DataManager DataManager { get; private set; }
        [PluginService] public static ClientState ClientState { get; private set; }
        [PluginService] public static ObjectTable ObjectTable { get; private set; }
#pragma warning restore CS8618
    }
}
