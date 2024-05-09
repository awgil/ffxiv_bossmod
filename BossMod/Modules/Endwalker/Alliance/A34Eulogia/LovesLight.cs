namespace BossMod.Endwalker.Alliance.A34Eulogia;

class LovesLight(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<(WPos position, Angle rotation, DateTime activation)> AOEs = [];
    private static readonly AOEShapeRect _shape = new(80, 12.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (AOEs.Count > 1)
            yield return new(_shape, AOEs[1].position, AOEs[1].rotation, AOEs[1].activation);
        if (AOEs.Count > 0)
            yield return new(_shape, AOEs[0].position, AOEs[0].rotation, AOEs[0].activation, ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FirstBlush1 or AID.FirstBlush2 or AID.FirstBlush3 or AID.FirstBlush4)
        {
            AOEs.Add((caster.Position, spell.Rotation, spell.NPCFinishAt));
            AOEs.SortBy(aoe => aoe.activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FirstBlush1 or AID.FirstBlush2 or AID.FirstBlush3 or AID.FirstBlush4)
        {
            ++NumCasts;
            if (AOEs.Count > 0)
                AOEs.RemoveAt(0);
        }
    }
}
