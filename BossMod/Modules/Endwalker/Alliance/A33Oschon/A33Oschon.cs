namespace BossMod.Endwalker.Alliance.A33Oschon;

class DownhillP1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Downhill), 6);
class SoaringMinuet1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SoaringMinuet1), new AOEShapeCone(65, 135.Degrees()));
class SoaringMinuet2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SoaringMinuet2), new AOEShapeCone(65, 135.Degrees()));
class SuddenDownpour(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SuddenDownpour2));
class ClimbingShotRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ClimbingShot), "Raidwide + Knockback");
class ClimbingShotRaidwide2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ClimbingShot2), "Raidwide + Knockback");
class ClimbingShotRaidwide3(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ClimbingShot3), "Raidwide + Knockback");
class ClimbingShotRaidwide4(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ClimbingShot4), "Raidwide + Knockback");
class LoftyPeaks(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.LoftyPeaks), "Raidwide x5 coming");
class TrekShot(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TrekShot), new AOEShapeCone(65, 60.Degrees()));
class TrekShot2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TrekShot2), new AOEShapeCone(65, 60.Degrees()));

class TheArrow(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.TheArrow), new AOEShapeCircle(6), true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class TheArrowP2(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.TheArrowP2), new AOEShapeCircle(10), true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class PitonPull(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.PitonPull), 22);
class Altitude(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Altitude), 6);
class DownhillSmall(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.DownhillSmall), 6);
class DownhillBig(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.DownhillBig), 8);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus, LTS", PrimaryActorOID = (uint)OID.OschonP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11300, SortOrder = 4)]
public class A33Oschon(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsSquare(new(0, 750), 25))
{
    private Actor? _oschonP1;
    private Actor? _oschonP2;

    public Actor? OschonP1() => PrimaryActor;
    public Actor? OschonP2() => _oschonP2;

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
