namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to devouring brand mechanic
class DevouringBrand : BossComponent
{
    private static readonly float _halfWidth = 5;

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        var offset = actor.Position - module.Bounds.Center;
        if (MathF.Abs(offset.X) <= _halfWidth || MathF.Abs(offset.Z) <= _halfWidth)
        {
            hints.Add("GTFO from brand!");
        }
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        arena.ZoneRect(module.Bounds.Center, new WDir(1,  0), module.Bounds.HalfSize, module.Bounds.HalfSize, _halfWidth, ArenaColor.AOE);
        arena.ZoneRect(module.Bounds.Center, new WDir(0,  1), module.Bounds.HalfSize, -_halfWidth, _halfWidth, ArenaColor.AOE);
        arena.ZoneRect(module.Bounds.Center, new WDir(0, -1), module.Bounds.HalfSize, -_halfWidth, _halfWidth, ArenaColor.AOE);
    }
}
