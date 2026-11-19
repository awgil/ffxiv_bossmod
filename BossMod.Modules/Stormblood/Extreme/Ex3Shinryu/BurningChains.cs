namespace BossMod.Stormblood.Extreme.Ex3Shinryu;

class BurningChains : Components.Chains
{
    public BurningChains(BossModule module) : base(module, (uint)TetherID.BurningChains, AID.BurningChains, 15, activationDelay: 3.6f)
    {
        KeepOnPhaseChange = true;
    }
}
