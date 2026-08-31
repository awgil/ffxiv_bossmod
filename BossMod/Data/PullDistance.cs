namespace BossMod.Data;

public static class PullDistance
{
    private static readonly Dictionary<uint, float> Known = new()
    {
        // Twintania (UCOB)
        [0x1FDF] = 0.5f,
        // Nael (UCOB)
        [0x1FE1] = 0.5f,
    };

    public static bool TryGet(uint oid, out float distance) => Known.TryGetValue(oid, out distance);
}
