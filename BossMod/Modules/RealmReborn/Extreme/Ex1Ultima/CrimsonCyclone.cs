namespace BossMod.RealmReborn.Extreme.Ex1Ultima;

class CrimsonCyclone(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.CrimsonCyclone))
{
    private Actor? _ifrit; // non-null while mechanic is active
    private DateTime _resolve;

    public bool Active => _ifrit != null;

    private static readonly AOEShapeRect _shape = new(43, 6, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_ifrit != null)
            yield return new(_shape, _ifrit.Position, _ifrit.Rotation, _resolve);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _ifrit = caster;
            _resolve = spell.NPCFinishAt;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _ifrit = null;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.UltimaIfrit && id == 0x008D)
        {
            _ifrit = actor;
            _resolve = WorldState.FutureTime(5);
        }
    }
}
