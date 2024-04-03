namespace BossMod.Endwalker.Alliance.A35Eulogia;

class A35EulogiaStates : StateMachineBuilder
{
    public A35EulogiaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SunbeamSelf>()
            .ActivateOnEnter<ByregotStrikeJump>()
            .ActivateOnEnter<ByregotStrikeKnockback>()
            .ActivateOnEnter<ByregotStrikeCone>()
            .ActivateOnEnter<Hydrostasis>()
            .ActivateOnEnter<DestructiveBoltStack>()
            .ActivateOnEnter<Hieroglyphika>()
            .ActivateOnEnter<HandOfTheDestroyer>()
            .ActivateOnEnter<AsAboveSoBelow>()
            .ActivateOnEnter<MatronsBreath>()
            .ActivateOnEnter<SoaringMinuet>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<FirstBlush1>()
            .ActivateOnEnter<FirstBlush2>()
            .ActivateOnEnter<FirstBlush3>()
            .ActivateOnEnter<FirstBlush4>()
            .ActivateOnEnter<ThousandfoldThrust>()
            .ActivateOnEnter<SolarFans>()
            .ActivateOnEnter<ClimbingShotKnockback>()
            .ActivateOnEnter<Quintessence>();
    }
}