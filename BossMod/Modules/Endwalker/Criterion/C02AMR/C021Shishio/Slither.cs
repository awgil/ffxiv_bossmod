namespace BossMod.Endwalker.VariantCriterion.C02AMR.C021Shishio;

sealed class Slither(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _caster;
    private DateTime _predictedActivation;

    private readonly AOEShapeCone _shape = new(25f, 45f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_caster?.CastInfo != null)
            return new AOEInstance[1] { new(_shape, _caster.Position, _caster.CastInfo.Rotation, Module.CastFinishAt(_caster.CastInfo)) };
        else if (_predictedActivation != default)
            return new AOEInstance[1] { new(_shape, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + 180f.Degrees(), _predictedActivation) };
        else
            return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NSlither:
            case (uint)AID.SSlither:
                _caster = caster;
                _predictedActivation = default;
                break;
            case (uint)AID.NSplittingCry:
            case (uint)AID.SSplittingCry:
                _predictedActivation = Module.CastFinishAt(spell, 4.2d);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NSlither or (uint)AID.SSlither)
        {
            _caster = null;
            ++NumCasts;
        }
    }
}
