namespace BossMod.Dawntrail.Extreme.Ex3Sphene;

class LegitimateForce(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeRect _shape = new(60, 15);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var dx = (AID)spell.Action.ID switch
        {
            AID.LegitimateForceFirstR => -15,
            AID.LegitimateForceFirstL => +15,
            _ => 0
        };
        if (dx != 0)
        {
            AOEs.Add(new(_shape, caster.Position + new WDir(dx, 0), default, Module.CastFinishAt(spell)));
            AOEs.Add(new(_shape, caster.Position - new WDir(dx, 0), default, Module.CastFinishAt(spell, 3.1f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.LegitimateForceFirstR or AID.LegitimateForceFirstL or AID.LegitimateForceSecondR or AID.LegitimateForceSecondL)
        {
            ++NumCasts;
            if (AOEs.Count > 0)
                AOEs.RemoveAt(0);
        }
    }
}
