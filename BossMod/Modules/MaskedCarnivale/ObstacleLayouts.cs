using System.Collections.Generic;

namespace BossMod.MaskedCarnivale
{
    public class Layout2Corners(WorldState ws, Actor primary, ArenaBounds bounds) : BossModule(ws, primary, bounds)
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
            protected override void DrawArenaForeground(int pcSlot, Actor pc)
            {
                Arena.AddPolygon(Wall1(),ArenaColor.Border);
                Arena.AddPolygon(Wall2(),ArenaColor.Border);
            }
    }

    public class Layout4Quads(WorldState ws, Actor primary, ArenaBounds bounds) : BossModule(ws, primary, bounds)
    {
        protected override void DrawArenaForeground(int pcSlot, Actor pc)
            {
                Arena.AddQuad(new(107,110),new(110,113),new(113,110),new(110,107), ArenaColor.Border, 2);
                Arena.AddQuad(new(93,110),new(90,107),new(87,110),new(90,113), ArenaColor.Border, 2);
                Arena.AddQuad(new(90,93),new(93,90),new(90,87),new(87,90), ArenaColor.Border, 2);
                Arena.AddQuad(new(110,93),new(113,90),new(110,87),new(107,90), ArenaColor.Border, 2);
            }
    }

    public class LayoutBigQuad(WorldState ws, Actor primary, ArenaBounds bounds) : BossModule(ws, primary, bounds)
    {
        protected override void DrawArenaForeground(int pcSlot, Actor pc)
            {
                Arena.AddQuad(new(100,107),new(107,100),new(100,93),new(93,100), ArenaColor.Border, 2);
            }
    }
}