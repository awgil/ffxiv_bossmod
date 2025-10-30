namespace BossMod.Dawntrail.Foray.ForkedTower.FT04Magitaur;

class CriticalAxeblow(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;

    public bool Risky = true;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
        {
            yield return new(new AOEShapeCircle(20), Arena.Center, Activation: _activation, Risky: Risky);
            yield return new(FT04Magitaur.NotPlatforms, Arena.Center, Activation: _activation, Risky: Risky);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CriticalAxeblowCast)
            _activation = Module.CastFinishAt(spell, 1.3f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.CriticalAxeblowCircle)
        {
            NumCasts++;
            _activation = default;
        }
    }
}

class CriticalLanceblow(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;

    public bool Risky = true;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
        {
            yield return new AOEInstance(new AOEShapeDonut(10, 32), Arena.Center, Activation: _activation, Risky: Risky);
            yield return new AOEInstance(new AOEShapeRect(10, 10, 10), Arena.Center + new WDir(0, 14.5f), 45.Degrees(), _activation, Risky: Risky);
            yield return new AOEInstance(new AOEShapeRect(10, 10, 10), Arena.Center + new WDir(0, 14.5f).Rotate(120.Degrees()), 165.Degrees(), _activation, Risky: Risky);
            yield return new AOEInstance(new AOEShapeRect(10, 10, 10), Arena.Center + new WDir(0, 14.5f).Rotate(-120.Degrees()), -75.Degrees(), _activation, Risky: Risky);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CriticalLanceblowCast)
            _activation = Module.CastFinishAt(spell, 1.3f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.CriticalLanceblowDonut)
        {
            NumCasts++;
            _activation = default;
        }
    }
}

class CriticalCounter(BossModule module) : Components.CastCounterMulti(module, [AID.CriticalAxeblowCircle, AID.CriticalLanceblowDonut]);
