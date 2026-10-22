using BossMod.Interfaces;
using BossMod.ReplayVisualization;
using DalaMock.Host.Mediator;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System.Collections.Concurrent;

namespace BossMod;

public sealed class Service
{
#pragma warning disable CS8618
    [PluginService] public static IChatGui ChatGui { get; private set; }
    [PluginService] public static IGameGui GameGui { get; private set; }
    [PluginService] public static IGameConfig GameConfig { get; private set; }
    [PluginService] public static IGameInteropProvider Hook { get; private set; }
    [PluginService] public static ISigScanner SigScanner { get; private set; }
    [PluginService] public static ICondition Condition { get; private set; }
    [PluginService] public static IFramework Framework { get; private set; }
    [PluginService] public static ITextureProvider Texture { get; private set; }
    public static IDalamudPluginInterface PluginInterface { get; set; }
    // TODO: get rid of stuff below in favour of CS
    [PluginService] public static IClientState ClientState { get; private set; }
    [PluginService] public static IObjectTable ObjectTable { get; private set; }
    [PluginService] public static IPlayerState PlayerState { get; private set; }
    [PluginService] public static ITargetManager TargetManager { get; private set; }
    [PluginService] public static IKeyState KeyState { get; private set; }
    [PluginService] private static INotificationManager Notifications { get; set; }
    [PluginService] public static IPluginLog PluginLog { get; private set; }
    [PluginService] public static IUnlockState UnlockState { get; private set; }
#pragma warning restore CS8618

#pragma warning disable CA2211
    public static void Log(string msg, params object[] values) => PluginLog.Debug(msg, values);

    public static bool IsDev;
    public static bool IsMock;

    public static void ChatMessage(string msg) => ChatGui.Print(msg, "VBM");
    public static void ChatError(string msg) => ChatGui.PrintError(msg, "VBM");

    public static void ShowNotification(string msg, string? title = null, NotificationType type = NotificationType.Success) => Notifications?.AddNotification(new() { Content = msg, Type = type, Title = title });

    public static Lumina.GameData LuminaGameData = null!;
    public static Lumina.Excel.ExcelSheet<T> LuminaSheet<T>() where T : struct, Lumina.Excel.IExcelRow<T> => LuminaGameData.GetExcelSheet<T>()!;
    public static T? LuminaRow<T>(uint row) where T : struct, Lumina.Excel.IExcelRow<T> => LuminaSheet<T>().GetRowOrDefault(row);
    public static ConcurrentDictionary<Lumina.Text.ReadOnly.ReadOnlySeString, Lumina.Text.ReadOnly.ReadOnlySeString> LuminaRSV = []; // TODO: reconsider

    public static IWindowSystem WindowSystem = null!;
    public static IUiBuilder UiBuilder = null!;
    public static ConfigRoot Config = null!;
    public static MediatorService Mediator = null!;
    public static GaugeVisualizer GaugeVisualizer = null!;
    public static ICamera Camera = null!;
#pragma warning restore CA2211

    //public static SharpDX.Direct3D11.Device? Device = null;
}
