namespace BossMod.Endwalker.Alliance.A34Eulogia;

class RadiantFlourish : Components.GenericAOEs
{
    private static readonly AOEShapeCircle circle = new(27);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SolarFansAOE)
            _aoes.Add(new(circle, spell.LocXZ, activation: module.WorldState.CurrentTime.AddSeconds(13.7f)));
        if ((AID)spell.Action.ID == AID.RadiantFlourish)
            _aoes.Clear();
    }
}

class RadiantRhythm : Components.GenericAOEs
{
    private static readonly AOEShapeDonutSector _shape = new(20, 30, 45.Degrees());
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<AOEInstance> _aoes2 = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            yield return new(_aoes[0].Shape, _aoes[0].Origin, _aoes[0].Rotation, _aoes[0].Activation, ArenaColor.Danger);
            yield return new(_aoes2[0].Shape, _aoes2[0].Origin, _aoes2[0].Rotation, _aoes2[0].Activation, ArenaColor.Danger);
        }
        if (_aoes.Count > 1)
        {
            yield return new(_aoes[1].Shape, _aoes[1].Origin, _aoes[1].Rotation, _aoes[1].Activation);
            yield return new(_aoes2[1].Shape, _aoes2[1].Origin, _aoes2[1].Rotation, _aoes2[1].Activation);
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var _activation = spell.NPCFinishAt.AddSeconds(2.7f);
        if ((AID)spell.Action.ID == AID.SolarFansAOE)
        {
            if (spell.LocXZ.AlmostEqual(new(970, -945), 1))
                for (int i = 1; i < 5; ++i)
                {
                    _aoes.Add(new(_shape, module.Bounds.Center, (225 + i * 90).Degrees(), _activation.AddSeconds(2.1f * (i - 1))));
                    _aoes2.Add(new(_shape, module.Bounds.Center, (45 + i * 90).Degrees(), _activation.AddSeconds(2.1f * (i - 1))));
                }
            if (spell.LocXZ.AlmostEqual(new(945, -970), 1))
                for (int i = 1; i < 5; ++i)
                {
                    _aoes.Add(new(_shape, module.Bounds.Center, (135 + i * 90).Degrees(), _activation.AddSeconds(2.1f * (i - 1))));
                    _aoes2.Add(new(_shape, module.Bounds.Center, (-45 + i * 90).Degrees(), _activation.AddSeconds(2.1f * (i - 1))));
                }
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.RadiantFlight)
        {
            if (++NumCasts % 2 == 0)
            {
                _aoes.RemoveAt(0);
                _aoes2.RemoveAt(0);
            }
        }
    }
}

class SolarFans : Components.ChargeAOEs
{
    public SolarFans() : base(ActionID.MakeSpell(AID.SolarFansAOE), 5) { }
}
