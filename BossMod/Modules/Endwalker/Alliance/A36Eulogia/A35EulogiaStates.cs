namespace BossMod.Endwalker.Alliance.A36Eulogia;

class ArenaChanges : BossComponent
{
    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        if (index == 0x1B)
        {
            if (state == 0x00080004)
                module.Arena.Bounds = new ArenaBoundsCircle(new(945, -945), 35);
            if (state == 0x00020001)
                module.Arena.Bounds = new ArenaBoundsCircle(new(945, -945), 30);
            if (state == 0x00100001)
                module.Arena.Bounds = new ArenaBoundsCircle(new(945, -945), 30);
            if (state == 0x00400020)
                module.Arena.Bounds = new ArenaBoundsSquare(new(945, -945), 24);
        }
    }
}

class A35EulogiaStates : StateMachineBuilder
{
    public A35EulogiaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<SunbeamSelf>()
            .ActivateOnEnter<ByregotStrikeJump>()
            .ActivateOnEnter<ByregotStrikeKnockback>()
            .ActivateOnEnter<ByregotStrikeCone>()
            .ActivateOnEnter<Hydrostasis>()
            .ActivateOnEnter<DestructiveBoltStack>()
            .ActivateOnEnter<Hieroglyphika>()
            .ActivateOnEnter<HandOfTheDestroyerWrath>()
            .ActivateOnEnter<HandOfTheDestroyerJudgment>()
            .ActivateOnEnter<AsAboveSoBelow>()
            .ActivateOnEnter<MatronsBreath>()
            .ActivateOnEnter<SoaringMinuet>()
            .ActivateOnEnter<TorrentialTridents>()
            .ActivateOnEnter<Tridents>()
            .ActivateOnEnter<FirstBlush1>()
            .ActivateOnEnter<FirstBlush2>()
            .ActivateOnEnter<FirstBlush3>()
            .ActivateOnEnter<FirstBlush4>()
            .ActivateOnEnter<ThousandfoldThrust>()
            .ActivateOnEnter<SolarFans>()
            .ActivateOnEnter<ClimbingShot1>()
            .ActivateOnEnter<ClimbingShot2>()
            .ActivateOnEnter<Quintessence>();
    }
}