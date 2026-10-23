namespace BossMod.Stormblood.Ultimate.UCOB;

class Twister(BossModule module, AID cast, float delay, float predelay) : Components.CastTwister(module, 1.25f, (uint)OID.VoidzoneTwister, cast, delay, predictBeforeSpawn: predelay)
{
    // how early before the predicted twister appearance should movement, casts, and gapclosers be prevented
    // movement before resolution will kill us because our movement will be interpolated serverside
    // additionally, dashes or slidecasting forces a position update, which may cause the twister to unavoidably spawn on top of the player
    // this is COMPLETELY arbitrary, there is probably no practical way to actually measure what this value "should" be
    // this specific value is based on what worked for the regular Twister cast (2 second cast time + 0.3 spawn delay - 0.65 for prediction time)
    const double PreSnapshotFreeze = 1.65;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var t in ActiveTwisters)
        {
            hints.AddForbiddenZone(ShapeDistance.Circle(t.Position, Radius));
            hints.Voidzones.Add((t.Position, Radius));
        }
        foreach (var z in PredictedPositions)
        {
            hints.AddForbiddenZone(ShapeDistance.Circle(z, Radius));
            hints.Voidzones.Add((z, Radius));
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
            hints.ForceCancelCast = true;
        }

        // pathfinder...please stop cutting across imminent danger zones to get to your goal........just get away...........
        if (PredictedPositions.Any(p => actor.Position.InCircle(p, Radius)))
        {
            hints.GoalZonesEnabled = false;
            hints.AllowDash = false;
        }
    }
}

class P1Twister : Twister
{
    public P1Twister(BossModule module) : base(module, AID.Twister, 0.3f, 0.65f) { KeepOnPhaseChange = true; }
}
