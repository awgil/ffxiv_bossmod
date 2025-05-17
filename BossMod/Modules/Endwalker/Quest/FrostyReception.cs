namespace BossMod.Endwalker.Quest.AFrostyReception;

public enum OID : uint
{
    Boss = 0x3646,
    Helper = 0x233C,
    LockOn = 0x3648, // R1.200, x0 (spawn during fight)
}

public enum AID : uint
{
    GigaTempest = 27440, // Boss->self, 5.0s cast, range 20 circle
    Ruination1 = 27443, // Boss->self, 5.0s cast, range 40 width 8 cross
    Ruination2 = 27444, // Helper->self, 5.0s cast, range 30 width 8 rect
    ResinBomb = 27449, // Helper->Lyse/Magnai/Sadu/Pipin/Lucia/Cirina, 5.0s cast, range 5 circle
    LockOn1 = 27461, // _Gen_6->self, 1.0s cast, range 6 circle
    MagitekCannon = 27457, // _Gen_TelotekReaper1->player/Lyse/Sadu/Magnai/Lucia/Cirina/Pipin, 5.0s cast, range 6 circle
    Bombardment = 27459, // Helper->location, 4.0s cast, range 6 circle
    LockOn2 = 27463, // _Gen_6->self, 1.0s cast, range 6 circle
}

class GigaTempest(BossModule module) : Components.RaidwideCast(module, AID.GigaTempest);
class Ruination(BossModule module) : Components.StandardAOEs(module, AID.Ruination1, new AOEShapeCross(40, 4));
class Ruination2(BossModule module) : Components.StandardAOEs(module, AID.Ruination2, new AOEShapeRect(30, 4));
class ResinBomb(BossModule module) : Components.SpreadFromCastTargets(module, AID.ResinBomb, 5);
class MagitekCannon(BossModule module) : Components.SpreadFromCastTargets(module, AID.MagitekCannon, 6);
class Bombardment(BossModule module) : Components.StandardAOEs(module, AID.Bombardment, 6);

class LockOn(BossModule module) : Components.GenericAOEs(module)
{
    private class Caster
    {
        public required Actor Actor;
        public required DateTime FinishAt;
    }

    private readonly List<Caster> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeCircle(6), c.Actor.Position, default, c.FinishAt));

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.LockOn)
            Casters.Add(new() { Actor = actor, FinishAt = WorldState.FutureTime(6.6f) });
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.LockOn1 or AID.LockOn2)
        {
            var c = Casters.FindIndex(p => p.Actor == caster);
            if (c >= 0)
                Casters[c].FinishAt = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.LockOn1 or AID.LockOn2)
            Casters.RemoveAll(c => c.Actor == caster);
    }
}

class VergiliaVanCorculumStates : StateMachineBuilder
{
    public VergiliaVanCorculumStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GigaTempest>()
            .ActivateOnEnter<Ruination>()
            .ActivateOnEnter<Ruination2>()
            .ActivateOnEnter<LockOn>()
            .ActivateOnEnter<ResinBomb>()
            .ActivateOnEnter<MagitekCannon>()
            .ActivateOnEnter<Bombardment>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69919, NameID = 10572)]
public class VergiliaVanCorculum(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -78), new ArenaBoundsCircle(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
