namespace BossMod.Endwalker.Alliance.A23Halone;

class Lochos : Components.GenericAOEs
{
    private List<AOEInstance> _aoes = new();
    private float _activationDelay;

    private static readonly AOEShapeRect _shape = new(60, 15);

    public Lochos(float activationDelay)
    {
        _activationDelay = activationDelay;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.LochosFirst or AID.LochosRest)
        {
            if (!_aoes.Any(aoe => aoe.Origin.AlmostEqual(caster.Position, 1) && aoe.Rotation.AlmostEqual(caster.Rotation, 0.1f)))
                module.ReportError(this, "Unexpected caster position/rotation");
            ++NumCasts;
        }
    }

    public override void OnEventEnvControl(BossModule module, byte index, uint state)
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
                _aoes.Add(new(_shape, module.Bounds.Center + offset, dir, module.WorldState.CurrentTime.AddSeconds(_activationDelay)));
            }
        }
    }
}

class Lochos1 : Lochos { public Lochos1() : base(10.9f) { } }
class Lochos2 : Lochos { public Lochos2() : base(14.8f) { } }
