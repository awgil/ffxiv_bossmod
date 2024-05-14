namespace BossMod.Stormblood.Trial.T09Seiryu;

class T09SeiryuStates : StateMachineBuilder
{
    public T09SeiryuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HundredTonzeSwing>()
            .ActivateOnEnter<CoursingRiverCircleAOE>()
            .ActivateOnEnter<CoursingRiverRectAOE>()
            .ActivateOnEnter<DragonsWake2>()
            .ActivateOnEnter<FifthElement>()
            .ActivateOnEnter<FortuneBladeSigil>()
            .ActivateOnEnter<GreatTyphoon28>()
            .ActivateOnEnter<GreatTyphoon34>()
            .ActivateOnEnter<GreatTyphoon40>()
            .ActivateOnEnter<InfirmSoul>()
            .ActivateOnEnter<InfirmSoulSpread>()
            .ActivateOnEnter<Kanabo1>()
            //.ActivateOnEnter<KanaboTether>() tether is not implemented yet, one iconid shared with multple tethered AIDs, multiple shapes simultaneously
            .ActivateOnEnter<OnmyoSigil2>()
            .ActivateOnEnter<SerpentDescending>()
            .ActivateOnEnter<SerpentEyeSigil2>()
            .ActivateOnEnter<YamaKagura>()
            .ActivateOnEnter<Handprint3>()
            .ActivateOnEnter<Handprint4>()
            .ActivateOnEnter<ForceOfNature1>()
            .ActivateOnEnter<ForceOfNature2>();
    }
}