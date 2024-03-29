namespace BossMod.Stormblood.Ultimate.UCOB;

class P5Teraflare : Components.CastCounter
{
    public bool DownForTheCountAssigned;

    public P5Teraflare() : base(ActionID.MakeSpell(AID.Teraflare)) { }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DownForTheCount)
            DownForTheCountAssigned = true;
    }
}

class P5FlamesOfRebirth : Components.CastCounter
{
    public P5FlamesOfRebirth() : base(ActionID.MakeSpell(AID.FlamesOfRebirth)) { }
}
