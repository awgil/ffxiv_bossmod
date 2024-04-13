#if false
namespace BossMod.StrikingDummy
{
    public enum OID : uint
    {
        Boss = 0x385,
    }

    class StrikingDummyStates : StateMachineBuilder
    {
        public StrikingDummyStates(BossModule module) : base(module)
        {
            TrivialPhase();
        }
    }

    public class StrikingDummy(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
}
#endif
