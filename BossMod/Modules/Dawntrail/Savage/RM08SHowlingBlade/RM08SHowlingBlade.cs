namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class TrackingTremorsStack(BossModule module) : Components.UniformStackSpread(module, 6, 0, 8)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.MultiStack)
            AddStack(actor, WorldState.FutureTime(5.9f));
    }
}

class TrackingTremors(BossModule module) : Components.CastCounter(module, AID.TrackingTremors);

class ExtraplanarPursuit(BossModule module) : Components.RaidwideCastDelay(module, AID.ExtraplanarPursuitVisual, AID.ExtraplanarPursuit, 2.4f);

abstract class PlayActionAOEs(BossModule module, uint oid, ushort eventId, AOEShape shape, AID action, float activationDelay, bool actorIsCaster = true) : Components.GenericAOEs(module, action)
{
    public bool Risky = true;
    protected readonly List<(Actor Actor, DateTime Activation)> _casters = [];

    public IEnumerable<Actor> Casters => _casters.Select(c => c.Actor);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == oid && id == eventId)
            _casters.Add((actor, WorldState.FutureTime(activationDelay)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (actorIsCaster)
                _casters.RemoveAll(c => c.Actor == caster);
            else
                _casters.RemoveAt(0);
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(shape, c.Actor.Position, c.Actor.Rotation, c.Activation, Risky: Risky));
}

class ProwlingGale2(BossModule module) : Components.CastTowers(module, AID.ProwlingGale2, 2, 2, 2);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1026, NameID = 13843, PlanLevel = 100)]
public class RM08SHowlingBlade(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(12, MapResolution))
{
    public const float MapResolution = 0.25f;

    public static readonly ArenaBoundsCustom BoundsP2 = MakeBoundsP2();

    public static ArenaBoundsCustom MakeBoundsP2(BitMask missingPlatforms = default)
    {
        var p = new RelSimplifiedComplexPolygon();
        for (var i = 0; i < 5; i++)
        {
            if (missingPlatforms[i])
                continue;
            p.Parts.Add(new([.. CurveApprox.Circle(8, 1 / 90f).Select(t => t + (72.Degrees() * -i).ToDirection() * 17.5f)]));
        }
        return new(23.5f, p);
    }

    public Actor? BossP1() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? BossP2() => _bossP2;

    private Actor? _bossP2;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _bossP2 ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.BossP2).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        if (_bossP2?.IsTargetable == true)
            Arena.ActorInsideBounds(_bossP2.Position, _bossP2.Rotation, ArenaColor.Enemy);
    }
}
