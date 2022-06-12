namespace BossMod.Endwalker.P3S
{
    public class P3S : BossModule
    {
        public P3S(BossModuleManager manager, Actor primary)
            : base(manager, primary, new ArenaBoundsCircle(new(100, 100), 20))
        {
            InitStates(new P3SStates(this).Build());
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }
    }
}
