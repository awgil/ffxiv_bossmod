namespace BossMod.Stormblood.Ultimate.UCOB;

// this component makes a genuine effort to ensure that the twister will spawn exactly at the player's current position, and then to dodge out of that exact position as late as possible, to minimize the amount of movement required (especially useful in p4)
// it is impossible to precisely solve this in general, which is why predictRadius is a higher number
// e.g. even in an environment with apparently consistent framerate and ping, it is possible for your twister to spawn in the position you're dodging to, before you even get there (on the client) - see mchteleport.log
class Twister(BossModule module, AID cast, float delay) : Components.CastTwister(module, 1.25f, (uint)OID.VoidzoneTwister, cast, delay, predictBeforeSpawn: 0.6f, predictRadius: 2.25f)
{
    // how early before the predicted twister appearance should movement, casts, and gapclosers be prevented
    // movement before resolution will kill us because our movement will be interpolated serverside
    // additionally, dashes or slidecasting forces a position update, which may cause the twister to unavoidably spawn on top of the player
    // this is COMPLETELY arbitrary, there is probably no practical way to actually measure what this value "should" be
    const double PreSnapshotFreeze = 1.5;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _spawnAt = Module.CastFinishAt(spell, SpawnDelay);
            _predictAt = _spawnAt.AddSeconds(-(PredictionTime + WorldState.Network.LastRTT));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var t in ActiveTwisters)
        {
            hints.AddForbiddenZone(ShapeDistance.Circle(t.Position, Radius));
            hints.Voidzones.Add((t.Position, Radius));
        }
        foreach (var z in PredictedPositions)
        {
            hints.AddForbiddenZone(ShapeDistance.Circle(z, PredictionRadius));
            hints.Voidzones.Add((z, PredictionRadius));
        }

        var predictionPeriod = (_predictAt - WorldState.CurrentTime).TotalSeconds is > 0 and < PreSnapshotFreeze;

        if (predictionPeriod && PredictedPositions.Count == 0)
        {
            hints.ForcedMovement = new(0);
            hints.AllowDash = false;
        }

        if (predictionPeriod || PredictedPositions.Count > 0)
        {
            hints.MaxCastTime = 0;

            if (Service.Config.Get<BossModuleConfig>().AllowAutomaticActions)
                hints.ForceCancelCast = true;
        }

        // pathfinder...please stop cutting across imminent danger zones to get to your goal........just get away...........
        if (PredictedPositions.Any(p => actor.Position.InCircle(p, PredictionRadius)))
        {
            hints.GoalZonesEnabled = false;
            hints.AllowDash = false;
        }
    }
}

class P1Twister : Twister
{
    public P1Twister(BossModule module) : base(module, AID.Twister, 0.3f) { KeepOnPhaseChange = true; }
}

// prevents dropping twisters in our prescribed stack spot
class P1TwisterFireball(BossModule module) : Components.CastCounter(module, AID.Twister)
{
    bool Enabled = true;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        Enabled |= spell.Action == WatchedAction;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Enabled)
            return;

        var dropPosition = Module.PrimaryActor.Position + Module.PrimaryActor.DirectionTo(Arena.Center) * (Module.PrimaryActor.HitboxRadius + 3);

        hints.AddForbiddenZone(ShapeDistance.Circle(dropPosition, 2.5f), DateTime.MaxValue);
    }
}
