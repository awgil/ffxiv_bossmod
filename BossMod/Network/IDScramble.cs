namespace BossMod.Network;

// some of the packets have a simplistic 'scramble' transformation applied to some of the fields:
// - action-id of ActorCast packet
// - action-id of ActionEffectN packets
// - param1 (icon-id) of ActorControl packet with TargetIcon category
// the scramble itself is based on information in InitZone and/or ApplyIDScramble packets
// the process looks like this:
// - based on packet data and some hardcoded lookup tables, we calculate scramble delta - it is then added to these fields sent by the server (and subtracted on client before processing)
// - two random values are generated on the client, one at static init, another on each update
// - if delta is >0 (i.e. scramble is applied), client stores sum of delta and two bases in 'adjusted' field; otherwise it generates random value for 'adjusted' field that is less than sum of bases
public static unsafe class IDScramble
{
    public static int* OffsetBaseFixed { get; private set; } = null; // this is set to rand() % 256 + 14 on static init
    public static int* OffsetBaseChanging { get; private set; } = null; // this is set to rand() % 255 + 1 on every zone change
    public static int* OffsetAdjusted { get; private set; } = null; // this is set to (rand() % base-sum) if id is not scrambled (so < base-sum) -or- to base-sum + delta calculated from packet data (if scrambled) on every zone change

    public static uint Delta => OffsetAdjusted != null && *OffsetAdjusted > *OffsetBaseFixed + *OffsetBaseChanging ? (uint)(*OffsetAdjusted - *OffsetBaseFixed - *OffsetBaseChanging) : 0;

    public static void Initialize()
    {
        // sequence we're looking for:
        //.text:0000000140778C4B mov     cs:g_netOffBaseChanging, r8d  ; 44 89 05 ?? ?? ?? ??
        //.text:0000000140778C52 call    rand                          ; E8 ?? ?? ?? ??
        //.text:0000000140778C57 mov     r8d, cs:g_netOffBaseFixed     ; 44 8B 05 ?? ?? ?? ??
        //.text:0000000140778C5E xor     edx, edx                      ; 33 D2
        //.text:0000000140778C60 add     r8d, cs:g_netOffBaseChanging  ; 44 03 05 ?? ?? ?? ??
        //.text:0000000140778C67 mov     rbx, [rsp+28h+arg_0]          ; 48 8B 5C 24 ??
        //.text:0000000140778C6C mov     rsi, [rsp+28h+arg_8]          ; 48 8B 74 24 ??
        //.text:0000000140778C71 div     r8d                           ; 41 F7 F0
        //.text:0000000140778C74 mov     cs:g_netOffAdjusted, edx      ; 89 15 ?? ?? ?? ??
        // note that these three globals are laid out in sequence, but in some builds there's another unknown int between them
        var scrambleAddr = Service.SigScanner.ScanText("44 89 05 ?? ?? ?? ?? E8 ?? ?? ?? ?? 44 8B 05 ?? ?? ?? ?? 33 D2 44 03 05 ?? ?? ?? ?? 48 8B 5C 24 ?? 48 8B 74 24 ?? 41 F7 F0 89 15");
        Service.Log($"IDScramble sequence = 0x{scrambleAddr:X}");
        OffsetBaseChanging = ReadRVA(scrambleAddr + 3);
        OffsetAdjusted = ReadRVA(scrambleAddr + 43);
        OffsetBaseFixed = ReadRVA(scrambleAddr + 15);
        Service.Log($"IDScramble addresses = 0x{(nint)OffsetBaseChanging:X}==0x{(nint)ReadRVA(scrambleAddr + 24):X} 0x{(nint)OffsetAdjusted:X} 0x{(nint)OffsetBaseFixed:X}");
    }

    private static int* ReadRVA(nint ptr) => (int*)(ptr + 4 + *(int*)ptr);
}
