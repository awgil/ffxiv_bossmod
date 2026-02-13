using DalaMock.Shared.Interfaces;
using Dalamud.Bindings.ImGui;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System.Collections.Concurrent;

namespace BossMod;

public sealed class Service
{
#pragma warning disable CS8618
    [PluginService] public static IPluginLog Logger { get; private set; }
    public static IGameInteropProvider Hook;
    public static ISigScanner SigScanner;
    public static ICondition Condition;
    [PluginService] public static IFramework Framework { get; private set; }
    [PluginService] public static ICommandManager CommandManager { get; private set; }
    public static IDalamudPluginInterface PluginInterface;
    // TODO: get rid of stuff below in favour of CS
    public static IClientState ClientState;
    public static IObjectTable ObjectTable;
    [PluginService] public static IPlayerState PlayerState { get; private set; }
    [PluginService] public static ITargetManager TargetManager { get; private set; }
    public static IKeyState KeyState;
    [PluginService] public static INotificationManager Notifications { get; private set; }
#pragma warning restore CS8618

#pragma warning disable CA2211
    public static Action<string>? LogHandlerDebug;
    public static Action<string>? LogHandlerVerbose;
    public static void Log(string msg) => LogHandlerDebug?.Invoke(msg);
    public static void LogVerbose(string msg) => LogHandlerVerbose?.Invoke(msg);

    public static bool IsDev = true;

    public static Lumina.GameData? LuminaGameData;
    public static Lumina.Excel.ExcelSheet<T>? LuminaSheet<T>() where T : struct, Lumina.Excel.IExcelRow<T> => LuminaGameData?.GetExcelSheet<T>(Lumina.Data.Language.English);
    public static T? LuminaRow<T>(uint row) where T : struct, Lumina.Excel.IExcelRow<T> => LuminaSheet<T>()?.GetRowOrDefault(row);
    public static ConcurrentDictionary<Lumina.Text.ReadOnly.ReadOnlySeString, Lumina.Text.ReadOnly.ReadOnlySeString> LuminaRSV = []; // TODO: reconsider

    public static IWindowSystem? WindowSystem;

    public static ImFontPtr IconFont;
#pragma warning restore CA2211

    public static bool IsUIDev => PluginInterface == null;

    public static readonly ConfigRoot Config = new();

    //public static SharpDX.Direct3D11.Device? Device = null;
}
