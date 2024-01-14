namespace BossMod.Shadowbringers.Foray.Duel.Duel2Lyon
{
public enum OID : uint
    {
        Boss = 0x2E8D, // R0.60
        Helper = 0x233C, // R0.500
    };
class LyonStates : StateMachineBuilder
    {
        public LyonStates(BossModule module) : base(module)
        {
            TrivialPhase();
        }
    }
public class Lyon2Duel : BossModule
    {
        public Lyon2Duel(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(211, 380), 20)) {}
    }
}