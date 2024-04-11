namespace BossMod.Stormblood.Ultimate.UCOB;

class P5Teraflare(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.Teraflare))
{
    public bool DownForTheCountAssigned;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DownForTheCount)
            DownForTheCountAssigned = true;
    }
}

class P5FlamesOfRebirth(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.FlamesOfRebirth));
