using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using System;

namespace BossMod
{
    public class Service
    {
#pragma warning disable CS8618
        [PluginService] public static DataManager DataManager { get; private set; }
        [PluginService] public static ClientState ClientState { get; private set; }
        [PluginService] public static ObjectTable ObjectTable { get; private set; }
        [PluginService] public static GameGui GameGui { get; private set; }
        [PluginService] public static SigScanner SigScanner { get; private set; }
#pragma warning restore CS8618

        public static Action<string>? LogHandler = null;
        public static void Log(string msg)
        {
            if (LogHandler != null)
                LogHandler(msg);
        }
    }
}
