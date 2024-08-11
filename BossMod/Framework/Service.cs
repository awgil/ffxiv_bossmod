﻿using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace BossMod;

public sealed class Service
{
#pragma warning disable CS8618
    [PluginService] public static IPluginLog Logger { get; private set; }
    [PluginService] public static IChatGui ChatGui { get; private set; }
    [PluginService] public static IGameGui GameGui { get; private set; }
    [PluginService] public static IGameConfig GameConfig { get; private set; }
    [PluginService] public static IGameInteropProvider Hook { get; private set; }
    [PluginService] public static ISigScanner SigScanner { get; private set; }
    [PluginService] public static ICondition Condition { get; private set; }
    [PluginService] public static IFramework Framework { get; private set; }
    [PluginService] public static ITextureProvider Texture { get; private set; }
    [PluginService] public static ICommandManager CommandManager { get; private set; }
    [PluginService] public static IDtrBar DtrBar { get; private set; }
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; }
    // TODO: get rid of stuff below in favour of CS
    [PluginService] public static IClientState ClientState { get; private set; }
    [PluginService] public static IObjectTable ObjectTable { get; private set; }
    [PluginService] public static ITargetManager TargetManager { get; private set; }
    [PluginService] public static IKeyState KeyState { get; private set; }
#pragma warning restore CS8618

#pragma warning disable CA2211
    public static Action<string>? LogHandler;
    public static void Log(string msg) => LogHandler?.Invoke(msg);

    public static object LuminaRSVLock = new(); // TODO: replace with System.Threading.Lock
    public static Lumina.GameData? LuminaGameData;
    public static Lumina.Excel.ExcelSheet<T>? LuminaSheet<T>() where T : Lumina.Excel.ExcelRow => LuminaGameData?.GetExcelSheet<T>(Lumina.Data.Language.English);
    public static T? LuminaRow<T>(uint row) where T : Lumina.Excel.ExcelRow => LuminaSheet<T>()?.GetRow(row);

    public static WindowSystem? WindowSystem;
#pragma warning restore CA2211

    public static readonly ConfigRoot Config = new();

    //public static SharpDX.Direct3D11.Device? Device = null;
}
