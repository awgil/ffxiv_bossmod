namespace BossMod.Dawntrail.Ultimate.UMAD;

class P1DoubleTroubleTrap : Components.UniformStackSpread
{
    readonly UMADConfig _config = Service.Config.Get<UMADConfig>();
    public int NumCasts { get; private set; }
    public int Order;

    private DateTime _lastActivation;
    private BitMask _wasStack;

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
            _lastActivation = WorldState.CurrentTime;
            _wasStack.Set(Raid.FindSlot(spell.MainTargetID));
            Stacks.Clear();
            NumCasts++;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Stacks.FirstOrNull() is not { Activation: var activation })
            return;

        var isStack = IsStackTarget(actor);

        if (Order == 1)
        {
            var myOrder = _config.P1WaveCannonConga[assignment];
            if (myOrder >= 0)
            {
                var side = myOrder < 4 ? -1 : 1;
                hints.AddForbiddenZone(ShapeContains.PrecisePosition(Arena.Center + new WDir((isStack ? 9 : 5.75f) * side, 0), new(0, 1), 0.5f, actor.Position, 0.1f), activation);
                return;
            }
        }

        if (Order == 2 && _config.P1GravityPuddleStrategy == UMADConfig.P1GravityPuddlePlacement.StackAll && EnableHints)
        {
            var ourSide = actor.Class.IsSupport() ? -1 : 1;
            if (Module.Enemies(OID.Gravitas).Where(a => MathF.Sign(a.Position.Z - 100) == ourSide).Closest(Arena.Center) is { } closestPuddle)
            {
                if (isStack)
                {
                    var puddleEdgeZ = closestPuddle.Position.Z - 6 * ourSide;
                    hints.AddForbiddenZone(ShapeContains.PrecisePosition(new(100, puddleEdgeZ), new(0, 1), 0.5f, actor.Position, 0.1f), activation);
                    return;
                }

                if (Stacks.FirstOrNull(s => s.Target.Class.IsSupport() == actor.Class.IsSupport()) is { } stackWith)
                {
                    hints.AddForbiddenZone(ShapeContains.PrecisePosition(new(stackWith.Target.Position.X, stackWith.Target.Position.Z - 1 * ourSide), new(0, 1), 0.5f, actor.Position, 0.1f), activation);
                    return;
                }
            }
        }

        if (Order == 3 && _config.P1ArrowsConfettiStrategy == UMADConfig.P1ArrowsStacks.SuppNDamageS && EnableHints)
        {
            WPos dest = (isStack, actor.Class.IsSupport()) switch
            {
                (true, true) => new(94, 94),
                (true, false) => new(106, 106),
                (false, true) => new(96, 96),
                (false, false) => new(104, 104)
            };

            hints.AddForbiddenZone(ShapeContains.PrecisePosition(dest, new(0, 1), 0.5f, actor.Position, 0.1f), activation);
            return;
        }

        base.AddAIHints(slot, actor, assignment, hints);
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

// have to wait until confetti knockbacks actually go off before moving, since kb type is 6 (meaning that the cast itself does nothing and the knockback is applied later via ActorControl)
class P1DoubleTroubleStay(BossModule module) : BossComponent(module)
{
    BitMask PendingStacks;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DoubleTroubleTrapStack)
            PendingStacks.Set(Raid.FindSlot(spell.MainTargetID));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (PendingStacks[slot])
            hints.ForcedMovement = new(0);
    }

    public override void Update()
    {
        if (Raid.WithoutSlot().All(p => p.PendingKnockbacks.Count == 0))
            PendingStacks.Reset();
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
