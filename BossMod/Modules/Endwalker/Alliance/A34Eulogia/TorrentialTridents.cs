namespace BossMod.Endwalker.Alliance.A34Eulogia;

sealed class TorrentialTrident(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [with(6)];
    private static readonly AOEShapeCircle _shape = new(18f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var max = count > 5 ? 5 : count;
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        if (count > 1)
        {
            ref var aoe0 = ref aoes[0];
            aoe0.Color = Colors.Danger;
        }
        return aoes[..max];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TorrentialTridentLanding:
                AOEs.Add(new(_shape, caster.Position.Quantized(), default, WorldState.FutureTime(13.6d)));
                break;
            case (uint)AID.LightningBolt:
                ++NumCasts;
                if (AOEs.Count != 0)
                {
                    AOEs.RemoveAt(0);
                }
                break;
        }
    }
}
