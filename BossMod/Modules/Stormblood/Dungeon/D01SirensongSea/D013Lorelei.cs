using BossMod.Components;

namespace BossMod.Stormblood.Dungeon.D01SirensongSea.D013Lorelei;

public enum OID : uint
{
    Boss = 0x1AFE,   // R3.360, x?
    GenExit = 0x1E850B, // R0.500, x?, EventObj type
    GenActor1e8f2f = 0x1E8F2F, // R0.500, x?, EventObj type
    GenActor1ea2f6 = 0x1EA2F6, // R2.000, x?, EventObj type
    GenActor1e8fb8 = 0x1E8FB8, // R2.000, x?, EventObj type
    GenActor1ea2ff = 0x1EA2FF, // R2.000, x?, EventObj type
    GenActor1ea2f7 = 0x1EA2F7, // R2.000, x?, EventObj type
    GenActor1ea300 = 0x1EA300, // R0.500, x?, EventObj type
}

public enum AID : uint
{
    VoidWater = 8040,
    IllWill = 8035,         // Boss->player, no cast, single-target
    VirginTears = 8041, // Boss->self, 3.0s cast, single-target
    MorbidAdvance = 8037, // Boss->self, 5.0s cast, range 80+R circle
    HeadButt = 8036, // Boss->player, no cast, single-target
    SomberMelody = 8039, // Boss->self, 4.0s cast, range 80+R circle
    MorbidRetreat = 8038, // Boss->self, 5.0s cast, range 80+R circle
}

public enum SID : uint
{
    ForcedMarchBackwards = 3629, // Boss->player, extra=0x1/0x2
    ForcedMarchForwards = 1257, // Boss->player, extra=0x1/0x2
    Bleeding = 320,  // none->player, extra=0x0
}

class ArenaLimit(BossModule module) : GenericAOEs(module)
{
    private AOEInstance? donut;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        donut ??= new AOEInstance(new AOEShapeDonut(15, 25), Module.Center);

        yield return (AOEInstance)donut;
    }
}

class VoidWater(BossModule module) : StandardAOEs(module, AID.VoidWater, 8);

class Puddles(BossModule module) : GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.GenActor1ea300)
        {
            aoes.Add(new AOEInstance(new AOEShapeCircle(7f), actor.Position));
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.GenActor1ea300)
        {
            aoes.Remove(aoes.First());
        }
    }
}

class Morbid(BossModule module) : GenericForcedMarch(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MorbidAdvance)
        {
            AddForcedMovement(Raid.Player()!, 0.Degrees(), 3, WorldState.FutureTime(spell.RemainingTime));
        }
        if ((AID)spell.Action.ID == AID.MorbidRetreat)
        {
            AddForcedMovement(Raid.Player()!, 180.Degrees(), 3, WorldState.FutureTime(spell.RemainingTime));
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.ForcedMarchBackwards or SID.ForcedMarchForwards && actor == Raid.Player())
        {
            State.GetOrAdd(actor.InstanceID).PendingMoves.Clear();
            ActivateForcedMovement(Raid.Player()!, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.ForcedMarchBackwards or SID.ForcedMarchForwards && actor == Raid.Player())
        {
            DeactivateForcedMovement(Raid.Player()!);
        }
    }
}

class D013LoreleiStates : StateMachineBuilder
{
    public D013LoreleiStates(BossModule module) : base(module)
    {
        TrivialPhase().
            ActivateOnEnter<Puddles>().
            ActivateOnEnter<ArenaLimit>().
            ActivateOnEnter<VoidWater>().
            ActivateOnEnter<Morbid>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "erdelf", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 238, NameID = 6074)]
public class D013Lorelei(WorldState ws, Actor primary) : BossModule(ws, primary, new WPos(-44.564f, 465.154f), new ArenaBoundsCircle(25))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(Enemies(OID.Boss), ArenaColor.Enemy);
}
