namespace BossMod;

// common color constants (ABGR)
// TODO: migrate away to direct use of ColorConfig
public static class ArenaColor
{
    private static readonly ColorConfig Config = Service.Config.Get<ColorConfig>();

    public static uint Background => Config.ArenaBackground.ABGR;
    public static uint Border => Config.ArenaBorder.ABGR;
    public static uint AOE => Config.ArenaAOE.ABGR;
    public static uint SafeFromAOE => Config.ArenaSafeFromAOE.ABGR;
    public static uint Danger => Config.ArenaDanger.ABGR;
    public static uint Safe => Config.ArenaSafe.ABGR;
    public static uint Trap => 0x80000080; // TODO: reconsider?
    public static uint PC => Config.ArenaPC.ABGR;
    public static uint Enemy => Config.ArenaEnemy.ABGR;
    public static uint Object => Config.ArenaObject.ABGR;
    public static uint PlayerInteresting => Config.ArenaPlayerInteresting.ABGR;
    public static uint PlayerGeneric => Config.ArenaPlayerGeneric.ABGR;
    public static uint PlayerReallyGeneric => Config.ArenaPlayerReallyGeneric.ABGR;
    public static uint Vulnerable => Config.ArenaPlayerVulnerable.ABGR;
}
