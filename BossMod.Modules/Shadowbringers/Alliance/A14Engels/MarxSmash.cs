namespace BossMod.Shadowbringers.Alliance.A14Engels;

class MarxSmashLR(BossModule module) : Components.GenericAOEs(module)
{
    private Angle? _rotation;
    private DateTime _activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.MarxSmashLeftBoss:
                _rotation = 90.Degrees();
                _activation = Module.CastFinishAt(spell, 1.7f);
                break;
            case AID.MarxSmashRightBoss:
                _rotation = -90.Degrees();
                _activation = Module.CastFinishAt(spell, 1.7f);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.MarxSmashLeft or AID.MarxSmashRight)
        {
            NumCasts++;
            _rotation = null;
            _activation = default;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_rotation).Select(r => new AOEInstance(new AOEShapeRect(30, 30), Arena.Center, r, _activation));
}
class MarxSmashOutside(BossModule module) : Components.GenericAOEs(module, AID.MarxSmashOutsideFirst)
{
    private DateTime _activation1;
    private DateTime _activation2;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _activation1 = Module.CastFinishAt(spell, 1.6f);
            _activation2 = _activation1.AddSeconds(4.7f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.MarxSmashOutsideFirst:
                _activation1 = default;
                break;
            case AID.MarxSmashOutsideLeft:
            case AID.MarxSmashOutsideRight:
                _activation2 = default;
                break;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation1 != default)
            yield return new(new AOEShapeRect(30, 30, 5), Arena.Center, default, Activation: _activation1, Color: ArenaColor.Danger);
        if (_activation2 != default)
        {
            var color = _activation1 == default ? ArenaColor.Danger : ArenaColor.AOE;
            yield return new(new AOEShapeRect(60, 10), Arena.Center + new WDir(15, -30), Activation: _activation2, Color: color);
            yield return new(new AOEShapeRect(60, 10), Arena.Center + new WDir(-15, -30), Activation: _activation2, Color: color);
        }
    }
}

class MarxSmashInside(BossModule module) : Components.GenericAOEs(module, AID.MarxSmashInsideFirst)
{
    private DateTime _activation1;
    private DateTime _activation2;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _activation1 = Module.CastFinishAt(spell, 1.6f);
            _activation2 = _activation1.AddSeconds(4.7f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.MarxSmashInsideNear:
                _activation1 = default;
                break;
            case AID.MarxSmashInsideMiddle:
                _activation2 = default;
                break;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation1 != default)
            yield return new(new AOEShapeRect(30, 30), Arena.Center, -180.Degrees(), Activation: _activation1, Color: ArenaColor.Danger);
        if (_activation2 != default)
        {
            var color = _activation1 == default ? ArenaColor.Danger : ArenaColor.AOE;
            yield return new(new AOEShapeRect(30, 15, 30), Arena.Center, Activation: _activation2, Color: color);
        }
    }
}
