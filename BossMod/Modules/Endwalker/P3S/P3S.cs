namespace BossMod.Endwalker.P3S
{
    public class P3S : BossModule
    {
        public P3S(BossModuleManager manager, Actor primary)
            : base(manager, primary, true)
        {
            Arena.IsCircle = true;
            InitStates(new P3SStates(this).Initial);
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }
    }
}
