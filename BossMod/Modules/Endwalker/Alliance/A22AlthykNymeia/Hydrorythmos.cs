namespace BossMod.Endwalker.Alliance.A22AlthykNymeia;

class Hydrorythmos(BossModule module) : Components.GenericAOEs(module)
{
    private Angle _dir;
    private DateTime _activation;

    private static readonly AOEShapeRect _shapeFirst = new(25, 5, 25);
    private static readonly AOEShapeRect _shapeRest = new(25, 2.5f, 25);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NumCasts > 0)
        {
            var offset = ((NumCasts + 1) >> 1) * 5 * _dir.ToDirection().OrthoL();
            yield return new(_shapeRest, Module.Center + offset, _dir, _activation);
            yield return new(_shapeRest, Module.Center - offset, _dir, _activation);
        }
        else if (_activation != default)
        {
            yield return new(_shapeFirst, Module.Center, _dir, _activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HydrorythmosFirst)
        {
            _dir = spell.Rotation;
            _activation = spell.NPCFinishAt;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HydrorythmosFirst or AID.HydrorythmosRest)
        {
            ++NumCasts;
            _activation = WorldState.FutureTime(2.1f);
        }
    }
}
