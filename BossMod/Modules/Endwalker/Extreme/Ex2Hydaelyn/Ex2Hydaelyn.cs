namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn
{
    // state related to mousa scorn mechanic (shared tankbuster)
    class MousaScorn : Components.CastSharedTankbuster
    {
        public MousaScorn() : base(ActionID.MakeSpell(AID.MousaScorn), 4) { }
    }

    // cast counter for pre-intermission AOE
    class PureCrystal : Components.CastCounter
    {
        public PureCrystal() : base(ActionID.MakeSpell(AID.PureCrystal)) { }
    }

    // cast counter for post-intermission AOE
    class Exodus : Components.CastCounter
    {
        public Exodus() : base(ActionID.MakeSpell(AID.Exodus)) { }
    }

    [ConfigDisplay(Order = 0x020, Parent = typeof(EndwalkerConfig))]
    public class Ex2HydaelynConfig : CooldownPlanningConfigNode
    {
        public Ex2HydaelynConfig() : base(90) { }
    }

    [ModuleInfo(CFCID = 791, NameID = 10453)]
    public class Ex2Hydaelyn : BossModule
    {
        public Ex2Hydaelyn(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }
    }
}
