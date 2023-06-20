namespace BossMod.Endwalker.Savage.P12S2PallasAthena
{
    class P12S2PallasAthenaStates : StateMachineBuilder
    {
        public P12S2PallasAthenaStates(BossModule module) : base(module)
        {
            DeathPhase(0, SinglePhase);
        }

        private void SinglePhase(uint id)
        {
            SimpleState(id + 0xFF0000, 523, "Enrage");
        }
    }

    public class P12S2PallasAthena : BossModule
    {
        public P12S2PallasAthena(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 20)) { }
    }
}
