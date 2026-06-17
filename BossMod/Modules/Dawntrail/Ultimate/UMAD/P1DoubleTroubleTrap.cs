namespace BossMod.Dawntrail.Ultimate.UMAD;

class P1DoubleTroubleTrap : Components.UniformStackSpread
{
    public int NumCasts { get; private set; }

    public P1DoubleTroubleTrap(BossModule module) : base(module, 6, 0, 4)
    {
        EnableHints = false;
        PermitOverlap = true;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DoubleTroubleTrap)
            AddStack(actor, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DoubleTroubleTrapStack)
        {
            Stacks.Clear();
            NumCasts++;
        }
    }
}

class P1DoubleTroubleTrapKB(BossModule module) : Components.Knockback(module, AID.DoubleTroubleTrapStack, true)
{
    readonly List<(Actor Source, DateTime Activation)> _sources = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var src in _sources)
            if (actor.Position.InCircle(src.Source.Position, 6))
                yield return new(src.Source.Position, 14, src.Activation);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DoubleTroubleTrap && NumCasts == 0)
            _sources.Add((actor, status.ExpireAt));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (_sources.Count > 0)
                _sources.RemoveAt(0);
        }
    }
}

// passing confetti isn't required to survive P1, so we should continue to show expected state transitions regardless of how many stacks are alive
class P1DoubleTroubleTrapCounter(BossModule module) : Components.CastCounter(module, AID.DoubleTroubleTrapStack)
{
    public float Timeout;
    public bool Resolved;

    readonly int _numExpected = module.FindComponent<P1DoubleTroubleTrap>()!.Stacks.Count;
    readonly DateTime _start = module.WorldState.CurrentTime;

    public override void Update()
    {
        if (Resolved || Timeout == 0)
            return;

        if (_numExpected == 0)
        {
            if (WorldState.CurrentTime >= _start.AddSeconds(Timeout))
                Resolved = true;
        }
        else if (NumCasts > 0)
            Resolved = true;
    }
}
