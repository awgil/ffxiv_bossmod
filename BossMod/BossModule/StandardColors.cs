namespace BossMod;

// common color constants (ABGR)
public class StandardColors(ColorConfig config)
{
    public ColorConfig Configuration => config;
    public uint Background => config.ArenaBackground.ABGR;
    public uint Border => config.ArenaBorder.ABGR;
    public uint AOE => config.ArenaAOE.ABGR;
    public uint SafeFromAOE => config.ArenaSafeFromAOE.ABGR;
    public uint Danger => config.ArenaDanger.ABGR;
    public uint Safe => config.ArenaSafe.ABGR;
    public uint Trap => 0x80000080; // TODO: reconsider?
    public uint PC => config.ArenaPC.ABGR;
    public uint Enemy => config.ArenaEnemy.ABGR;
    public uint Object => config.ArenaObject.ABGR;
    public uint PlayerInteresting => config.ArenaPlayerInteresting.ABGR;
    public uint PlayerGeneric => config.ArenaPlayerGeneric.ABGR;
    public uint PlayerReallyGeneric => config.ArenaPlayerReallyGeneric.ABGR;
    public uint Vulnerable => config.ArenaPlayerVulnerable.ABGR;
}
