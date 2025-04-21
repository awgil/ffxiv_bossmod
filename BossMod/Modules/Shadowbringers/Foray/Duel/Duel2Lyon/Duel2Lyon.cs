namespace BossMod.Shadowbringers.Foray.Duel.Duel2Lyon;

class Duel2LyonStates : StateMachineBuilder
{
    public Duel2LyonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Enaero>()
            .ActivateOnEnter<HeartOfNatureConcentric>()
            .ActivateOnEnter<TasteOfBlood>()
            .ActivateOnEnter<RavenousGaleTwister>()
            .ActivateOnEnter<RavenousGaleHint>()
            .ActivateOnEnter<WindsPeakKB>()
            .ActivateOnEnter<WindsPeak>()
            .ActivateOnEnter<SplittingRage>()
            .ActivateOnEnter<TheKingsNotice>()
            .ActivateOnEnter<TwinAgonies>()
            .ActivateOnEnter<NaturesBlood>()
            .ActivateOnEnter<VermillionFlame>()
            .ActivateOnEnter<SpitefulFlameRect>()
            .ActivateOnEnter<DynasticFlame>()
            .ActivateOnEnter<SkyrendingStrike>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.BozjaDuel, GroupID = 735, NameID = 8)] // bnpcname=9409
public class Duel2Lyon(WorldState ws, Actor primary) : BossModule(ws, primary, new(211, 380), new ArenaBoundsCircle(20));
