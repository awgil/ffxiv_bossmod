namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn
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

    [ConfigDisplay(Order = 0x020, Parent = typeof(EndwalkerConfig))]
    public class Ex2HydaelynConfig : CooldownPlanningConfigNode { }

    [CooldownPlanning(typeof(Ex2HydaelynConfig))]
    public class Ex2Hydaelyn : BossModule
    {
        public Ex2Hydaelyn(BossModuleManager manager, Actor primary)
            : base(manager, primary, new ArenaBoundsCircle(new(100, 100), 20))
        {
            StateMachine = new Ex2HydaelynStates(this).Build();
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        }
    }
}
