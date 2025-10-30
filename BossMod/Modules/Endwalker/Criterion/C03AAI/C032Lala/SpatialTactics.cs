namespace BossMod.Endwalker.Criterion.C03AAI.C032Lala;

class SpatialTactics(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ArcaneArray? _array = module.FindComponent<ArcaneArray>();
    private readonly List<Actor> _fonts = [];
    private readonly int[] _remainingStacks = new int[4];

    private static readonly AOEShapeCross _shape = new(50, 4);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_array == null)
            yield break;
        bool wantMore = _remainingStacks[slot] > (NumCasts == 0 ? 1 : 0);
        foreach (var f in NextFonts().Take(2))
            foreach (var p in _array.SafeZoneCenters.Where(p => _shape.Check(p, f.actor)))
                yield return new(ArcaneArrayPlot.Shape, p, default, f.activation, wantMore ? ArenaColor.SafeFromAOE : ArenaColor.AOE, !wantMore);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_remainingStacks[slot] > 0)
            hints.Add($"Remaining stacks: {_remainingStacks[slot]}", false);
        base.AddHints(slot, actor, hints);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.NArcaneFont or OID.SArcaneFont)
            _fonts.Add(actor);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SubtractiveSuppressorBeta && Raid.TryFindSlot(actor.InstanceID, out var slot) && slot < _remainingStacks.Length)
            _remainingStacks[slot] = status.Extra;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SubtractiveSuppressorBeta && Raid.TryFindSlot(actor.InstanceID, out var slot) && slot < _remainingStacks.Length)
            _remainingStacks[slot] = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
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
