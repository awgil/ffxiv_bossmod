using DalaMock.Shared.Interfaces;
using Dalamud.Bindings.ImGui;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BossMod;

public sealed class Service
{
#pragma warning disable CA2211
    public static Action<string>? LogHandlerDebug;
    public static Action<string>? LogHandlerVerbose;
    public static void Log(string msg) => LogHandlerDebug?.Invoke(msg);
    public static void LogVerbose(string msg) => LogHandlerVerbose?.Invoke(msg);

    public static Lumina.GameData? LuminaGameData;
    public static Lumina.Excel.ExcelSheet<T>? LuminaSheet<T>() where T : struct, Lumina.Excel.IExcelRow<T> => LuminaGameData?.GetExcelSheet<T>(Lumina.Data.Language.English);
    public static T? LuminaRow<T>(uint row) where T : struct, Lumina.Excel.IExcelRow<T> => LuminaSheet<T>()?.GetRowOrDefault(row);
    public static ConcurrentDictionary<Lumina.Text.ReadOnly.ReadOnlySeString, Lumina.Text.ReadOnly.ReadOnlySeString> LuminaRSV = []; // TODO: reconsider

    public static IWindowSystem? WindowSystem;

    public static ImFontPtr IconFont;
#pragma warning restore CA2211

    public static readonly LazyExternal<ConfigRoot> ConfigLazy = new();
    public static ConfigRoot Config => ConfigLazy.ValueOrException;

    //public static SharpDX.Direct3D11.Device? Device = null;
}

public class LazyExternal<T>
{
    private readonly TaskCompletionSource<T> _source = new();

    public T Value => _source.Task.Result;
    public T ValueOrException => _source.Task.IsCompleted ? Value : throw new InvalidOperationException($"Accessed LazyExternal<{typeof(T).Name}> before initialization");
    public async Task<T> GetValue()
    {
        await _source.Task;
        return Value;
    }

    public void SetValue(T value)
    {
        //if (_source.Task.IsCompleted)
        //    throw new InvalidOperationException($"Set called on LazyExternal<{typeof(T).Name}> after being initialized");

        _source.SetResult(value);
    }
}
