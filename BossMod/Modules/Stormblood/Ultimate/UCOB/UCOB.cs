namespace BossMod.Stormblood.Ultimate.UCOB;

class P1Plummet(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Plummet), new AOEShapeCone(12, 60.Degrees()), (uint)OID.Twintania);
class P1Fireball(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Fireball, ActionID.MakeSpell(AID.Fireball), 4, 5.3f, 4);
class P2BahamutsClaw(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.BahamutsClaw));
class P3FlareBreath(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.FlareBreath), new AOEShapeCone(29.2f, 45.Degrees()), (uint)OID.BahamutPrime); // TODO: verify angle
class P5MornAfah(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.MornAfah), 4, 8); // TODO: verify radius

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.Twintania, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 280)]
public class UCOB(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(21))
{
    private Actor? _nael;
    private Actor? _bahamutPrime;

    public Actor? Twintania() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? Nael() => _nael;
    public Actor? BahamutPrime() => _bahamutPrime;

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
