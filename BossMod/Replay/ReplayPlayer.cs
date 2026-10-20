using Autofac;
using BossMod.Autorotation;

namespace BossMod;

// utility for applying replay operations to world state, accurate scrolling, etc.
public record ReplayPlayer(Replay Replay, WorldState WorldState, RotationDatabase RotationDB, RotationModuleManager Rotations, BossModuleManager Bossmods, ZoneModuleManager Zonemods, AIHintsBuilder HintsBuilder, AIHints Hints)
{
    private int _nextOp; // first unexecuted operation; note that it corresponds to first operation with timestamp > that worldstate's current

    // execute next group of operations that share timestamp; returns false if we've reached the end
    public bool TickForward()
    {
        if (_nextOp == Replay.Ops.Count)
            return false;

        var ts = Replay.Ops[_nextOp].Timestamp;
        while (_nextOp < Replay.Ops.Count && Replay.Ops[_nextOp].Timestamp == ts)
        {
            var nextOp = Replay.Ops[_nextOp++];
            WorldState.Execute(nextOp);
            ApplyReplayStateOps(nextOp);
        }
        return true;
    }

    // execute actions to advance current time to specific timestamp
    // after this call, next-op points to first operation with timestamp *greater* than what was passed, and world's current time is *less than or equal* to what was passed
    public void AdvanceTo(DateTime timestamp, Action update)
    {
        while (_nextOp < Replay.Ops.Count && Replay.Ops[_nextOp].Timestamp <= timestamp)
        {
            TickForward();
            update();
        }
    }

    public DateTime NextTimestamp() => _nextOp < Replay.Ops.Count ? Replay.Ops[_nextOp].Timestamp : default;
    public DateTime CurrTimestamp() => _nextOp > 0 ? Replay.Ops[_nextOp - 1].Timestamp : default;
    public DateTime PrevTimestamp()
    {
        var curr = CurrTimestamp();
        for (var i = _nextOp - 1; i >= 0; i--)
            if (Replay.Ops[i].Timestamp < curr)
                return Replay.Ops[i].Timestamp;
        return default;
    }

    private void ApplyReplayStateOps(WorldState.Operation op)
    {
        switch (op)
        {
            case ReplayState.OpActivePresetsChange p:
                Rotations.Clear();
                foreach (var active in p.Presets)
                    Rotations.Activate(active);
                break;
        }
    }

    public static (ILifetimeScope Scope, ReplayPlayer Player) Create(Replay data, ILifetimeScope parent)
    {
        var scope = parent.BeginLifetimeScope(ctx =>
        {
            ctx.Register(_ => data).SingleInstance();
            ctx.Register(_ => new WorldState(data.QPF, data.GameVersion)).SingleInstance();
            ctx.RegisterType<ReplayPlayer>().SingleInstance();
        });
        return (scope, scope.Resolve<ReplayPlayer>());
    }
}
