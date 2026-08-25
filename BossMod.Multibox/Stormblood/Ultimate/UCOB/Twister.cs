namespace BossMod.Stormblood.Ultimate.UCOB;

class Twister(BossModule module) : Components.CastTwister(module, 2, (uint)OID.VoidzoneTwister, AID.Twister, 0.3f, 0.5f); // TODO: verify radius

class P1Twister : Twister
{
    public P1Twister(BossModule module) : base(module) { KeepOnPhaseChange = true; }
}
