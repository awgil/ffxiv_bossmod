using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using InteropGenerator.Runtime;
using System.Runtime.InteropServices;

namespace BossMod.Services;

public class GameInteropExtended(IGameInteropProvider provider, ISigScanner scanner)
{
    public T GetDelegateFromSignature<T>(string signature) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>(scanner.ScanText(signature));
    public Hook<T> HookFromAddress<T>(Address addr, T detour, bool autoEnable = true) where T : Delegate => HookFromAddress(addr.Value, detour, autoEnable);
    public Hook<T> HookFromAddress<T>(nint addr, T detour, bool autoEnable = true) where T : Delegate
    {
        var hook = provider.HookFromAddress(addr, detour);
        if (autoEnable)
            hook.Enable();
        return hook;
    }
    public Hook<T> HookFromSignature<T>(string signature, T detour, bool autoEnable = true) where T : Delegate => HookFromAddress(scanner.ScanText(signature), detour, autoEnable);
}
