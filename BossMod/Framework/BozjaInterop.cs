using System.Runtime.InteropServices;

namespace BossMod;

// bozja-specific utilities
unsafe class BozjaInterop : IDisposable
{
    public static BozjaInterop? Instance;

    [StructLayout(LayoutKind.Explicit)]
    private struct Holster
    {
        public const int Capacity = 100;

        [FieldOffset(0x6C)] public fixed byte Contents[Capacity];
    }

    private delegate Holster* GetHolsterDelegate();
    private GetHolsterDelegate _getHolsterFunc;

    private delegate bool UseFromHolsterDelegate(uint holsterId, uint slot);
    private UseFromHolsterDelegate _useFromHolsterFunc;

    public BozjaInterop()
    {
        var getHolsterAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 85 FF 74 1D");
        _getHolsterFunc = Marshal.GetDelegateForFunctionPointer<GetHolsterDelegate>(getHolsterAddress);
        Service.Log($"[BozjaInterop] GetHolster address = 0x{getHolsterAddress:X}");

        var useFromHolsterAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B 47 38 89 70 18");
        _useFromHolsterFunc = Marshal.GetDelegateForFunctionPointer<UseFromHolsterDelegate>(useFromHolsterAddress);
        Service.Log($"[BozjaInterop] UseFromHolster address = 0x{useFromHolsterAddress:X}");
    }

    public void Dispose()
    {
    }

    public static void FetchHolster(Span<byte> result)
    {
        if (result.Length < (int)BozjaHolsterID.Count)
            throw new ArgumentException($"Buffer too small: {result.Length} < {(int)BozjaHolsterID.Count}");

        result.Clear();
        var holster = Instance != null ? Instance._getHolsterFunc() : null;
        if (holster != null)
        {
            for (int i = 0; i < Holster.Capacity; ++i)
            {
                var entry = holster->Contents[i];
                if (entry != 0)
                    ++result[entry];
            }
        }
    }

    public static BozjaHolsterID GetHolsterEntry(uint index)
    {
        var holster = Instance != null ? Instance._getHolsterFunc() : null;
        return holster != null ? (BozjaHolsterID)holster->Contents[index] : BozjaHolsterID.None;
    }

    public static bool UseFromHolster(BozjaHolsterID id, uint slot) => Instance?._useFromHolsterFunc((uint)id, slot) ?? false;
}
