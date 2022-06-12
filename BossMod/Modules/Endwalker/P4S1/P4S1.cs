namespace BossMod.Endwalker.P4S1
{
    // state related to elegant evisceration mechanic (dual hit tankbuster)
    // TODO: consider showing some tank swap / invul hint...
    public class ElegantEvisceration : CommonComponents.CastCounter
    {
        public ElegantEvisceration() : base(ActionID.MakeSpell(AID.ElegantEviscerationSecond)) { }
    }

    public class P4S1 : BossModule
    {
        public P4S1(BossModuleManager manager, Actor primary)
            : base(manager, primary, new ArenaBoundsSquare(new(100, 100), 20))
        {
            InitStates(new P4S1States(this).Build());
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            Arena.Actor(pc, ArenaColor.PC);
        }
    }
}
