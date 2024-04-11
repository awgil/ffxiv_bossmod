namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to devouring brand mechanic
class DevouringBrand(BossModule module) : BossComponent(module)
{
    private static readonly float _halfWidth = 5;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var offset = actor.Position - Module.Bounds.Center;
        if (MathF.Abs(offset.X) <= _halfWidth || MathF.Abs(offset.Z) <= _halfWidth)
        {
            hints.Add("GTFO from brand!");
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        Arena.ZoneRect(Module.Bounds.Center, new WDir(1,  0), Module.Bounds.HalfSize, Module.Bounds.HalfSize, _halfWidth, ArenaColor.AOE);
        Arena.ZoneRect(Module.Bounds.Center, new WDir(0,  1), Module.Bounds.HalfSize, -_halfWidth, _halfWidth, ArenaColor.AOE);
        Arena.ZoneRect(Module.Bounds.Center, new WDir(0, -1), Module.Bounds.HalfSize, -_halfWidth, _halfWidth, ArenaColor.AOE);
    }
}
