namespace BossMod.Autorotation;

// raidwides / tankbusters from the module's timeline, for modules that don't predict damage
public static class StateTimeline
{
    // timeline estimates are usually off by a second or two; a state overdue by more is waiting on something else (hp push, adds), so its end time is unknown
    private const float OverdueGrace = 3;

    public static IEnumerable<(DateTime at, StateMachine.StateHint hint)> Upcoming(BossModule? module, WorldState ws, float horizon = 30)
    {
        var sm = module?.StateMachine;
        var s = sm?.ActiveState;
        if (sm == null || s == null)
            yield break;
        var overdue = sm.TimeSinceTransition - s.Duration;
        if (overdue > OverdueGrace)
            yield break; // nothing after it can be timed either
        var t = MathF.Max(0, -overdue);
        for (var i = 0; i < 256 && t <= horizon; i++)
        {
            if ((s.EndHint & (StateMachine.StateHint.Raidwide | StateMachine.StateHint.Tankbuster)) != 0)
                yield return (ws.FutureTime(t), s.EndHint);
            if (s.NextStates?.Length != 1)
                yield break;
            s = s.NextStates[0];
            t += s.Duration;
        }
    }

    // includeShared: also count shared/stack damage, which the timeline itself doesn't distinguish from raidwides
    public static IEnumerable<DateTime> Raidwides(BossModule? module, WorldState ws, AIHints hints, float horizon = 30, bool includeShared = true)
    {
        var predicted = hints.PredictedDamage.Where(d => d.Type == AIHints.PredictedDamageType.Raidwide || includeShared && d.Type == AIHints.PredictedDamageType.Shared).Select(d => d.Activation).ToList();
        foreach (var p in predicted)
            yield return p;
        foreach (var (at, hint) in Upcoming(module, ws, horizon))
            if (hint.HasFlag(StateMachine.StateHint.Raidwide) && !predicted.Any(p => Math.Abs((p - at).TotalSeconds) < 3))
                yield return at;
    }

    public static IEnumerable<(Actor target, DateTime at)> Tankbusters(BossModule? module, WorldState ws, AIHints hints, float horizon = 30)
    {
        var predicted = hints.PredictedDamage.Where(d => d.Type == AIHints.PredictedDamageType.Tankbuster)
            .SelectMany(d => ws.Party.WithSlot().IncludedInMask(d.Players).Select(p => (target: p.Item2, at: d.Activation))).ToList();
        foreach (var p in predicted)
            yield return p;
        if (module?.PrimaryActor is not { } boss || ws.Actors.Find(boss.TargetID) is not { } tank || ws.Party.FindSlot(tank.InstanceID) < 0)
            yield break;
        foreach (var (at, hint) in Upcoming(module, ws, horizon))
            if (hint.HasFlag(StateMachine.StateHint.Tankbuster) && !predicted.Any(p => Math.Abs((p.at - at).TotalSeconds) < 3))
                yield return (tank, at);
    }
}
