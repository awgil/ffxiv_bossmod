namespace BossMod.Endwalker.Savage.P4S2Hesperos;

// state related to nearsight & farsight mechanics
class NearFarSight : BossComponent
{
    public enum State { Near, Far, Done }

    public State CurState { get; private set; }
    private BitMask _targets;
    private BitMask _inAOE;

    private static readonly float _aoeRadius = 5;

    public override void Init(BossModule module)
    {
        CurState = (AID)(module.PrimaryActor.CastInfo?.Action.ID ?? 0) switch
        {
            AID.Nearsight => State.Near,
            AID.Farsight => State.Far,
            _ => State.Done
        };
        if (CurState == State.Done)
            module.ReportError(this, $"Failed to initialize near/far sight, unexpected cast {module.PrimaryActor.CastInfo?.Action}");
    }

    public override void Update(BossModule module)
    {
        _targets = _inAOE = new();
        if (CurState == State.Done)
            return;

        var playersByRange = module.Raid.WithSlot().SortedByRange(module.PrimaryActor.Position);
        foreach ((int i, var player) in CurState == State.Near ? playersByRange.Take(2) : playersByRange.TakeLast(2))
        {
            _targets.Set(i);
            _inAOE |= module.Raid.WithSlot().InRadiusExcluding(player, _aoeRadius).Mask();
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_targets.None())
            return;

        bool isTarget = _targets[slot];
        bool shouldBeTarget = actor.Role == Role.Tank;
        bool isFailing = isTarget != shouldBeTarget;
        bool shouldBeNear = CurState == State.Near ? shouldBeTarget : !shouldBeTarget;
        hints.Add(shouldBeNear ? "Stay near boss" : "Stay on max melee", isFailing);
        if (_inAOE[slot])
        {
            hints.Add("GTFO from tanks!");
        }
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_targets.None())
            return;

        foreach ((int i, var player) in module.Raid.WithSlot())
        {
            if (_targets[i])
            {
                arena.Actor(player, ArenaColor.Danger);
                arena.AddCircle(player.Position, _aoeRadius, ArenaColor.Danger);
            }
            else
            {
                arena.Actor(player, _inAOE[i] ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
            }
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NearsightAOE or AID.FarsightAOE)
            CurState = State.Done;
    }
}
