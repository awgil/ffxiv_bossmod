namespace BossMod.Data;

public static class EnemyBehavior
{
    // distance is between hitboxes
    public static readonly Dictionary<uint, float> TankDistance = new()
    {
        // Twintania/Nael/Bahamut (UCOB)
        [0x1FDF] = 0,
        [0x1FE1] = 0,
        [0x1FE8] = 0,
    };

    public static bool TryGetTankDistance(uint oid, out float distance) => TankDistance.TryGetValue(oid, out distance);

    public static readonly HashSet<uint> MovementDisabled = [];
}
