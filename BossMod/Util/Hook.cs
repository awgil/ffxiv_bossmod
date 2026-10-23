using Dalamud.Hooking;
using InteropGenerator.Runtime;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

    public HookAddress(Address address, T detour, bool autoEnable = true) : this(address.Value, detour, autoEnable) { }
    public HookAddress(string signature, T detour, bool autoEnable = true) : this(Service.SigScanner.ScanText(signature), detour, autoEnable) { }
    public HookAddress(nint address, T detour, bool autoEnable = true)
    {
        var @base = Process.GetCurrentProcess().MainModule!.BaseAddress;
        Service.Log($"Hooking {typeof(T)} @ +0x{address - @base:X}");
        _hook = Service.Hook.HookFromAddress(address, detour);
        if (autoEnable)
            _hook.Enable();
    }

    public void Dispose() => _hook.Dispose();

    public static T FnPtr(string signature)
    {
        var @base = Process.GetCurrentProcess().MainModule!.BaseAddress;
        var addr = Service.SigScanner.ScanText(signature);
        Service.Log($"Located {typeof(T)} @ +0x{addr - @base:X}");
        return Marshal.GetDelegateForFunctionPointer<T>(addr);
    }
}
