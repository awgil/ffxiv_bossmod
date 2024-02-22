namespace BossMod.Endwalker.Savage.P1SErichthonios
{
    [ModuleInfo(CFCID = 809, NameID = 10576)]
    public class P1S : BossModule
    {
        public static float InnerCircleRadius { get; } = 12; // this determines in/out flails and cells boundary

        public P1S(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 20)) { }

        protected override void DrawArenaForeground(int pcSlot, Actor pc)
        {
            if (Bounds is ArenaBoundsCircle)
            {
                // cells mode
                float diag = Bounds.HalfSize / 1.414214f;
                Arena.AddCircle(Bounds.Center, InnerCircleRadius, ComponentType.Border);
                Arena.AddLine(Bounds.Center + new WDir(Bounds.HalfSize, 0), Bounds.Center - new WDir(Bounds.HalfSize, 0), ComponentType.Border);
                Arena.AddLine(Bounds.Center + new WDir(0, Bounds.HalfSize), Bounds.Center - new WDir(0, Bounds.HalfSize), ComponentType.Border);
                Arena.AddLine(Bounds.Center + new WDir(diag,  diag), Bounds.Center - new WDir(diag,  diag), ComponentType.Border);
                Arena.AddLine(Bounds.Center + new WDir(diag, -diag), Bounds.Center - new WDir(diag, -diag), ComponentType.Border);
            }
        }
    }
}
