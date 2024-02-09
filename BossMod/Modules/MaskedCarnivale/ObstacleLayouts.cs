using System.Collections.Generic;

namespace BossMod.MaskedCarnivale
{
    public class Layout2Corners : BossComponent
    {
        public static IEnumerable<WPos> Wall1()
        {
            yield return new WPos(85,95);
            yield return new WPos(95,95);
            yield return new WPos(95,89);
            yield return new WPos(94.5f,89);
            yield return new WPos(94.5f,94.5f);
            yield return new WPos(85,94.5f);
        }

        public static IEnumerable<WPos> Wall2()
        {
            yield return new WPos(105,95);
            yield return new WPos(115,95);
            yield return new WPos(115,94.5f);
            yield return new WPos(105.5f,94.5f);
            yield return new WPos(105.5f,89);
            yield return new WPos(105,89);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.AddPolygon(Wall1(),ArenaColor.Border);
            arena.AddPolygon(Wall2(),ArenaColor.Border);
        }
    }

    public class Layout4Quads : BossComponent
    {
        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.AddQuad(new(107,110),new(110,113),new(113,110),new(110,107), ArenaColor.Border, 2);
            arena.AddQuad(new(93,110),new(90,107),new(87,110),new(90,113), ArenaColor.Border, 2);
            arena.AddQuad(new(90,93),new(93,90),new(90,87),new(87,90), ArenaColor.Border, 2);
            arena.AddQuad(new(110,93),new(113,90),new(110,87),new(107,90), ArenaColor.Border, 2);
        }
    }
 
    public class LayoutBigQuad : BossComponent
    {
        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.AddQuad(new(100,107),new(107,100),new(100,93),new(93,100), ArenaColor.Border, 2);
        }
    }
}
