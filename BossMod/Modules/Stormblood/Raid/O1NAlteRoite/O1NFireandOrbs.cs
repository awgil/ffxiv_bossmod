namespace BossMod.Stormblood.Raid.O1NAlteRoite;

// Fire Orbs Burn
// Shows orb AOEs only shortly before expected explosion, and follows orb movement (Breath Wing / Downburst).
sealed class FireOrbsTimedFollowAOE(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(8f);

    // tune these if needed based on observed timings
    private const float WarningSeconds = 5f;

    // inner ring: from log ~3:19 spawn, ~3:35 Burn starts, explode ~1s later => ~17s from spawn
    private const float InnerExplodeFromSpawn = 16.84f;

    // outer ring: explode shortly after Breath Wing / Downburst resolves; approximate as "boss cast start + 4s"
    // outer ring explosion timings measured from replay (cast start -> orb explosion)
    private const float OuterExplodeFromBreathStart = 9.08f;
    private const float OuterExplodeFromDownburstStart = 12.18f;
    private record struct OrbInfo(DateTime Spawn, bool IsOuter, DateTime PredictedExplode, DateTime? ActualExplode);

    private readonly Dictionary<ulong, OrbInfo> _orbs = [];
    private readonly List<AOEInstance> _tmp = [];

    // used to classify inner vs outer by spawn timing
    private DateTime _firstRingSpawn = default;

    // set when boss starts Breath Wing / Downburst to predict outer explosions
    private DateTime _breathStart = default;
    private DateTime _downburstStart = default;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _tmp.Clear();
        var now = WorldState.CurrentTime;

        var liveOrbs = Module.Enemies((uint)OID.BallOfFire);
        for (int i = 0; i < liveOrbs.Count; ++i)
        {
            var o = liveOrbs[i];
            if (!_orbs.TryGetValue(o.InstanceID, out var info))
                continue; // should not happen, but keep safe

            var explodeAt = info.ActualExplode ?? info.PredictedExplode;
            var showFrom = explodeAt.AddSeconds(-WarningSeconds);

            if (now >= showFrom)
                _tmp.Add(new(_shape, o.Position.Quantized(), default, explodeAt));
        }

        return CollectionsMarshal.AsSpan(_tmp);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID != (uint)OID.BallOfFire)
            return;

        var now = WorldState.CurrentTime;

        // new orb phase: reset timing anchors so predictions don't use stale BreathWing/Downburst times
        if (_orbs.Count == 0)
        {
            _firstRingSpawn = now;
            _breathStart = default;
            _downburstStart = default;
        }

        // classify rings: first ring establishes baseline; anything spawning a few seconds later counts as "outer"
        if (_firstRingSpawn == default)
            _firstRingSpawn = now;

        bool isOuter = (now - _firstRingSpawn).TotalSeconds >= 2.0; // From log: ~3s gap between rings
        DateTime predicted = isOuter
            ? PredictOuterExplosion(now)
            : now.AddSeconds(InnerExplodeFromSpawn);

        _orbs[actor.InstanceID] = new(now, isOuter, predicted, null);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.BallOfFire)
            _orbs.Remove(actor.InstanceID);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // boss casts that reposition outer orbs
        if (caster == Module.PrimaryActor)
        {
            if (spell.Action.ID == (uint)AID.BreathWing) // adjust name if enum differs
                _breathStart = WorldState.CurrentTime;
            else if (spell.Action.ID == (uint)AID.Downburst) // adjust name if enum differs
                _downburstStart = WorldState.CurrentTime;

            // once seen Breath/Downburst, update predictions for any outer orbs that haven't started casting yet
            if (spell.Action.ID is (uint)AID.BreathWing or (uint)AID.Downburst)
                RefreshOuterPredictions();

            return;
        }

        // orb explosion casts: lock in actual explode time once seen them
        if (caster.OID == (uint)OID.BallOfFire &&
            (spell.Action.ID == (uint)AID.Burn || spell.Action.ID == (uint)AID.Burn1) &&
            _orbs.TryGetValue(caster.InstanceID, out var info))
        {
            info.ActualExplode = WorldState.FutureTime(spell.NPCRemainingTime);
            _orbs[caster.InstanceID] = info;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (caster.OID == (uint)OID.BallOfFire &&
            (spell.Action.ID == (uint)AID.Burn || spell.Action.ID == (uint)AID.Burn1))
        {
            _orbs.Remove(caster.InstanceID);
        }
    }

    private DateTime PredictOuterExplosion(DateTime now)
    {
        // prefer the most recent known driver
        if (_downburstStart != default)
            return _downburstStart.AddSeconds(OuterExplodeFromDownburstStart);
        if (_breathStart != default)
            return _breathStart.AddSeconds(OuterExplodeFromBreathStart);

        // fallback: if I haven't seen Breath/Downburst yet, guess similarly to inner
        return now.AddSeconds(InnerExplodeFromSpawn);
    }

    private void RefreshOuterPredictions()
    {
        foreach (var (id, info) in _orbs.ToArray())
        {
            if (!info.IsOuter || info.ActualExplode.HasValue)
                continue;

            var updated = info;
            updated.PredictedExplode = PredictOuterExplosion(info.Spawn);
            _orbs[id] = updated;
        }
    }
}
