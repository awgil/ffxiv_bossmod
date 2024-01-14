namespace BossMod.Shadowbringers.Foray.Duel.Duel2Lyon
{

class LyonStates : StateMachineBuilder
    {
        public LyonStates(BossModule module) : base(module)
        {
            TrivialPhase()
            .ActivateOnEnter<HeartOfNatureConcentric>();
        }
    }
public class Lyon : BossModule
    {
        public Lyon(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(211, 380), 20)) {}
    }
}