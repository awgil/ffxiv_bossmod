namespace BossMod.Components;

// generic component dealing with 'forced march' mechanics
// these mechanics typically feature 'march left/right/forward/backward' debuffs, which rotate player and apply 'forced march' on expiration
// if there are several active march debuffs, we assume they are chained together
public class GenericForcedMarch(BossModule module, float activationLimit = float.MaxValue, bool stopAtWall = false) : BossComponent(module)
{
    public class PlayerState
    {
        public List<(Angle dir, float duration, DateTime activation)> PendingMoves = [];
        public DateTime ForcedEnd; // zero if forced march not active

        public bool Active(BossModule module) => ForcedEnd > module.WorldState.CurrentTime || PendingMoves.Count > 0;
    }

    public int NumActiveForcedMarches { get; private set; }
    public Dictionary<ulong, PlayerState> State = []; // key = instance ID
    public float MovementSpeed = 6; // default movement speed, can be overridden if necessary
    public float ActivationLimit = activationLimit; // do not show pending moves that activate later than this limit

    // called to determine whether we need to show hint
    public virtual bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var last = ForcedMovements(actor).LastOrDefault();
        if (last.from != last.to && DestinationUnsafe(slot, actor, last.to))
            hints.Add("Aim for safe spot!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var m in ForcedMovements(pc))
        {
            Arena.ActorProjected(m.from, m.to, m.dir, ArenaColor.Danger);
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddLine(m.from, m.to, 0xFF000000, 2);
            Arena.AddLine(m.from, m.to, ArenaColor.Danger);
        }
    }

    public void AddForcedMovement(Actor player, Angle direction, float duration, DateTime activation)
    {
        var moves = State.GetOrAdd(player.InstanceID).PendingMoves;
        moves.Add((direction, duration, activation));
        moves.SortBy(e => e.activation);
    }

    public bool HasForcedMovements(Actor player) => State.GetValueOrDefault(player.InstanceID)?.Active(Module) ?? false;

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

    public IEnumerable<(WPos from, WPos to, Angle dir)> ForcedMovements(Actor player)
    {
        var state = State.GetValueOrDefault(player.InstanceID);
        if (state == null)
            yield break;

        var from = player.Position;
        var dir = player.Rotation;
        if (state.ForcedEnd > WorldState.CurrentTime)
        {
            var dist = MovementSpeed * (float)(state.ForcedEnd - WorldState.CurrentTime).TotalSeconds;

            if (stopAtWall)
                dist = Math.Min(dist, Arena.IntersectRayBounds(from, dir.ToDirection()) - 0.1f);

            // note: as soon as player starts marching, he turns to desired direction
            // TODO: would be nice to use non-interpolated rotation here...
            var to = from + dist * dir.ToDirection();

            yield return (from, to, dir);
            from = to;
        }

        var limit = ActivationLimit < float.MaxValue ? WorldState.FutureTime(ActivationLimit) : DateTime.MaxValue;
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
public class StatusDrivenForcedMarch(BossModule module, float duration, uint statusForward, uint statusBackward, uint statusLeft, uint statusRight, uint statusForced = 1257, float activationLimit = float.MaxValue, bool stopAtWall = false) : GenericForcedMarch(module, activationLimit, stopAtWall)
{
    public float Duration = duration;
    public readonly uint[] Statuses = [statusForward, statusLeft, statusBackward, statusRight, statusForced]; // 5 elements: fwd, left, back, right, forced

    public override void OnStatusGain(Actor actor, ActorStatus status)
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

    public override void OnStatusLose(Actor actor, ActorStatus status)
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
