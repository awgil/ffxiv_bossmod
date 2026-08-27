namespace BossMod.Stormblood.Ultimate.UCOB;

class P1Plummet : Components.Cleave
{
    public P1Plummet(BossModule module) : base(module, AID.Plummet, new AOEShapeCone(12, 60.Degrees()), (uint)OID.Twintania)
    {
        NextExpected = DateTime.MaxValue;
    }
}
class P1Fireball(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Fireball, AID.Fireball, 4, 5.3f, 4)
{
    int _neurolinkCount = 0;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == StackIcon)
        {
            if (_neurolinkCount == 0)
                AddStack(actor, WorldState.FutureTime(5.3f), Raid.WithSlot().WhereActor(a => a.Role == Role.Tank).Mask());
            else
                AddStack(actor, WorldState.FutureTime(7.4f));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Neurolink)
            _neurolinkCount++;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_neurolinkCount > 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        if (!EnableHints || Stacks.Count == 0)
            return;

        var stack = Stacks[0];

        var stackDestination = Module.PrimaryActor.Position + new WDir(0, Module.PrimaryActor.HitboxRadius + 3);

        if (stack.Target == actor) // stack target shouldn't move around too much, just plant on boss
        {
            hints.AddForbiddenZone(ShapeDistance.PrecisePosition(stackDestination, new(0, 1), 0.5f, actor.Position, 0.1f), stack.Activation);
        }
        else if (!stack.ForbiddenPlayers[slot])
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(stackDestination, stack.Radius - 1), stack.Activation);
        else
            hints.AddForbiddenZone(ShapeDistance.Circle(stackDestination, stack.Radius + 0.5f), stack.Activation);
    }
}
class P2BahamutsClaw(BossModule module) : Components.CastCounter(module, AID.BahamutsClaw);
class P3FlareBreath(BossModule module) : Components.Cleave(module, AID.FlareBreath, new AOEShapeCone(29.2f, 45.Degrees()), (uint)OID.BahamutPrime); // TODO: verify angle
class P5MornAfah(BossModule module) : Components.StackWithCastTargets(module, AID.MornAfah, 4, 8); // TODO: verify radius

[ModuleInfo(PrimaryActorOID = (uint)OID.Twintania, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 280, PlanLevel = 70)]
public class UCOB(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(21))
{
    private Actor? _nael;
    private Actor? _bahamutPrime;

    public Actor? Twintania() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? Nael() => _nael;
    public Actor? BahamutPrime() => _bahamutPrime;

    public override bool ShouldPrioritizeAllEnemies => true;

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
