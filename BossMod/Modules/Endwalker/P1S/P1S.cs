using System.Numerics;

namespace BossMod.Endwalker.P1S
{
    public class P1S : BossModule
    {
        public static float InnerCircleRadius { get; } = 12; // this determines in/out flails and cells boundary

        public P1S(BossModuleManager manager, Actor primary)
            : base(manager, primary, new ArenaBoundsSquare(new(100, 100), 20))
        {
            InitStates(new P1SStates(this).Build());
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            if (Bounds is ArenaBoundsCircle)
            {
                // cells mode
                float diag = Bounds.HalfSize / 1.414214f;
                Arena.AddCircle(Bounds.Center, InnerCircleRadius, Arena.ColorBorder);
                Arena.AddLine(Bounds.Center + new WDir(Bounds.HalfSize, 0), Bounds.Center - new WDir(Bounds.HalfSize, 0), Arena.ColorBorder);
                Arena.AddLine(Bounds.Center + new WDir(0, Bounds.HalfSize), Bounds.Center - new WDir(0, Bounds.HalfSize), Arena.ColorBorder);
                Arena.AddLine(Bounds.Center + new WDir(diag,  diag), Bounds.Center - new WDir(diag,  diag), Arena.ColorBorder);
                Arena.AddLine(Bounds.Center + new WDir(diag, -diag), Bounds.Center - new WDir(diag, -diag), Arena.ColorBorder);
            }

            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }
    }
}
