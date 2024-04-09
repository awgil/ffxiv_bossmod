namespace BossMod.Endwalker.Alliance.A33Oschon;

class DownhillP1() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.Downhill), 6);
class SoaringMinuet1() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.SoaringMinuet1), new AOEShapeCone(65, 135.Degrees()));
class SoaringMinuet2() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.SoaringMinuet2), new AOEShapeCone(65, 135.Degrees()));
class SuddenDownpour() : Components.RaidwideCast(ActionID.MakeSpell(AID.SuddenDownpour2));
class ClimbingShotRaidwide() : Components.RaidwideCast(ActionID.MakeSpell(AID.ClimbingShot), "Raidwide + Knockback");
class ClimbingShotRaidwide2() : Components.RaidwideCast(ActionID.MakeSpell(AID.ClimbingShot2), "Raidwide + Knockback");
class ClimbingShotRaidwide3() : Components.RaidwideCast(ActionID.MakeSpell(AID.ClimbingShot3), "Raidwide + Knockback");
class ClimbingShotRaidwide4() : Components.RaidwideCast(ActionID.MakeSpell(AID.ClimbingShot4), "Raidwide + Knockback");
class LoftyPeaks() : Components.RaidwideCast(ActionID.MakeSpell(AID.LoftyPeaks), "Raidwide x5 coming");
class TrekShot() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.TrekShot), new AOEShapeCone(65, 60.Degrees()));
class TrekShot2() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.TrekShot2), new AOEShapeCone(65, 60.Degrees()));
class PitonPull() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.PitonPull), 22);
class Altitude() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.Altitude), 6);
class DownhillSmall() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.DownhillSmall), 6);
class DownhillBig() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.DownhillBig), 8);

class TheArrow : Components.BaitAwayCast
{
    public TheArrow() : base(ActionID.MakeSpell(AID.TheArrow), new AOEShapeCircle(6), true) { }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class TheArrowP2 : Components.BaitAwayCast
{
    public TheArrowP2() : base(ActionID.MakeSpell(AID.TheArrowP2), new AOEShapeCircle(10), true) { }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus, LTS", PrimaryActorOID = (uint)OID.OschonP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11300)]
public class A33Oschon : BossModule
{
    private Actor? _oschonP1;
    private Actor? _oschonP2;

    public Actor? OschonP1() => PrimaryActor;
    public Actor? OschonP2() => _oschonP2;

    public A33Oschon(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(0, 750), 25)) { }

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _oschonP1 ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.OschonP1).FirstOrDefault() : null;
        _oschonP2 ??= StateMachine.ActivePhaseIndex == 1 ? Enemies(OID.OschonP2).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(_oschonP1, ArenaColor.Enemy);
        Arena.Actor(_oschonP2, ArenaColor.Enemy);
    }
}
