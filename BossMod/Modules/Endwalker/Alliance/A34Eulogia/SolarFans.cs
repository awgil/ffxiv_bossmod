namespace BossMod.Endwalker.Alliance.A34Eulogia;

class SolarFans(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.SolarFansAOE), 5);

class RadiantRhythm(BossModule module) : Components.GenericAOEs(module)
{
    private Angle _nextAngle;
    private DateTime _activation;
    private static readonly AOEShapeDonutSector _shape = new(20, 30, 45.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation == default)
            yield break;

        // assumption: we always have 4 moves
        if (NumCasts < 8)
        {
            yield return new(_shape, Module.Center, _nextAngle, _activation, ArenaColor.Danger);
            yield return new(_shape, Module.Center, _nextAngle + 180.Degrees(), _activation, ArenaColor.Danger);
        }
        if (NumCasts < 6)
        {
            var future = _activation.AddSeconds(2.1f);
            yield return new(_shape, Module.Center, _nextAngle + 90.Degrees(), future);
            yield return new(_shape, Module.Center, _nextAngle - 90.Degrees(), future);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SolarFansAOE:
                // assumption: flames always move CCW
                var startingAngle = Angle.FromDirection(spell.LocXZ - Module.Center);
                if (_nextAngle != default && !_nextAngle.AlmostEqual(startingAngle + 45.Degrees(), 0.1f) && !_nextAngle.AlmostEqual(startingAngle - 135.Degrees(), 0.1f))
                    ReportError($"Inconsistent starting angle: {_nextAngle} -> {startingAngle}");
                NumCasts = 0;
                _nextAngle = startingAngle + 45.Degrees();
                _activation = spell.NPCFinishAt.AddSeconds(2.8f);
                break;
            case AID.RadiantFlight:
                // verify our assumption
                if (!spell.Rotation.AlmostEqual(_nextAngle, 0.1f) && !spell.Rotation.AlmostEqual(_nextAngle + 180.Degrees(), 0.1f))
                    ReportError($"Unexpected angle: {spell.Rotation} vs {_nextAngle}");
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RadiantFlight)
        {
            ++NumCasts;
            if (NumCasts == 8)
            {
                _nextAngle = default;
                _activation = default;
            }
            else if ((NumCasts & 1) == 0)
            {
                _nextAngle += 90.Degrees();
                _activation = WorldState.FutureTime(2.1f);
            }
        }
    }
}

class RadiantFlourish(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle _shape = new(25);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // assumption: we always have 4 moves, so flames finish where they start
        switch ((AID)spell.Action.ID)
        {
            case AID.SolarFansAOE:
                _aoes.Add(new(_shape, spell.LocXZ, default, spell.NPCFinishAt.AddSeconds(13.8f)));
                break;
            case AID.RadiantFlourish:
                // verify the assumption
                if (!_aoes.Any(aoe => aoe.Origin.AlmostEqual(caster.Position, 1)))
                    ReportError($"Unexpected AOE position: {caster.Position}");
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RadiantFlourish)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}
