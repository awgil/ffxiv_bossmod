namespace BossMod.Endwalker.Criterion.C02AMR.C021Shishio;

class Slither(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _caster;
    private DateTime _predictedActivation;

    private static readonly AOEShapeCone _shape = new(25, 45.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_caster?.CastInfo != null)
            yield return new(_shape, _caster.Position, _caster.CastInfo.Rotation, _caster.CastInfo.NPCFinishAt);
        else if (_predictedActivation != default)
            yield return new(_shape, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + 180.Degrees(), _predictedActivation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NSlither:
            case AID.SSlither:
                _caster = caster;
                _predictedActivation = default;
                break;
            case AID.NSplittingCry:
            case AID.SSplittingCry:
                _predictedActivation = spell.NPCFinishAt.AddSeconds(4.2f);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NSlither or AID.SSlither)
        {
            _caster = null;
            ++NumCasts;
        }
    }
}
