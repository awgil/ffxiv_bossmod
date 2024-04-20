namespace BossMod;

public class DemoModule : BossModule
{
    private class DemoComponent(BossModule module) : BossComponent(module)
    {
        public override void AddHints(int slot, Actor actor, TextHints hints)
        {
            hints.Add("Hint", false);
            hints.Add("Risk");

        }

        public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
        {
            movementHints.Add(actor.Position, actor.Position + new WDir(10, 10), ArenaColor.Danger);
        }

        public override void AddGlobalHints(GlobalHints hints)
        {
            hints.Add("Global");
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc)
        {
            Arena.ZoneCircle(Module.Bounds.Center, 12, ArenaColor.AOE);
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc)
        {
            Arena.Actor(Module.Bounds.Center, 0.Degrees(), ArenaColor.PC);
        }
    }

    //for testing ray intersections with new arena shapes
    // class RayIntersectionTest(BossModule module) : Components.Knockback(module)
    // {

    //     public override IEnumerable<Source> Sources(int slot, Actor actor)
    //     {
    //         StopAfterWall = true;
    //         yield return new(Module.Bounds.Center, 100);
    //     }
    // }

    // various arena bounds for testing
    // public DemoModule(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsUnion([new ArenaBoundsDonut(new(80, 100), 20,30), new ArenaBoundsDonut(new(120, 120), 20,30)]))
    // public DemoModule(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsUnion([new ArenaBoundsDonut(new(80, 100), 20,30), new ArenaBoundsDonut(new(120, 120), 20,30), new ArenaBoundsDonut(new(100, 100), 20,30)]))
    // public DemoModule(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsUnion([new ArenaBoundsCircle(new(105, 115), 15), new ArenaBoundsCircle(new(120, 120), 15), new ArenaBoundsCircle(new(120, 100), 15)]))
    // public DemoModule(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsUnion([new ArenaBoundsDonut(new(100, 100), 20,30), new ArenaBoundsRect(new(120, 120), 5,20,240.Degrees()), new ArenaBoundsRect(new(80, 80), 5,20,-120.Degrees()), new ArenaBoundsRect(new(80, 120), 5,20,120.Degrees())]))
    // public DemoModule(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsDonut(new(100, 100), 20, 30))
    // public DemoModule(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsUnion([new ArenaBoundsCircle(new(105, 115), 10), new ArenaBoundsCircle(new(140, 100), 10), new ArenaBoundsCircle(new(120, 95), 10)]))
    // public DemoModule(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsUnion([new ArenaBoundsDonut(new(105, 115), 5,10), new ArenaBoundsDonut(new(120, 100), 5,10)]))
    // public DemoModule(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsUnion([new ArenaBoundsDonut(new(80, 100), 10,20), new ArenaBoundsDonut(new(120, 120), 10,20), new ArenaBoundsCircle(new(100, 100), 30)]))
    public DemoModule(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 20))
    {
        ActivateComponent<DemoComponent>();
        // ActivateComponent<RayIntersectionTest>();
    }

}
