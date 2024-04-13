namespace BossMod.Endwalker.Savage.P8S2;

class AshingBlaze(BossModule module) : Components.GenericAOEs(module)
{
    private WPos? _origin;
    private static readonly AOEShapeRect _shape = new(46, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_origin != null)
            yield return new(_shape, _origin.Value, 0.Degrees(), Module.PrimaryActor.CastInfo?.NPCFinishAt ?? default);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AshingBlazeL:
                _origin = caster.Position - new WDir(_shape.HalfWidth, 0);
                break;
            case AID.AshingBlazeR:
                _origin = caster.Position + new WDir(_shape.HalfWidth, 0);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AshingBlazeL or AID.AshingBlazeR)
            _origin = null;
    }
}
