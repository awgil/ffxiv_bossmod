using System.Numerics;

namespace BossMod.Endwalker.P1S
{
    public class P1S : BossModule
    {
        public static float InnerCircleRadius { get; } = 12; // this determines in/out flails and cells boundary

        public P1S(BossModuleManager manager, Actor primary)
            : base(manager, primary)
        {
            InitStates(new P1SStates(this).Build());
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            if (Arena.IsCircle)
            {
                // cells mode
                float diag = Arena.WorldHalfSize / 1.414214f;
                Arena.AddCircle(Arena.WorldCenter, InnerCircleRadius, Arena.ColorBorder);
                Arena.AddLine(Arena.WorldE, Arena.WorldW, Arena.ColorBorder);
                Arena.AddLine(Arena.WorldN, Arena.WorldS, Arena.ColorBorder);
                Arena.AddLine(Arena.WorldCenter + new WDir(diag,  diag), Arena.WorldCenter - new WDir(diag,  diag), Arena.ColorBorder);
                Arena.AddLine(Arena.WorldCenter + new WDir(diag, -diag), Arena.WorldCenter - new WDir(diag, -diag), Arena.ColorBorder);
            }

            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }
    }
}
