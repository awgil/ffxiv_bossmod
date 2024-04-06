namespace BossMod.Endwalker.Alliance.A36Eulogia;

class ArenaChanges : BossComponent
{
    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        if (index == 0x1B)
        {
            if (state == 0x00080004)
                module.Arena.Bounds = new ArenaBoundsCircle(new(945, -945), 35);
            if (state is 0x00020001 or 0x00100001)
                module.Arena.Bounds = new ArenaBoundsCircle(new(945, -945), 30);
            if (state == 0x00400020)
                module.Arena.Bounds = new ArenaBoundsSquare(new(945, -945), 24);
        }
    }
}

class A36EulogiaStates : StateMachineBuilder
{
    public A36EulogiaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<DawnOfTime>()
            .ActivateOnEnter<EudaimonEorzea>()
            .ActivateOnEnter<TheWhorl>()
            .ActivateOnEnter<Sunbeam>()
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
            .ActivateOnEnter<FirstBlush>()
            .ActivateOnEnter<ThousandfoldThrust>()
            .ActivateOnEnter<RadiantRhythm>()
            .ActivateOnEnter<RadiantFlourish>()
            .ActivateOnEnter<ClimbingShot>()
            .ActivateOnEnter<Quintessence>();
    }
}
