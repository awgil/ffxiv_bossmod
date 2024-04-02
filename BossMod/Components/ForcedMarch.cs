namespace BossMod.Components;

// generic component dealing with 'forced march' mechanics
// these mechanics typically feature 'march left/right/forward/backward' debuffs, which rotate player and apply 'forced march' on expiration
// if there are several active march debuffs, we assume they are chained together
public class GenericForcedMarch : BossComponent
{
    public class PlayerState
    {
        public List<(Angle dir, float duration, DateTime activation)> PendingMoves = new();
        public DateTime ForcedEnd; // zero if forced march not active

        public bool Active(BossModule module) => ForcedEnd > module.WorldState.CurrentTime || PendingMoves.Count > 0;
    }

    public int NumActiveForcedMarches { get; private set; }
    public Dictionary<ulong, PlayerState> State = new(); // key = instance ID
    public float MovementSpeed = 6; // default movement speed, can be overridden if necessary
    public float ActivationLimit = float.MaxValue; // do not show pending moves that activate later than this limit

    // called to determine whether we need to show hint
    public virtual bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => !module.Bounds.Contains(pos);

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        var last = ForcedMovements(module, actor).LastOrDefault();
        if (last.from != last.to && DestinationUnsafe(module, slot, actor, last.to))
            hints.Add("Aim for safe spot!");
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var m in ForcedMovements(module, pc))
        {
            if (arena.Config.ShowOutlinesAndShadows)
                arena.AddLine(m.from, m.to, 0xFF000000, 2);
            arena.AddLine(m.from, m.to, ArenaColor.Danger);
            arena.Actor(m.to, m.dir, ArenaColor.Danger);
        }
    }

    public void AddForcedMovement(Actor player, Angle direction, float duration, DateTime activation)
    {
        var moves = State.GetOrAdd(player.InstanceID).PendingMoves;
        moves.Add((direction, duration, activation));
        moves.SortBy(e => e.activation);
    }

    public bool HasForcedMovements(BossModule module, Actor player) => State.GetValueOrDefault(player.InstanceID)?.Active(module) ?? false;

    public void ActivateForcedMovement(Actor player, DateTime expiration)
    {
        State.GetOrAdd(player.InstanceID).ForcedEnd = expiration;
        ++NumActiveForcedMarches;
    }

    public void DeactivateForcedMovement(Actor player)
    {
        State.GetOrAdd(player.InstanceID).ForcedEnd = default;
        --NumActiveForcedMarches;
    }

    public IEnumerable<(WPos from, WPos to, Angle dir)> ForcedMovements(BossModule module, Actor player)
    {
        var state = State.GetValueOrDefault(player.InstanceID);
        if (state == null)
            yield break;

        var from = player.Position;
        var dir = player.Rotation;
        if (state.ForcedEnd > module.WorldState.CurrentTime)
        {
            // note: as soon as player starts marching, he turns to desired direction
            // TODO: would be nice to use non-interpolated rotation here...
            var to = from + MovementSpeed * (float)(state.ForcedEnd - module.WorldState.CurrentTime).TotalSeconds * dir.ToDirection();
            yield return (from, to, dir);
            from = to;
        }

        var limit = ActivationLimit < float.MaxValue ? module.WorldState.CurrentTime.AddSeconds(ActivationLimit) : DateTime.MaxValue;
        foreach (var move in state.PendingMoves.TakeWhile(move => move.activation <= limit))
        {
            dir += move.dir;
            var to = from + MovementSpeed * move.duration * dir.ToDirection();
            yield return (from, to, dir);
            from = to;
        }
    }
}

// typical forced march is driven by statuses
public class StatusDrivenForcedMarch : GenericForcedMarch
{
    public float Duration;
    public uint[] Statuses; // 5 elements: fwd, left, back, right, forced

    public StatusDrivenForcedMarch(float duration, uint statusForward, uint statusBackward, uint statusLeft, uint statusRight, uint statusForced = 1257)
    {
        Duration = duration;
        Statuses = new[] { statusForward, statusLeft, statusBackward, statusRight, statusForced };
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        var statusKind = Array.IndexOf(Statuses, status.ID);
        if (statusKind == 4)
        {
            ActivateForcedMovement(actor, status.ExpireAt);
        }
        else if (statusKind >= 0)
        {
            AddForcedMovement(actor, statusKind * 90.Degrees(), Duration, status.ExpireAt);
        }
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        var statusKind = Array.IndexOf(Statuses, status.ID);
        if (statusKind == 4)
        {
            DeactivateForcedMovement(actor);
        }
        else if (statusKind >= 0)
        {
            var dir = statusKind * 90.Degrees();
            State.GetOrAdd(actor.InstanceID).PendingMoves.RemoveAll(e => e.dir == dir);
        }
    }
}
