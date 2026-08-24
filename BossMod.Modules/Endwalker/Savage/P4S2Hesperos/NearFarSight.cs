namespace BossMod.Endwalker.Savage.P4S2Hesperos;

// state related to nearsight & farsight mechanics
class NearFarSight : BossComponent
{
    public enum State { Near, Far, Done }

    public State CurState { get; private set; }
    private BitMask _targets;
    private BitMask _inAOE;

    private const float _aoeRadius = 5;

    public NearFarSight(BossModule module) : base(module)
    {
        CurState = (AID)(Module.PrimaryActor.CastInfo?.Action.ID ?? 0) switch
        {
            AID.Nearsight => State.Near,
            AID.Farsight => State.Far,
            _ => State.Done
        };
        if (CurState == State.Done)
            ReportError($"Failed to initialize near/far sight, unexpected cast {Module.PrimaryActor.CastInfo?.Action}");
    }

    public override void Update()
    {
        _targets = _inAOE = new();
        if (CurState == State.Done)
            return;

        var playersByRange = Raid.WithSlot().SortedByRange(Module.PrimaryActor.Position);
        foreach ((int i, var player) in CurState == State.Near ? playersByRange.Take(2) : playersByRange.TakeLast(2))
        {
            _targets.Set(i);
            _inAOE |= Raid.WithSlot().InRadiusExcluding(player, _aoeRadius).Mask();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
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

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_targets.None())
            return;

        foreach ((int i, var player) in Raid.WithSlot())
        {
            if (_targets[i])
            {
                Arena.Actor(player, ArenaColor.Danger);
                Arena.AddCircle(player.Position, _aoeRadius, ArenaColor.Danger);
            }
            else
            {
                Arena.Actor(player, _inAOE[i] ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NearsightAOE or AID.FarsightAOE)
            CurState = State.Done;
    }
}
