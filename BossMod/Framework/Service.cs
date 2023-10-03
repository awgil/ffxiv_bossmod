using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using System;

namespace BossMod
{
    public class Service
    {
#pragma warning disable CS8618
        [PluginService] public static IPluginLog Logger { get; private set; }
        [PluginService] public static IDataManager DataManager { get; private set; }
        [PluginService] public static IClientState ClientState { get; private set; }
        [PluginService] public static IObjectTable ObjectTable { get; private set; }
        [PluginService] public static IPartyList PartyList { get; private set; }
        [PluginService] public static IChatGui ChatGui { get; private set; }
        [PluginService] public static IGameGui GameGui { get; private set; }
        [PluginService] public static IGameInteropProvider Hook { get; private set; }
        [PluginService] public static ISigScanner SigScanner { get; private set; }
        [PluginService] public static IJobGauges JobGauges { get; private set; }
        [PluginService] public static IKeyState KeyState { get; private set; }
        [PluginService] public static ICondition Condition { get; private set; }
        [PluginService] public static ITargetManager TargetManager { get; private set; }
        [PluginService] public static IFramework Framework { get; private set; }
#pragma warning restore CS8618

        public static Action<string>? LogHandler = null;
        public static void Log(string msg)
        {
            if (LogHandler != null)
                LogHandler(msg);
        }

        public static Lumina.GameData? LuminaGameData = null;
        public static T? LuminaRow<T>(uint row) where T : Lumina.Excel.ExcelRow => LuminaGameData?.GetExcelSheet<T>(Lumina.Data.Language.English)?.GetRow(row);

        public static WindowSystem? WindowSystem = null;

        public static ConfigRoot Config = new();

        //public static SharpDX.Direct3D11.Device? Device = null;
    }
}
