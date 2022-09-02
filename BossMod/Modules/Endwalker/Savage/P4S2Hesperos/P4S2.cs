namespace BossMod.Endwalker.Savage.P4S2Hesperos
{
    // state related to demigod double mechanic (shared tankbuster)
    class DemigodDouble : Components.SharedTankbuster
    {
        public DemigodDouble() : base(ActionID.MakeSpell(AID.DemigodDouble), 6) { }
    }

    // state related to heart stake mechanic (dual hit tankbuster with bleed)
    // TODO: consider showing some tank swap / invul hint...
    class HeartStake : Components.CastCounter
    {
        public HeartStake() : base(ActionID.MakeSpell(AID.HeartStakeSecond)) { }
    }

    public class P4S2 : BossModule
    {
        // common wreath of thorns constants
        public static float WreathAOERadius = 20;
        public static float WreathTowerRadius = 4;

        public P4S2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }
    }
}
