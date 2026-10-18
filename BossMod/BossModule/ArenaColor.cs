namespace BossMod;

// common color constants (ABGR)
// TODO: migrate away to direct use of ColorConfig
public class ArenaColor(ColorConfig colors)
{
    public uint Background => colors.ArenaBackground.ABGR;
    public uint Border => colors.ArenaBorder.ABGR;
    public uint HighlightN => colors.ArenaHighlightN.ABGR;
    public uint AOE => colors.ArenaAOE.ABGR;
    public uint SafeFromAOE => colors.ArenaSafeFromAOE.ABGR;
    public uint Danger => colors.ArenaDanger.ABGR;
    public uint Safe => colors.ArenaSafe.ABGR;
    public uint Trap => 0x80000080; // TODO: reconsider?
    public uint PC => colors.ArenaPC.ABGR;
    public uint Enemy => colors.ArenaEnemy.ABGR;
    public uint Object => colors.ArenaObject.ABGR;
    public uint PlayerInteresting => colors.ArenaPlayerInteresting.ABGR;
    public uint PlayerGeneric => colors.ArenaPlayerGeneric.ABGR;
    public uint PlayerReallyGeneric => colors.ArenaPlayerReallyGeneric.ABGR;
    public uint Vulnerable => colors.ArenaPlayerVulnerable.ABGR;
}
