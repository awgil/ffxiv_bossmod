namespace BossMod.Dawntrail.Savage.RM10STheXtremes;

class FlameFloaterSplit(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_FlameFloater5)
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

class SplitPuddle(BossModule module) : FlamePuddle(module, AID._Weaponskill_FlameFloater5, new AOEShapeRect(60, 4), OID.FlameFloater);
class FreakyPyrotation(BossModule module) : Components.StackWithIcon(module, (uint)IconID._Gen_Icon_com_share_fire01s5_0c, AID._Weaponskill_FreakyPyrotation1, 6, 5.3f, 2, 2);
class FreakyPyrotationPuddle(BossModule module) : FlamePuddle(module, AID._Weaponskill_FreakyPyrotation1, new AOEShapeCircle(6), OID.FlamePuddle6, true);
