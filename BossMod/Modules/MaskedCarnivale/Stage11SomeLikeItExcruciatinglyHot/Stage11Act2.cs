namespace BossMod.MaskedCarnivale.Stage11.Act2
{
    public enum OID : uint
    {
        Boss = 0x2719, //R=1.2
    };
    public enum AID : uint
    {
        Fulmination = 14583, // 2719->self, 23,0s cast, range 60 circle
    };
    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Same as last act, but this time there are 4 bombs. Pull them to the\nmiddle with Sticky Tongue and attack them with any AOE to keep them\ninterrupted. They are weak against wind and strong against fire.");
        } 
    }

    class Stage11Act2States : StateMachineBuilder
    {
        public Stage11Act2States(BossModule module) : base(module)
        {
            TrivialPhase()
            .DeactivateOnEnter<Hints>();
        }
    }

    public class Stage11Act2 : BossModule
    {
        public Stage11Act2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
        }
        protected override void DrawArenaForeground(int pcSlot, Actor pc)
        {
                Arena.AddQuad(new(107,110),new(110,113),new(113,110),new(110,107), ArenaColor.Border, 2);
                Arena.AddQuad(new(93,110),new(90,107),new(87,110),new(90,113), ArenaColor.Border, 2);
                Arena.AddQuad(new(90,93),new(93,90),new(90,87),new(87,90), ArenaColor.Border, 2);
                Arena.AddQuad(new(110,93),new(113,90),new(110,87),new(107,90), ArenaColor.Border, 2);
        }
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
        }
    }
}
