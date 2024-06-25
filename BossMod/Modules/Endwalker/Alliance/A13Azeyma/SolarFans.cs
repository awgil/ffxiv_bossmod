namespace BossMod.Endwalker.Alliance.A13Azeyma;

class SolarFans(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.SolarFansAOE), 5);

class RadiantRhythm(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.RadiantFlight))
{
    private static readonly AOEShapeDonutSector _shape = new(20, 30, 45.Degrees());
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<AOEInstance> _aoes2 = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
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

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var _activation = spell.NPCFinishAt.AddSeconds(7.7f);
        if ((AID)spell.Action.ID == AID.SolarFansCharge) //since it seems impossible to determine early enough if 5 or 6 casts happen, we draw one extra one just incase
        {
            if (spell.LocXZ.AlmostEqual(new(-775, -750), 10))
                for (var i = 1; i < 6; ++i)
                {
                    _aoes.Add(new(_shape, Arena.Center, (225 + i * 90).Degrees(), _activation.AddSeconds(1.3f * (i - 1))));
                    _aoes2.Add(new(_shape, Arena.Center, (45 + i * 90).Degrees(), _activation.AddSeconds(1.3f * (i - 1))));
                }
            if (spell.LocXZ.AlmostEqual(new(-750, -775), 1))
                for (var i = 1; i < 6; ++i)
                {
                    _aoes.Add(new(_shape, Arena.Center, (135 + i * 90).Degrees(), _activation.AddSeconds(1.3f * (i - 1))));
                    _aoes2.Add(new(_shape, Arena.Center, (-45 + i * 90).Degrees(), _activation.AddSeconds(1.3f * (i - 1))));
                }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RadiantFinish)
        {
            _aoes.Clear();
            _aoes2.Clear();
        }
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.RadiantFlight && ++NumCasts % 2 == 0)
        {
            _aoes.RemoveAt(0);
            _aoes2.RemoveAt(0);
        }
    }
}

class RadiantFlourish(BossModule module) : Components.GenericAOEs(module)
{
    private int teleportcounter;
    private static readonly AOEShapeCircle circle = new(25);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SolarFansAOE)
            _aoes.Add(new(circle, spell.LocXZ, default, WorldState.FutureTime(16.6f)));
        if ((AID)spell.Action.ID == AID.RadiantFlourish)
        {
            _aoes.Clear();
            teleportcounter = 0;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TeleportFlame) //correct circle location if Flight happens 10 times instead of 8 times, ugly hack but i couldn't find a better difference in logs
        {
            if (++teleportcounter > 8)
            {
                teleportcounter = 0;
                _aoes.Add(new(circle, Helpers.RotateAroundOrigin(90, Arena.Center, _aoes[0].Origin), default, _aoes[0].Activation.AddSeconds(1.4f)));
                _aoes.Add(new(circle, Helpers.RotateAroundOrigin(90, Arena.Center, _aoes[1].Origin), default, _aoes[1].Activation.AddSeconds(1.4f)));
                _aoes.RemoveAt(0);
                _aoes.RemoveAt(0);
            }
        }
    }
}
