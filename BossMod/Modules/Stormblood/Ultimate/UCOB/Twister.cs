namespace BossMod.Stormblood.Ultimate.UCOB;

class Twister : Components.CastTwister
{
    public Twister() : base(2, (uint)OID.VoidzoneTwister, ActionID.MakeSpell(AID.Twister), 0.3f, 0.5f) { } // TODO: verify radius
}

class P1Twister : Twister
{
    public P1Twister() { KeepOnPhaseChange = true; }
}
