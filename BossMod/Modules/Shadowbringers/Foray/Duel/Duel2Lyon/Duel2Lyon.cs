namespace BossMod.Shadowbringers.Foray.Duel.Duel2Lyon
{

class Lyon2DuelStates : StateMachineBuilder
    {
        public Lyon2DuelStates(BossModule module) : base(module)
        {
            TrivialPhase()
            .ActivateOnEnter<Enaero>()
            .ActivateOnEnter<HeartOfNatureConcentric>()
            .ActivateOnEnter<TasteOfBlood>()
            .ActivateOnEnter<RavenousGale>()
            .ActivateOnEnter<WindsPeakKB>()
            .ActivateOnEnter<WindsPeak>()
            .ActivateOnEnter<SplittingRage>()
            .ActivateOnEnter<TheKingsNotice>()
            .ActivateOnEnter<TwinAgonies>()
            .ActivateOnEnter<SkyrendingStrike>();
        }
    }
public class Lyon2Duel : BossModule
    {
        public Lyon2Duel(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(211, 380), 20)) {}
    }
}