namespace BossMod.Data;

public static class PullDistance
{
    // distance is between hitboxes
    private static readonly Dictionary<uint, float> Known = new()
    {
        // Twintania (UCOB)
        [0x1FDF] = 0,
        // Nael (UCOB)
        [0x1FE1] = 0,
    };

    public static bool TryGet(uint oid, out float distance) => Known.TryGetValue(oid, out distance);
}
