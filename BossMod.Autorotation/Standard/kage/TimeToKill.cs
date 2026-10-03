namespace BossMod.Autorotation.kage;

// shared time-to-kill estimate for the Kage rotations: HP lost per second over the last ~15s of samples
// answers "will this enemy live long enough for this action to pay off?" instead of fixed HP% thresholds
public static class TimeToKill
{
    private const float SampleInterval = 0.5f;
    private const float Window = 15;
    private const float MinObserved = 3;

    private static readonly Dictionary<ulong, Queue<(DateTime time, uint hp)>> _samples = [];
    private static DateTime _lastUpdate;

    public static void Update(WorldState ws, AIHints hints)
    {
        if ((ws.CurrentTime - _lastUpdate).TotalSeconds < SampleInterval && ws.CurrentTime >= _lastUpdate)
            return;
        _lastUpdate = ws.CurrentTime;

        foreach (var e in hints.PotentialTargets)
        {
            if (!_samples.TryGetValue(e.Actor.InstanceID, out var q))
                _samples[e.Actor.InstanceID] = q = new();
            q.Enqueue((ws.CurrentTime, e.Actor.HPMP.CurHP));
            while (q.Count > 0 && (ws.CurrentTime - q.Peek().time).TotalSeconds > Window)
                q.Dequeue();
        }

        // forget enemies that are gone
        foreach (var id in _samples.Keys.Where(id => ws.Actors.Find(id) is not { IsDead: false }).ToList())
            _samples.Remove(id);
    }

    // seconds until the enemy dies at the current damage rate; null while there isn't enough data yet
    public static float? Estimate(Actor enemy)
    {
        if (!_samples.TryGetValue(enemy.InstanceID, out var q) || q.Count < 2)
            return null;
        var (t0, hp0) = q.Peek();
        var dt = (float)(_lastUpdate - t0).TotalSeconds;
        if (dt < MinObserved)
            return null;
        var lost = (float)hp0 - enemy.HPMP.CurHP;
        return lost <= 0 ? float.MaxValue : enemy.HPMP.CurHP / (lost / dt);
    }

    // will the enemy still be alive in `seconds`? dummies always; without data yet, anything not nearly dead
    public static bool WillLive(Actor enemy, float seconds)
        => enemy.IsStrikingDummy || (Estimate(enemy) is { } ttk ? ttk >= seconds : enemy.HPRatio > 0.05f);

    // big cooldowns, with the boss targetable: kept for the boss, a boss-tier target (at least as much HP left), or an add the module
    // wants killed first (higher priority than the boss); boss away / trash: whatever lives long enough for `payoff`
    public static bool BurstWorthIt(BossModule? module, AIHints hints, Actor target, float payoff)
    {
        if (target.IsStrikingDummy)
            return true;
        if (module?.PrimaryActor is { IsDeadOrDestroyed: false, IsTargetable: true } boss && target != boss && target.HPMP.CurHP < boss.HPMP.CurHP
            && (hints.FindEnemy(target)?.Priority ?? 0) <= (hints.FindEnemy(boss)?.Priority ?? 0))
            return false;
        return WillLive(target, payoff);
    }
}
