namespace BossMod.Endwalker.HydaelynEx
{
    // state related to mousa scorn mechanic (shared tankbuster)
    class MousaScorn : CommonComponents.SharedTankbuster
    {
        public MousaScorn() : base(ActionID.MakeSpell(AID.MousaScorn), 4) { }
    }

    // cast counter for pre-intermission AOE
    class PureCrystal : CommonComponents.CastCounter
    {
        public PureCrystal() : base(ActionID.MakeSpell(AID.PureCrystal)) { }
    }

    // cast counter for post-intermission AOE
    class Exodus : CommonComponents.CastCounter
    {
        public Exodus() : base(ActionID.MakeSpell(AID.Exodus)) { }
    }

    public class HydaelynEx : BossModule
    {
        public HydaelynEx(BossModuleManager manager, Actor primary)
            : base(manager, primary)
        {
            Arena.IsCircle = true;
            InitStates(new HydaelynExStates(this).Build());
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }
    }
}
