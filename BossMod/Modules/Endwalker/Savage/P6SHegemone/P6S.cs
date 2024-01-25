namespace BossMod.Endwalker.Savage.P6SHegemone
{
    class UnholyDarkness : Components.StackWithCastTargets
    {
        public UnholyDarkness() : base(ActionID.MakeSpell(AID.UnholyDarknessAOE), 6) { }
    }

    class DarkDome : Components.LocationTargetedAOEs
    {
        public DarkDome() : base(ActionID.MakeSpell(AID.DarkDomeAOE), 5) { }
    }

    class DarkAshes : Components.SpreadFromCastTargets
    {
        public DarkAshes() : base(ActionID.MakeSpell(AID.DarkAshesAOE), 6) { }
    }

    class DarkSphere : Components.SpreadFromCastTargets
    {
        public DarkSphere() : base(ActionID.MakeSpell(AID.DarkSphereAOE), 10) { }
    }

    [ConfigDisplay(Order = 0x160, Parent = typeof(EndwalkerConfig))]
    public class P6SConfig : CooldownPlanningConfigNode
    {
        public P6SConfig() : base(90) { }
    }

    [ModuleInfo(CFCID = 881, NameID = 11381)]
    public class P6S : BossModule
    {
        public P6S(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 20)) { }
    }
}
