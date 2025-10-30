namespace BossMod.Endwalker.Criterion.C03AAI.C032Lala;

class Analysis(BossModule module) : BossComponent(module)
{
    public Angle[] SafeDir = new Angle[4];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        Angle? offset = (SID)status.ID switch
        {
            SID.FrontUnseen => 0.Degrees(),
            SID.BackUnseen => 180.Degrees(),
            SID.LeftUnseen => 90.Degrees(),
            SID.RightUnseen => -90.Degrees(),
            _ => null
        };
        if (offset != null && Raid.TryFindSlot(actor.InstanceID, out var slot) && slot < SafeDir.Length)
            SafeDir[slot] = offset.Value;
    }
}

class AnalysisRadiance(BossModule module) : Components.GenericGaze(module, default, true)
{
    private readonly Analysis? _analysis = module.FindComponent<Analysis>();
    private readonly ArcaneArray? _pulse = module.FindComponent<ArcaneArray>();
    private readonly List<Actor> _globes = [];

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        var (nextGlobe, activation) = NextGlobe();
        if (_analysis != null && nextGlobe != null && activation != default)
            yield return new(nextGlobe.Position, activation, _analysis.SafeDir[slot]);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.NArcaneGlobe or OID.SArcaneGlobe)
            _globes.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NRadiance1 or AID.SRadiance1)
        {
            ++NumCasts;
            _globes.Remove(caster);
        }
    }

    private DateTime GlobeActivation(Actor globe) => _pulse?.AOEs.FirstOrDefault(aoe => aoe.Check(globe.Position)).Activation.AddSeconds(0.2f) ?? default;
    private (Actor? actor, DateTime activation) NextGlobe() => _globes.Select(g => (g, GlobeActivation(g))).MinBy(ga => ga.Item2);
}

class TargetedLight(BossModule module) : Components.GenericGaze(module, default, true)
{
    public bool Active;
    private readonly Analysis? _analysis = module.FindComponent<Analysis>();
    private readonly Angle[] _rotation = new Angle[4];
    private readonly Angle[] _safeDir = new Angle[4];
    private readonly int[] _rotationCount = new int[4];
    private DateTime _activation;

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        if (Active)
            yield return new(Module.Center, _activation, _safeDir[slot]);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_rotation[slot] != default)
            hints.Add($"Rotation: {(_rotation[slot].Rad < 0 ? "CW" : "CCW")}", false);
        base.AddHints(slot, actor, hints);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var count = (SID)status.ID switch
        {
            SID.TimesThreePlayer => -1,
            SID.TimesFivePlayer => 1,
            _ => 0
        };
        if (count != 0 && Raid.TryFindSlot(actor.InstanceID, out var slot) && slot < _rotationCount.Length)
            _rotationCount[slot] = count;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var rot = (IconID)iconID switch
        {
            IconID.PlayerRotateCW => -90.Degrees(),
            IconID.PlayerRotateCCW => 90.Degrees(),
            _ => default
        };
        if (rot != default && Raid.TryFindSlot(actor.InstanceID, out var slot) && slot < _rotation.Length)
        {
            _rotation[slot] = rot * _rotationCount[slot];
            if (_analysis != null)
                _safeDir[slot] = _analysis.SafeDir[slot] + _rotation[slot];
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NTargetedLightAOE or AID.STargetedLightAOE)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NTargetedLightAOE or AID.STargetedLightAOE)
            ++NumCasts;
    }
}
