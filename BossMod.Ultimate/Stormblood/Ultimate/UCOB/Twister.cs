namespace BossMod.Stormblood.Ultimate.UCOB;

class Twister(BossModule module) : Components.CastTwister(module, 1.25f, (uint)OID.VoidzoneTwister, AID.Twister, 0.3f, predictBeforeSpawn: 0.8f)
{
    readonly bool _jump = Service.Config.Get<UCOBConfig>().TwisterForceJump;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_jump && _predictAt > WorldState.CurrentTime && _predictAt < WorldState.FutureTime(0.5f))
            hints.WantJump = true;

        //if (_jump && PredictedPositions.Count > 0 && !ActiveTwisters.Any() && (actor.LastFrameMovement.Length() / WorldState.Frame.Duration) > 3)
    }
}

class P1Twister : Twister
{
    public P1Twister(BossModule module) : base(module) { KeepOnPhaseChange = true; }
}
