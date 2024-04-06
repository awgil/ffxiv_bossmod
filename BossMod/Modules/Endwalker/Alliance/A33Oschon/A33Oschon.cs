namespace BossMod.Endwalker.Alliance.A33Oschon;

class DownhillP1 : Components.LocationTargetedAOEs
{
    public DownhillP1() : base(ActionID.MakeSpell(AID.Downhill), 6) { }
}

class SoaringMinuet1 : Components.SelfTargetedAOEs
{
    public SoaringMinuet1() : base(ActionID.MakeSpell(AID.SoaringMinuet1), new AOEShapeCone(65, 135.Degrees())) { }
}

class SoaringMinuet2 : Components.SelfTargetedAOEs
{
    public SoaringMinuet2() : base(ActionID.MakeSpell(AID.SoaringMinuet2), new AOEShapeCone(65, 135.Degrees())) { }
}

class SuddenDownpour : Components.RaidwideCast
{
    public SuddenDownpour() : base(ActionID.MakeSpell(AID.SuddenDownpour2)) { }
}

class LoftyPeaks : Components.RaidwideCast
{
    public LoftyPeaks() : base(ActionID.MakeSpell(AID.LoftyPeaks), "Raidwide x5 coming") { }
}

class TrekShot : Components.SelfTargetedAOEs
{
    public TrekShot() : base(ActionID.MakeSpell(AID.TrekShot), new AOEShapeCone(65, 60.Degrees())) { }
}

class TrekShot2 : Components.SelfTargetedAOEs
{
    public TrekShot2() : base(ActionID.MakeSpell(AID.TrekShot2), new AOEShapeCone(65, 60.Degrees())) { }
}

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

class PitonPull : Components.LocationTargetedAOEs
{
    public PitonPull() : base(ActionID.MakeSpell(AID.PitonPull), 22) { }
}

class Altitude : Components.LocationTargetedAOEs
{
    public Altitude() : base(ActionID.MakeSpell(AID.Altitude), 6) { }
}

class DownhillSmall : Components.LocationTargetedAOEs
{
    public DownhillSmall() : base(ActionID.MakeSpell(AID.DownhillSmall), 6) { }
}

class DownhillBig : Components.LocationTargetedAOEs
{
    public DownhillBig() : base(ActionID.MakeSpell(AID.DownhillBig), 8) { }
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
