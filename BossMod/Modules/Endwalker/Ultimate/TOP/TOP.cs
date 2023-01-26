namespace BossMod.Endwalker.Ultimate.TOP
{
    public class TOP : BossModule
    {
        public TOP(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }
    }
}
