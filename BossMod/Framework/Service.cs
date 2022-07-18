using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Gui;
using Dalamud.Game.Network;
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
        [PluginService] public static PartyList PartyList { get; private set; }
        [PluginService] public static ChatGui ChatGui { get; private set; }
        [PluginService] public static GameGui GameGui { get; private set; }
        [PluginService] public static SigScanner SigScanner { get; private set; }
        [PluginService] public static JobGauges JobGauges { get; private set; }
        [PluginService] public static GameNetwork GameNetwork { get; private set; }
        [PluginService] public static KeyState KeyState { get; private set; }
        [PluginService] public static Condition Condition { get; private set; }
        [PluginService] public static TargetManager TargetManager { get; private set; }
        [PluginService] public static Framework Framework { get; private set; }
#pragma warning restore CS8618

        public static Action<string>? LogHandler = null;
        public static void Log(string msg)
        {
            if (LogHandler != null)
                LogHandler(msg);
        }

        public static Lumina.GameData? LuminaGameData = null;
        public static T? LuminaRow<T>(uint row) where T : Lumina.Excel.ExcelRow => LuminaGameData?.GetExcelSheet<T>(Lumina.Data.Language.English)?.GetRow(row);

        public static ConfigRoot Config = new();

        //public static SharpDX.Direct3D11.Device? Device = null;
    }
}
