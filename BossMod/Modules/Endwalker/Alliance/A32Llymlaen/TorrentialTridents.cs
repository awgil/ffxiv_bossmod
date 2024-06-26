namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class TorrentialTridentLanding(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.TorrentialTridentLanding));

class TorrentialTridentAOE(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    private static readonly AOEShapeCircle _shape = new(18);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var aoe in AOEs.Skip(1).Take(4))
            yield return aoe;
        if (AOEs.Count > 0)
            yield return AOEs[0] with { Color = ArenaColor.Danger };
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TorrentialTridentLanding:
                AOEs.Add(new(_shape, caster.Position, default, WorldState.FutureTime(13.8f)));
                break;
            case AID.TorrentialTridentAOE:
                ++NumCasts;
                if (AOEs.Count > 0)
                    AOEs.RemoveAt(0);
                break;
        }
    }
}
