namespace BossMod.RealmReborn.Extreme.Ex3Titan;

class Geocrush : Components.CastCounter
{
    private float _radius;
    private static readonly float _ringWidth = 2;

    public Geocrush(float radius) : base(ActionID.MakeSpell(AID.Geocrush))
    {
        _radius = radius;
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (!actor.Position.InCircle(module.Bounds.Center, _radius))
            hints.Add("Move closer to center!");
        else if (actor.Position.InCircle(module.Bounds.Center, _radius - _ringWidth))
            hints.Add("Move closer to the edge!");
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var ring = ShapeDistance.Donut(module.Bounds.Center, _radius - _ringWidth, _radius);
        hints.AddForbiddenZone(p => -ring(p));
        hints.PredictedDamage.Add((module.Raid.WithSlot().Mask(), new()));
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        arena.ZoneDonut(module.Bounds.Center, _radius, 25, ArenaColor.AOE);
        arena.ZoneDonut(module.Bounds.Center, _radius - _ringWidth, _radius, ArenaColor.SafeFromAOE);
    }
}

class Geocrush1 : Geocrush
{
    public const float Radius = 15;
    public Geocrush1() : base(Radius) { }
}

class Geocrush2 : Geocrush
{
    public const float Radius = 12;
    public Geocrush2() : base(Radius) { }
}
