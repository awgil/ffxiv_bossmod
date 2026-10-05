namespace BossMod.Autorotation.kage;

// raidwides / tankbusters from the module's timeline, for modules that don't predict damage (FRU)
public static class StateTimeline
{
    public static IEnumerable<(DateTime at, StateMachine.StateHint hint)> Upcoming(BossModule? module, WorldState ws, float horizon = 30)
    {
        var sm = module?.StateMachine;
        var s = sm?.ActiveState;
        if (sm == null || s == null)
            yield break;
        var t = MathF.Max(0, s.Duration - sm.TimeSinceTransition);
        while (s != null && t <= horizon)
        {
            if ((s.EndHint & (StateMachine.StateHint.Raidwide | StateMachine.StateHint.Tankbuster)) != 0)
                yield return (ws.FutureTime(t), s.EndHint);
            if (s.NextStates?.Length != 1)
                yield break;
            s = s.NextStates[0];
            t += s.Duration;
        }
    }

    public static IEnumerable<DateTime> Raidwides(BossModule? module, WorldState ws, AIHints hints, float horizon = 30)
    {
        var predicted = hints.PredictedDamage.Where(d => d.Type is AIHints.PredictedDamageType.Raidwide or AIHints.PredictedDamageType.Shared).Select(d => d.Activation).ToList();
        foreach (var p in predicted)
            yield return p;
        foreach (var (at, hint) in Upcoming(module, ws, horizon))
            if (hint.HasFlag(StateMachine.StateHint.Raidwide) && !predicted.Any(p => Math.Abs((p - at).TotalSeconds) < 3))
                yield return at;
    }

    public static IEnumerable<(Actor target, DateTime at)> Tankbusters(BossModule? module, WorldState ws, AIHints hints, float horizon = 30)
    {
        var predicted = hints.PredictedDamage.Where(d => d.Type == AIHints.PredictedDamageType.Tankbuster)
            .SelectMany(d => ws.Party.WithSlot().IncludedInMask(d.Players).Select(p => (p.Item2, d.Activation))).ToList();
        foreach (var p in predicted)
            yield return p;
        if (module?.PrimaryActor is not { } boss || ws.Actors.Find(boss.TargetID) is not { } tank || ws.Party.FindSlot(tank.InstanceID) < 0)
            yield break;
        foreach (var (at, hint) in Upcoming(module, ws, horizon))
            if (hint.HasFlag(StateMachine.StateHint.Tankbuster) && !predicted.Any(p => Math.Abs((p.Item2 - at).TotalSeconds) < 3))
                yield return (tank, at);
    }
}
