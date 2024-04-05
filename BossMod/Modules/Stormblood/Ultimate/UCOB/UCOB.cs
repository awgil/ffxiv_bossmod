namespace BossMod.Stormblood.Ultimate.UCOB;

class P1Plummet : Components.Cleave
{
    public P1Plummet() : base(ActionID.MakeSpell(AID.Plummet), new AOEShapeCone(12, 60.Degrees()), (uint)OID.Twintania) { }
}

class P1Fireball : Components.StackWithIcon
{
    public P1Fireball() : base((uint)IconID.Fireball, ActionID.MakeSpell(AID.Fireball), 4, 5.3f, 4) { }
}

class P2BahamutsClaw : Components.CastCounter
{
    public P2BahamutsClaw() : base(ActionID.MakeSpell(AID.BahamutsClaw)) { }
}

class P3FlareBreath : Components.Cleave
{
    public P3FlareBreath() : base(ActionID.MakeSpell(AID.FlareBreath), new AOEShapeCone(29.2f, 45.Degrees()), (uint)OID.BahamutPrime) { } // TODO: verify angle
}

class P5MornAfah : Components.StackWithCastTargets
{
    public P5MornAfah() : base(ActionID.MakeSpell(AID.MornAfah), 4, 8) { } // TODO: verify radius
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.Twintania, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 280)]
public class UCOB : BossModule
{
    private Actor? _nael;
    private Actor? _bahamutPrime;

    public Actor? Twintania() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? Nael() => _nael;
    public Actor? BahamutPrime() => _bahamutPrime;

    public UCOB(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 0), 21)) { }

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _nael ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.NaelDeusDarnus).FirstOrDefault() : null;
        _bahamutPrime ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.BahamutPrime).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(Twintania(), ArenaColor.Enemy);
        Arena.Actor(Nael(), ArenaColor.Enemy);
        Arena.Actor(BahamutPrime(), ArenaColor.Enemy);
    }
}
