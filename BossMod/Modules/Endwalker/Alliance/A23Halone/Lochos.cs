namespace BossMod.Endwalker.Alliance.A23Halone;

class Lochos(BossModule module, float activationDelay) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly float _activationDelay = activationDelay;

    private static readonly AOEShapeRect _shape = new(60, 15);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.LochosFirst or AID.LochosRest)
        {
            if (!_aoes.Any(aoe => aoe.Origin.AlmostEqual(caster.Position, 1) && aoe.Rotation.AlmostEqual(caster.Rotation, 0.1f)))
                ReportError("Unexpected caster position/rotation");
            ++NumCasts;
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00200010)
        {
            (WDir offset, Angle dir) = index switch
            {
                8 => (new(+15, -30), 0.Degrees()),
                9 => (new(-30, -15), 90.Degrees()),
                10 => (new(+30, +15), -90.Degrees()),
                11 => (new(-15, +30), 180.Degrees()),
                _ => (new WDir(), 0.Degrees())
            };
            if (offset != default)
            {
                _aoes.Add(new(_shape, Module.Center + offset, dir, WorldState.FutureTime(_activationDelay)));
            }
        }
    }
}
class Lochos1(BossModule module) : Lochos(module, 10.9f);
class Lochos2(BossModule module) : Lochos(module, 14.8f);
