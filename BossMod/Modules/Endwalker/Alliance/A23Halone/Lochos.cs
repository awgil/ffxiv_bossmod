namespace BossMod.Endwalker.Alliance.A23Halone;

abstract class Lochos(BossModule module, double activationDelay) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly double _activationDelay = activationDelay;

    private readonly AOEShapeRect _shape = new(60f, 15f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.LochosFirst or (uint)AID.LochosRest)
        {
            ++NumCasts;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00200010u)
        {
            (var offset, var dir) = index switch
            {
                8 => (new(+15f, -30f), default),
                9 => (new(-30f, -15f), 90f.Degrees()),
                10 => (new(+30f, +15), -90f.Degrees()),
                11 => (new(-15f, +30f), 180f.Degrees()),
                _ => (new WDir(), new Angle())
            };
            if (offset != default)
            {
                _aoes.Add(new(_shape, (Arena.Center + offset).Quantized(), dir, WorldState.FutureTime(_activationDelay)));
            }
        }
    }
}

sealed class Lochos1(BossModule module) : Lochos(module, 10.9d);
sealed class Lochos2(BossModule module) : Lochos(module, 14.8d);
