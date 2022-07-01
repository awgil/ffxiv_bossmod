namespace BossMod.RealmReborn.Extreme.Ex1Ultima
{
    public class Ex1UltimaStates : StateMachineBuilder
    {
        public Ex1UltimaStates(BossModule module) : base(module)
        {
            // TODO: reconsider
            TrivialPhase(600);
        }
    }

    public class Ex1Ultima : BossModule
    {
        public Ex1Ultima(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 0), 20)) { }
    }
}
