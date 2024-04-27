namespace BossMod.Endwalker.Savage.P1SErichthonios;

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 809, NameID = 10576)]
public class P1S(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsSquare(new(100, 100), 20))
{
    public const float InnerCircleRadius = 12; // this determines in/out flails and cells boundary

    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Bounds is ArenaBoundsCircle)
        {
            // cells mode
            float diag = Bounds.Radius / 1.414214f;
            Arena.AddCircle(Bounds.Center, InnerCircleRadius, ArenaColor.Border);
            Arena.AddLine(Bounds.Center + new WDir(Bounds.Radius, 0), Bounds.Center - new WDir(Bounds.Radius, 0), ArenaColor.Border);
            Arena.AddLine(Bounds.Center + new WDir(0, Bounds.Radius), Bounds.Center - new WDir(0, Bounds.Radius), ArenaColor.Border);
            Arena.AddLine(Bounds.Center + new WDir(diag, +diag), Bounds.Center - new WDir(diag, +diag), ArenaColor.Border);
            Arena.AddLine(Bounds.Center + new WDir(diag, -diag), Bounds.Center - new WDir(diag, -diag), ArenaColor.Border);
        }
    }
}
