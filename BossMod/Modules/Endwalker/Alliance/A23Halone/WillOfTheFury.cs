namespace BossMod.Endwalker.Alliance.A23Halone;

class WillOfTheFury(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private const float _impactRadiusIncrement = 6;

    public bool Active => _aoe != null;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WillOfTheFuryAOE1)
        {
            UpdateAOE(spell.NPCFinishAt);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WillOfTheFuryAOE1 or AID.WillOfTheFuryAOE2 or AID.WillOfTheFuryAOE3 or AID.WillOfTheFuryAOE4 or AID.WillOfTheFuryAOE5)
        {
            ++NumCasts;
            UpdateAOE(WorldState.FutureTime(2));
        }
    }

    private void UpdateAOE(DateTime activation)
    {
        var outerRadius = (5 - NumCasts) * _impactRadiusIncrement;
        AOEShape? shape = NumCasts switch
        {
            < 4 => new AOEShapeDonut(outerRadius - _impactRadiusIncrement, outerRadius),
            4 => new AOEShapeCircle(outerRadius),
            _ => null
        };
        _aoe = shape != null ? new(shape, Module.Center, default, activation) : null;
    }
}
