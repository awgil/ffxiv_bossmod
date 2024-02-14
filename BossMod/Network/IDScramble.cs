namespace BossMod.Network
{
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
        public static int* OffsetBaseFixed = null; // this is set to rand() % 256 + 14 on static init
        public static int* OffsetBaseChanging = null; // this is set to rand() % 255 + 1 on every zone change
        public static int* OffsetAdjusted = null; // this is set to (rand() % base-sum) if id is not scrambled (so < base-sum) -or- to base-sum + delta calculated from packet data (if scrambled) on every zone change

        public static uint Delta => OffsetAdjusted != null && *OffsetAdjusted > *OffsetBaseFixed + *OffsetBaseChanging ? (uint)(*OffsetAdjusted - *OffsetBaseFixed - *OffsetBaseChanging) : 0;

        public static void Initialize()
        {
            var scrambleAddr = Service.SigScanner.GetStaticAddressFromSig("44 89 05 ?? ?? ?? ?? E8 ?? ?? ?? ?? 44 8B 05 ?? ?? ?? ?? 33 D2 44 03 05 ?? ?? ?? ?? 48 8B 5C 24");
            Service.Log($"IDScramble address = 0x{scrambleAddr:X}");
            OffsetBaseChanging = (int*)scrambleAddr;
            OffsetAdjusted = OffsetBaseChanging + 1;
            OffsetBaseFixed = OffsetBaseChanging + 3;
        }
    }
}
