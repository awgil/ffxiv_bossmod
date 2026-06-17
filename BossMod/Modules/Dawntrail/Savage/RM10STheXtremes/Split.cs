namespace BossMod.Dawntrail.Savage.RM10STheXtremes;

class FlameFloaterSplit(BossModule module) : Components.GenericAOEs(module, AID.FlameFloaterSplitCast)
{
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(new AOEShapeRect(60, 4), Module.PrimaryActor.Position, default, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _activation = Module.CastFinishAt(spell, 0.2f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = default;
        }
    }
}

class SplitPuddle(BossModule module) : FlamePuddle(module, AID.FlameFloaterSplitCast, new AOEShapeRect(60, 4), OID.FlameFloater);
class FreakyPyrotation(BossModule module) : Components.StackWithIcon(module, (uint)IconID.FreakyPyrotation, AID.FreakyPyrotationStack, 6, 5.3f, 2, 2);
class FreakyPyrotationPuddle(BossModule module) : FlamePuddle(module, AID.FreakyPyrotationStack, new AOEShapeCircle(6), OID.FlamePuddle6, true);
