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

        // movement before twister resolution will kill us due to interpolation
        if (_spawnAt > WorldState.CurrentTime && PredictedPositions.Count == 0)
            hints.ForcedMovement = new(0);
    }
}

class P1Twister : Twister
{
    public P1Twister(BossModule module) : base(module) { KeepOnPhaseChange = true; }
}
