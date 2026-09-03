namespace BossMod.Stormblood.Ultimate.UCOB;

class Twister(BossModule module) : Components.CastTwister(module, 1.25f, (uint)OID.VoidzoneTwister, AID.Twister, 0.3f, predictBeforeSpawn: 0.8f);

class P1Twister : Twister
{
    public P1Twister(BossModule module) : base(module) { KeepOnPhaseChange = true; }
}
