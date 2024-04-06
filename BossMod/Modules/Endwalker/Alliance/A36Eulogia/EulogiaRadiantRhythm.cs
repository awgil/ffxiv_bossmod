namespace BossMod.Endwalker.Alliance.A36Eulogia;

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

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_aoes.Count > 1)
            for (int i = 0; i < 2; ++i)
                yield return new(_aoes[i].Shape, _aoes[i].Origin, _aoes[i].Rotation, _aoes[i].Activation, ArenaColor.Danger);
        if (_aoes.Count > 3)
            for (int i = 2; i < 4; ++i)
                yield return new(_aoes[i].Shape, _aoes[i].Origin, _aoes[i].Rotation, _aoes[i].Activation);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var _activation = spell.NPCFinishAt.AddSeconds(2.7f);
        if ((AID)spell.Action.ID == AID.SolarFansAOE)
        {
            if (spell.LocXZ.AlmostEqual(new(970, -945), 1))
                for (int i = 1; i < 5; ++i)
                {
                    _aoes.Add(new(_shape, new(945, -945), (225 + i * 90).Degrees(), _activation.AddSeconds(i > 1 ? 2.1f * i : 0)));
                    _aoes.Add(new(_shape, new(945, -945), (45 + i * 90).Degrees(), _activation.AddSeconds(i > 1 ? 2.1f * i : 0)));
                }
            if (spell.LocXZ.AlmostEqual(new(945, -970), 1))
                for (int i = 1; i < 5; ++i)
                {
                    _aoes.Add(new(_shape, new(945, -945), (135 + i * 90).Degrees(), _activation.AddSeconds(i > 1 ? 2.1f * i : 0)));
                    _aoes.Add(new(_shape, new(945, -945), (-45 + i * 90).Degrees(), _activation.AddSeconds(i > 1 ? 2.1f * i : 0)));
                }
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.RadiantFlight)
            _aoes.RemoveAt(0);
    }
}
