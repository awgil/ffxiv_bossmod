namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

class HailOfThunder(BossModule module) : Components.GenericAOEs(module)
{
    private int _advance;
    private int _lastPos;
    private DateTime _nextActivation;

    static readonly WDir[] Sources = [new(0, -10), new(-4.8f, 0), new(0, 10), new(4.8f, 0)];

    private AOEInstance? _next;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_next);

    public override void Update()
    {
        if (_advance == 0)
            return;

        var indicator = Module.Enemies(OID.ArcaneRevelation).FirstOrDefault();
        if (indicator == null)
        {
            ReportError("Indicator not found, can't predict AOE");
            _advance = 0;
            return;
        }

        if (indicator.LastFrameMovement == default)
            return;

        var sign = MathF.Sign(indicator.LastFrameMovement.OrthoL().Dot(indicator.DirectionTo(Arena.Center)));
        _lastPos += _advance * sign;

        if (_lastPos < 0)
            _lastPos += 4;
        _lastPos %= 4;

        _next = new AOEInstance(new AOEShapeCircle(16), Arena.Center + Sources[_lastPos], default, _nextActivation);
        _advance = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.HailOfThunderShort:
                _advance = 2;
                _nextActivation = WorldState.FutureTime(7.5f);
                break;
            case AID.HailOfThunderMedium:
                _advance = 3;
                _nextActivation = WorldState.FutureTime(10.5f);
                break;
            case AID.HailOfThunderLong:
                _advance = 4;
                _nextActivation = WorldState.FutureTime(13.4f);
                break;
            case AID.HailOfThunder:
                _advance = 0;
                _next = null;
                NumCasts++;
                break;
        }
    }
}

class DesignatedConductor(BossModule module) : Components.GenericStackSpread(module)
{
    private BitMask _conductors;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DesignatedConductor)
            _conductors.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DesignatedConductor)
            _conductors.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            // delay: 13.4s
            case AID.HailOfThunderLong:
                Predict(13.4f);
                break;
            // delay: 7.5s
            case AID.HailOfThunderShort:
                Predict(7.5f);
                break;
            case AID.HailOfThunderMedium:
                // FIXME
                Predict(0);
                break;
            case AID.HyperconductivePlasma:
                if (Stacks.Count > 0)
                    Stacks.RemoveAt(0);
                break;
        }
    }

    void Predict(float advance)
    {
        foreach (var (_, player) in Raid.WithSlot().IncludedInMask(_conductors))
            Stacks.Add(new(player, 13, minSize: 3, activation: WorldState.FutureTime(advance)));
    }
}
