namespace BossMod.Endwalker.Alliance.A34Eulogia;

class ArenaChanges(BossModule module) : BossComponent(module)
{
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x1B)
        {
            if (state == 0x00080004)
                Module.Arena.Bounds = new ArenaBoundsCircle(new(945, -945), 35);
            if (state is 0x00020001 or 0x00100001)
                Module.Arena.Bounds = new ArenaBoundsCircle(new(945, -945), 30);
            if (state == 0x00400020)
                Module.Arena.Bounds = new ArenaBoundsSquare(new(945, -945), 24);
        }
    }
}

class A34EulogiaStates : StateMachineBuilder
{
    public A34EulogiaStates(BossModule module) : base(module)
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
