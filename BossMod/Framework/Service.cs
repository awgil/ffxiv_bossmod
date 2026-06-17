using DalaMock.Shared.Interfaces;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System.Collections.Concurrent;

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
    public static IDalamudPluginInterface PluginInterface { get; set; }
    // TODO: get rid of stuff below in favour of CS
    [PluginService] public static IClientState ClientState { get; private set; }
    [PluginService] public static IObjectTable ObjectTable { get; private set; }
    [PluginService] public static IPlayerState PlayerState { get; private set; }
    [PluginService] public static ITargetManager TargetManager { get; private set; }
    [PluginService] public static IKeyState KeyState { get; private set; }
    [PluginService] public static INotificationManager Notifications { get; private set; }
    [PluginService] public static IPluginLog PluginLog { get; private set; }
#pragma warning restore CS8618

#pragma warning disable CA2211
    public static void Log(string msg, params object[] values) => PluginLog.Debug(msg, values);
    public static void LogVerbose(string msg, params object[] values) => PluginLog.Verbose(msg, values);

    public static bool IsDev => PluginInterface.IsDev;
    public static bool IsMock;

    public static Lumina.GameData? LuminaGameData;
    public static Lumina.Excel.ExcelSheet<T>? LuminaSheet<T>() where T : struct, Lumina.Excel.IExcelRow<T> => LuminaGameData?.GetExcelSheet<T>();
    public static T? LuminaRow<T>(uint row) where T : struct, Lumina.Excel.IExcelRow<T> => LuminaSheet<T>()?.GetRowOrDefault(row);
    public static ConcurrentDictionary<Lumina.Text.ReadOnly.ReadOnlySeString, Lumina.Text.ReadOnly.ReadOnlySeString> LuminaRSV = []; // TODO: reconsider

    public static IWindowSystem WindowSystem = null!;
    public static IFileDialogManager FileDialogManager = null!;
    public static ImFontPtr IconFont = ImFontPtr.Null;
#pragma warning restore CA2211

    public static readonly ConfigRoot Config = new();

    //public static SharpDX.Direct3D11.Device? Device = null;
}
