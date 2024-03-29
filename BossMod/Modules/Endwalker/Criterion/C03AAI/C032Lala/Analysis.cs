namespace BossMod.Endwalker.Criterion.C03AAI.C032Lala;

class Analysis : BossComponent
{
    public Angle[] SafeDir = new Angle[4];

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        Angle? offset = (SID)status.ID switch
        {
            SID.FrontUnseen => 0.Degrees(),
            SID.BackUnseen => 180.Degrees(),
            SID.LeftUnseen => 90.Degrees(),
            SID.RightUnseen => -90.Degrees(),
            _ => null
        };
        if (offset != null && module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < SafeDir.Length)
            SafeDir[slot] = offset.Value;
    }
}

class AnalysisRadiance : Components.GenericGaze
{
    private Analysis? _analysis;
    private ArcaneArray? _pulse;
    private List<Actor> _globes = new();

    public AnalysisRadiance() : base(default, true) { }

    public override IEnumerable<Eye> ActiveEyes(BossModule module, int slot, Actor actor)
    {
        var (nextGlobe, activation) = NextGlobe();
        if (_analysis != null && nextGlobe != null && activation != default)
            yield return new(nextGlobe.Position, activation, _analysis.SafeDir[slot]);
    }

    public override void Init(BossModule module)
    {
        _analysis = module.FindComponent<Analysis>();
        _pulse = module.FindComponent<ArcaneArray>();
    }

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID is OID.NArcaneGlobe or OID.SArcaneGlobe)
            _globes.Add(actor);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
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

class TargetedLight : Components.GenericGaze
{
    public bool Active;
    private Analysis? _analysis;
    private Angle[] _rotation = new Angle[4];
    private Angle[] _safeDir = new Angle[4];
    private int[] _rotationCount = new int[4];
    private DateTime _activation;

    public TargetedLight() : base(default, true) { }

    public override IEnumerable<Eye> ActiveEyes(BossModule module, int slot, Actor actor)
    {
        if (Active)
            yield return new(module.Bounds.Center, _activation, _safeDir[slot]);
    }

    public override void Init(BossModule module)
    {
        _analysis = module.FindComponent<Analysis>();
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_rotation[slot] != default)
            hints.Add($"Rotation: {(_rotation[slot].Rad < 0 ? "CW" : "CCW")}", false);
        base.AddHints(module, slot, actor, hints, movementHints);
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        var count = (SID)status.ID switch
        {
            SID.TimesThreePlayer => -1,
            SID.TimesFivePlayer => 1,
            _ => 0
        };
        if (count != 0 && module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < _rotationCount.Length)
            _rotationCount[slot] = count;
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        var rot = (IconID)iconID switch
        {
            IconID.PlayerRotateCW => -90.Degrees(),
            IconID.PlayerRotateCCW => 90.Degrees(),
            _ => default
        };
        if (rot != default && module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < _rotation.Length)
        {
            _rotation[slot] = rot * _rotationCount[slot];
            if (_analysis != null)
                _safeDir[slot] = _analysis.SafeDir[slot] + _rotation[slot];
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NTargetedLightAOE or AID.STargetedLightAOE)
            _activation = spell.NPCFinishAt;
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NTargetedLightAOE or AID.STargetedLightAOE)
            ++NumCasts;
    }
}
