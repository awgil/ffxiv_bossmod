namespace BossMod.Dawntrail.Savage.RM12S1TheLindwurm;

// cruel coil: boss hitbox is 10y-13y donut
// gap is 45 degrees
class CruelCoil(BossModule module) : BossComponent(module)
{
    public static readonly AOEShape CoilShape = new AOEShapeDonutSector(9.5f, 13.5f, 157.5f.Degrees());

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Module.PrimaryActor.Position.InCircle(Arena.Center, 5))
        {
            CoilShape.Outline(Arena, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + 180.Degrees(), ArenaColor.Object);
        }
    }
}
