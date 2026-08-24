namespace BossMod.Dawntrail.Savage.RM01SBlackCat;

class PredaceousPounce(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCircle _impact = new(11);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Skip(NumCasts);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (delay, charge) = (AID)spell.Action.ID switch
        {
            AID.PredaceousPounceMove1 => (11.5f, true),
            AID.PredaceousPounceMove2 => (11.0f, false),
            AID.PredaceousPounceMove3 => (10.5f, true),
            AID.PredaceousPounceMove4 => (10.0f, false),
            AID.PredaceousPounceMove5 => (9.5f, true),
            AID.PredaceousPounceMove6 => (9.0f, false),
            AID.PredaceousPounceMove7 => (8.5f, true),
            AID.PredaceousPounceMove8 => (8.0f, false),
            AID.PredaceousPounceMove9 => (7.5f, true),
            AID.PredaceousPounceMove10 => (7.0f, false),
            AID.PredaceousPounceMove11 => (6.5f, true),
            AID.PredaceousPounceMove12 => (6.0f, false),
            _ => default
        };

        if (delay > 0)
        {
            if (charge)
            {
                var toDest = spell.LocXZ - caster.Position;
                AOEs.Add(new(new AOEShapeRect(toDest.Length(), 3), caster.Position, Angle.FromDirection(toDest), Module.CastFinishAt(spell, delay)));
            }
            else
            {
                AOEs.Add(new(_impact, caster.Position, default, Module.CastFinishAt(spell, delay)));
            }
            AOEs.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PredaceousPounceChargeAOEFirst or AID.PredaceousPounceImpactAOEFirst or AID.PredaceousPounceChargeAOERest or AID.PredaceousPounceImpactAOERest)
            ++NumCasts;
    }
}
