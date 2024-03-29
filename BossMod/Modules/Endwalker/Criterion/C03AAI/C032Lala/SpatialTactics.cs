namespace BossMod.Endwalker.Criterion.C03AAI.C032Lala;

class SpatialTactics : Components.GenericAOEs
{
    private ArcaneArray? _array;
    private List<Actor> _fonts = new();
    private int[] _remainingStacks = new int[4];

    private static readonly AOEShapeCross _shape = new(50, 4);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_array == null)
            yield break;
        bool wantMore = _remainingStacks[slot] > (NumCasts == 0 ? 1 : 0);
        foreach (var f in NextFonts().Take(2))
            foreach (var p in _array.SafeZoneCenters.Where(p => _shape.Check(p, f.actor)))
                yield return new(ArcaneArrayPlot.Shape, p, default, f.activation, wantMore ? ArenaColor.SafeFromAOE : ArenaColor.AOE, !wantMore);
    }

    public override void Init(BossModule module)
    {
        _array = module.FindComponent<ArcaneArray>();
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_remainingStacks[slot] > 0)
            hints.Add($"Remaining stacks: {_remainingStacks[slot]}", false);
        base.AddHints(module, slot, actor, hints, movementHints);
    }

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID is OID.NArcaneFont or OID.SArcaneFont)
            _fonts.Add(actor);
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SubtractiveSuppressorBeta && module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < _remainingStacks.Length)
            _remainingStacks[slot] = status.Extra;
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SubtractiveSuppressorBeta && module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < _remainingStacks.Length)
            _remainingStacks[slot] = 0;
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NRadiance2:
            case AID.SRadiance2:
                ++NumCasts;
                break;
            case AID.NInfernoDivide:
            case AID.SInfernoDivide:
                _fonts.Remove(caster);
                ++NumCasts;
                break;
        }
    }

    private DateTime FontActivation(Actor font) => _array?.AOEs.FirstOrDefault(aoe => aoe.Check(font.Position)).Activation.AddSeconds(0.2f) ?? default;
    private IEnumerable<(Actor actor, DateTime activation)> NextFonts() => _fonts.Select(f => (f, FontActivation(f))).OrderBy(fa => fa.Item2);
}
