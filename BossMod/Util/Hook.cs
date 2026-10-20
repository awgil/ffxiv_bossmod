using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using InteropGenerator.Runtime;
using System.Diagnostics;

namespace BossMod;

// very simple wrappers for hooks, that provide some quality of life (no need to repeat delegate types multiple times, etc)
public sealed class HookAddress<T> : IDisposable where T : Delegate
{
    private readonly Hook<T> _hook;

    public nint Address => _hook.Address;
    public T Original => _hook.Original;
    public bool Enabled
    {
        get => _hook.IsEnabled;
        set
        {
            if (value)
                _hook.Enable();
            else
                _hook.Disable();
        }
    }

    public HookAddress(ISigScanner scanner, string signature, T detour, bool autoEnable = true) : this(scanner.ScanText(signature), detour, autoEnable) { }
    public HookAddress(string signature, T detour, bool autoEnable = true) : this(Service.SigScanner.ScanText(signature), detour, autoEnable) { }
    public HookAddress(Address address, T detour, bool autoEnable = true) : this(address.Value, detour, autoEnable) { }
    public HookAddress(nint address, T detour, bool autoEnable = true)
    {
        Service.Log($"Hooking {typeof(T)} @ 0x{address:X}");
        _hook = Service.Hook.HookFromAddress(address, detour);
        if (autoEnable)
            _hook.Enable();
    }

    public void Dispose() => _hook.Dispose();
}

public sealed class HookProvider(ISigScanner scanner, IPluginLog logger, IGameInteropProvider hooking)
{
    public Hook<T> FromSignature<T>(string signature, T detour, bool autoEnable = true) where T : Delegate => FromAddress(scanner.ScanText(signature), detour, autoEnable);
    public Hook<T> FromAddress<T>(Address address, T detour, bool autoEnable = true) where T : Delegate => FromAddress(address.Value, detour, autoEnable);
    public Hook<T> FromAddress<T>(nint address, T detour, bool autoEnable = true) where T : Delegate
    {
        var hook = hooking.HookFromAddress(address, detour);
        logger.Debug($"Hooking {typeof(T)} @ +0x{address - GetBaseAddress():X}");
        if (autoEnable)
            hook.Enable();
        return hook;
    }

    public nint ScanText(string signature, string label)
    {
        var addr = scanner.ScanText(signature);
        logger.Debug($"{label} address: +0x{addr - GetBaseAddress():X}");
        return addr;
    }

    static nint GetBaseAddress() => Process.GetCurrentProcess().MainModule!.BaseAddress;
}
