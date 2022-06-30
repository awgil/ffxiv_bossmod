namespace BossMod.Endwalker.Savage.P3SPhoinix
{
    [ConfigDisplay(Order = 0x130, Parent = typeof(EndwalkerConfig))]
    public class P3SConfig : CooldownPlanningConfigNode { }

    [CooldownPlanning(typeof(P3SConfig))]
    public class P3S : BossModule
    {
        public P3S(BossModuleManager manager, Actor primary)
            : base(manager, primary, new ArenaBoundsCircle(new(100, 100), 20))
        {
            StateMachine = new P3SStates(this).Build();
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        }
    }
}
