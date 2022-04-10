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
            : base(manager, primary, true)
        {
            InitStates(new P4S1States(this).Initial);
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }
    }
}
