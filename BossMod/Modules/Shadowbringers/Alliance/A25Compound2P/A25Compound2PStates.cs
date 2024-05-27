namespace BossMod.Shadowbringers.Alliance.A25Compound2P;

class A25Compound2PStates : StateMachineBuilder
{
    public A25Compound2PStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CentrifugalSlice>()
            .ActivateOnEnter<PrimeBladeAOE>()
            .ActivateOnEnter<PrimeBladeFront>()
            .ActivateOnEnter<PrimeBladeDonut1>()
            .ActivateOnEnter<PrimeBladeDonut2>()
            .ActivateOnEnter<RelentlessSpiralLocAOE>()
            .ActivateOnEnter<RelentlessSpiralAOE>()
            .ActivateOnEnter<ThreePartsDisdainStack>()
            .ActivateOnEnter<R012LaserLoc>()
            .ActivateOnEnter<R012LaserSpread>()
            .ActivateOnEnter<R012LaserTankBuster>()
            .ActivateOnEnter<R011LaserLine>()
            .ActivateOnEnter<EnergyCompression>();
    }
}
