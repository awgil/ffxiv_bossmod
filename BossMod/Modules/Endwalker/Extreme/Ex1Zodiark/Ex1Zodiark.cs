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

    public class Ex1Zodiark : BossModule
    {
        public Ex1Zodiark(BossModuleManager manager, Actor primary)
            : base(manager, primary, new ArenaBoundsSquare(new(100, 100), 20))
        {
            InitStates(new Ex1ZodiarkStates(this).Build());
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            Arena.Actor(pc, ArenaColor.PC);
        }
    }
}
