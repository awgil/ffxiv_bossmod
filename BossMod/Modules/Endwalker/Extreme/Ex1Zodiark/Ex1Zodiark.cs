namespace BossMod.Endwalker.Extreme.Ex1Zodiark
{
    // simple component tracking raidwide cast at the end of intermission
    public class Apomnemoneumata : CommonComponents.CastCounter
    {
        public Apomnemoneumata() : base(ActionID.MakeSpell(AID.ApomnemoneumataNormal)) { }
    }

    public class Phlegethon : CommonComponents.Puddles
    {
        public Phlegethon() : base(ActionID.MakeSpell(AID.PhlegetonAOE), 5) { }
    }

    [ConfigDisplay(Order = 0x010, Parent = typeof(EndwalkerConfig))]
    public class Ex1ZodiarkConfig : CooldownPlanningConfigNode { }

    [CooldownPlanning(typeof(Ex1ZodiarkConfig))]
    public class Ex1Zodiark : BossModule
    {
        public Ex1Zodiark(WorldState ws, Actor primary)
            : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 20))
        {
            StateMachine = new Ex1ZodiarkStates(this).Build();
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        }
    }
}
