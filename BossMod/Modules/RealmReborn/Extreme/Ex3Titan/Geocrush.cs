namespace BossMod.RealmReborn.Extreme.Ex3Titan;

class Geocrush : Components.CastCounter
{
    private float _radius;
    private static readonly float _ringWidth = 2;

    public Geocrush(float radius) : base(ActionID.MakeSpell(AID.Geocrush))
    {
        _radius = radius;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!actor.Position.InCircle(Module.Bounds.Center, _radius))
            hints.Add("Move closer to center!");
        else if (actor.Position.InCircle(Module.Bounds.Center, _radius - _ringWidth))
            hints.Add("Move closer to the edge!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var ring = ShapeDistance.Donut(Module.Bounds.Center, _radius - _ringWidth, _radius);
        hints.AddForbiddenZone(p => -ring(p));
        hints.PredictedDamage.Add((Raid.WithSlot().Mask(), new()));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        arena.ZoneDonut(Module.Bounds.Center, _radius, 25, ArenaColor.AOE);
        arena.ZoneDonut(Module.Bounds.Center, _radius - _ringWidth, _radius, ArenaColor.SafeFromAOE);
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
