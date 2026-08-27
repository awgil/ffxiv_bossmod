namespace BossMod.Stormblood.Ultimate.UCOB;

class Twister(BossModule module) : Components.CastTwister(module, 2, (uint)OID.VoidzoneTwister, AID.Twister, 0.3f, 0.3f)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var p in PredictedPositions)
            yield return new(new AOEShapeCircle(3), p, default, PredictedActivation);
        foreach (var p in ActiveTwisters)
            yield return new(new AOEShapeCircle(1), p.Position);
    }
}

class P1Twister : Twister
{
    public P1Twister(BossModule module) : base(module) { KeepOnPhaseChange = true; }
}
