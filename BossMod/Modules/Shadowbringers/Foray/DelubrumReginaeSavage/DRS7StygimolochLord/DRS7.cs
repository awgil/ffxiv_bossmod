namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

sealed class FoeSplitter(BossModule module) : Components.Cleave(module, (uint)AID.FoeSplitter, new AOEShapeCone(9f, 45f.Degrees())); // TODO: verify angle
sealed class ThunderousDischarge(BossModule module) : Components.CastCounter(module, (uint)AID.ThunderousDischargeAOE);
sealed class ThousandTonzeSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThousandTonzeSwing, 20f);
sealed class Whack(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WhackAOE, new AOEShapeCone(40f, 30f.Degrees()));
sealed class DevastatingBoltOuter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DevastatingBoltOuter, new AOEShapeDonut(25f, 30f));
sealed class DevastatingBoltInner(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DevastatingBoltInner, new AOEShapeDonut(12f, 17f));
sealed class Electrocution(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electrocution, 3f);

// TODO: ManaFlame component - show reflect hints
[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761u, NameID = 9759u, PlanLevel = 80)]
public sealed class DRS7StygimolochLord : BossModule
{
    public DRS7StygimolochLord(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private DRS7StygimolochLord(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    public static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(-416f, -184f), 34.5f, 48)], [new Rectangle(new(-416f, -219f), 20f, 1.4f), new Rectangle(new(-416f, -149.014f), 20f, 1.25f)]);
        return (arena.Center, arena);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.StygimolochMonk));
        Arena.Actors(Enemies((uint)OID.BallOfEarth), Colors.Object);
        Arena.Actors(Enemies((uint)OID.BallOfFire), Colors.Object);
    }
}
