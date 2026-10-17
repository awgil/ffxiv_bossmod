using Autofac;
using BossMod.Autorotation;
using BossMod.ReplayVisualization;

namespace BossMod;

// utility for applying replay operations to world state, accurate scrolling, etc.
public class ReplayPlayer(Replay r, WorldState ws, RotationDatabase rotationDB, RotationModuleManager rmm, BossModuleManager bmm, ZoneModuleManager zmm, AIHintsBuilder hintsBuilder, AIHints hints)
{
    public Replay Replay = r;
    private int _nextOp; // first unexecuted operation; note that it corresponds to first operation with timestamp > that worldstate's current

    public WorldState WorldState { get; private set; } = ws;
    public RotationDatabase RotationDB { get; } = rotationDB;
    public RotationModuleManager Rmm { get; } = rmm;
    public BossModuleManager Bmm { get; } = bmm;
    public ZoneModuleManager Zmm { get; } = zmm;
    public AIHintsBuilder HintsBuilder { get; } = hintsBuilder;
    public AIHints Hints { get; } = hints;

    // execute next group of operations that share timestamp; returns false if we've reached the end
    public bool TickForward()
    {
        if (_nextOp == Replay.Ops.Count)
            return false;

        var ts = Replay.Ops[_nextOp].Timestamp;
        while (_nextOp < Replay.Ops.Count && Replay.Ops[_nextOp].Timestamp == ts)
            WorldState.Execute(Replay.Ops[_nextOp++]);
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

    public static (ILifetimeScope Scope, ReplayPlayer Player) CreateOwned(Replay data, ILifetimeScope parent)
    {
        var scope = parent.BeginLifetimeScope(ctx =>
        {
            ctx.Register(_ => data).SingleInstance();
            ctx.Register(_ => new WorldState(data.QPF, data.GameVersion)).SingleInstance();
            ctx.RegisterType<ReplayPlayer>().SingleInstance();
            ctx.RegisterType<EventList>().SingleInstance();
        });
        return (scope, scope.Resolve<ReplayPlayer>());
    }
}
