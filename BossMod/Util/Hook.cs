using Dalamud.Hooking;
using InteropGenerator.Runtime;

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
        Service.Log($"Hooking {typeof(T)} @ 0x{address:X}");
        _hook = Service.Hook.HookFromAddress(address, detour);
        if (autoEnable)
            _hook.Enable();
    }

    public void Dispose() => _hook.Dispose();
}
