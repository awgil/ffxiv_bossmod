namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

class FoeSplitter(BossModule module) : Components.Cleave(module, AID.FoeSplitter, new AOEShapeCone(9, 45.Degrees())); // TODO: verify angle
class ThunderousDischarge(BossModule module) : Components.CastCounter(module, AID.ThunderousDischargeAOE);
class ThousandTonzeSwing(BossModule module) : Components.StandardAOEs(module, AID.ThousandTonzeSwing, new AOEShapeCircle(20));
class Whack(BossModule module) : Components.StandardAOEs(module, AID.WhackAOE, new AOEShapeCone(40, 30.Degrees()));
class DevastatingBoltOuter(BossModule module) : Components.StandardAOEs(module, AID.DevastatingBoltOuter, new AOEShapeDonut(25, 30));
class DevastatingBoltInner(BossModule module) : Components.StandardAOEs(module, AID.DevastatingBoltInner, new AOEShapeDonut(12, 17));
class Electrocution(BossModule module) : Components.StandardAOEs(module, AID.Electrocution, 3);

// TODO: ManaFlame component - show reflect hints
[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761, NameID = 9759, PlanLevel = 80)]
public class DRS7 : BossModule
{
    private readonly IReadOnlyList<Actor> _monks;
    private readonly IReadOnlyList<Actor> _ballsEarth;
    private readonly IReadOnlyList<Actor> _ballsFire;

    public DRS7(WorldState ws, Actor primary) : base(ws, primary, new(-416, -184), new ArenaBoundsCircle(35))
    {
        _monks = Enemies(OID.StygimolochMonk);
        _ballsEarth = Enemies(OID.BallOfEarth);
        _ballsFire = Enemies(OID.BallOfFire);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(_monks, ArenaColor.Enemy);
        Arena.Actors(_ballsEarth, ArenaColor.Object);
        Arena.Actors(_ballsFire, ArenaColor.Object);
    }
}
