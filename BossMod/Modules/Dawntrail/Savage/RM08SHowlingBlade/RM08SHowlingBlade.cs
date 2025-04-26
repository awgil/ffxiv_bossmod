namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class TrackingTremorsStack(BossModule module) : Components.UniformStackSpread(module, 6, 0, 8)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.TrackingTremors)
            AddStack(actor, WorldState.FutureTime(5.9f));
    }
}

class TrackingTremors(BossModule module) : Components.CastCounter(module, AID.TrackingTremors);

class ExtraplanarPursuit(BossModule module) : Components.RaidwideCastDelay(module, AID.ExtraplanarPursuitVisual, AID.ExtraplanarPursuit, 2.4f);

abstract class PlayActionAOEs(BossModule module, uint oid, ushort eventId, AOEShape shape, AID action, float activationDelay) : Components.GenericAOEs(module, action)
{
    protected readonly List<(Actor Actor, DateTime Activation)> Casters = [];

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == oid && id == eventId)
            Casters.Add((actor, WorldState.FutureTime(activationDelay)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Casters.RemoveAll(c => c.Actor == caster);
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(shape, c.Actor.Position, c.Actor.Rotation, c.Activation));
}

#if DEBUG
[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1026, NameID = 13843, PlanLevel = 100)]
public class RM08SHowlingBlade(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(12, MapResolution))
{
    public const float MapResolution = 0.25f;
}
#endif
