namespace BossMod.Stormblood.Ultimate.UCOB;

class PreTwister(BossModule module) : BossComponent(module);

class Twister(BossModule module) : Components.CastTwister(module, 1.25f, (uint)OID.VoidzoneTwister, AID.Twister, 0.3f, predictBeforeSpawn: 0.65f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var t in ActiveTwisters)
            hints.AddForbiddenZone(ShapeDistance.Circle(t.Position, Radius));
        foreach (var z in PredictedPositions)
            hints.AddForbiddenZone(ShapeDistance.Circle(z, Radius));

        // movement before twister resolution will kill us due to serverside interpolation
        // timing a slidecast at the perfect time will force the twister to spawn under the player even if they dodge "correctly"
        if (_spawnAt > WorldState.CurrentTime && PredictedPositions.Count == 0)
        {
            hints.MaxCastTime = 0;
            hints.ForceCancelCast = true;
            hints.ForcedMovement = new(0);
        }

        // pathfinder...please stop cutting across imminent danger zones to get to your goal........just get away...........
        if (PredictedPositions.Any(p => actor.Position.InCircle(p, Radius)))
            hints.GoalZonesEnabled = false;
    }
}

class P1Twister : Twister
{
    public P1Twister(BossModule module) : base(module) { KeepOnPhaseChange = true; }
}
