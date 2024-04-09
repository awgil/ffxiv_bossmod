namespace BossMod.Stormblood.Ultimate.UCOB;

class P5Teraflare : Components.CastCounter
{
    public bool DownForTheCountAssigned;

    public P5Teraflare() : base(ActionID.MakeSpell(AID.Teraflare)) { }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DownForTheCount)
            DownForTheCountAssigned = true;
    }
}

class P5FlamesOfRebirth(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.FlamesOfRebirth));
