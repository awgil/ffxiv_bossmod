namespace BossMod.Endwalker.P4S2
{
    // state related to demigod double mechanic (shared tankbuster)
    class DemigodDouble : CommonComponents.SharedTankbuster
    {
        public DemigodDouble() : base(6) { }
    }

    // state related to heart stake mechanic (dual hit tankbuster with bleed)
    // TODO: consider showing some tank swap / invul hint...
    class HeartStake : CommonComponents.CastCounter
    {
        public HeartStake() : base(ActionID.MakeSpell(AID.HeartStakeSecond)) { }
    }

    public class P4S2 : BossModule
    {
        // common wreath of thorns constants
        public static float WreathAOERadius = 20;
        public static float WreathTowerRadius = 4;

        public P4S2(BossModuleManager manager, Actor primary)
            : base(manager, primary, true)
        {
            Config = manager.Config.Get("Endwalker").Get<P4S2Config>();
            Arena.IsCircle = true;
            new P4S2States(this);
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }
    }
}
