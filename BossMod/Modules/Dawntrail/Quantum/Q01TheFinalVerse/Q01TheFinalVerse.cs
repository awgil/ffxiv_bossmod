namespace BossMod.Dawntrail.Quantum.Q01TheFinalVerse;

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1063, NameID = 14037, PlanLevel = 100, DevOnly = true)]
public class Q01TheFinalVerse(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600, -300), new ArenaBoundsRect(20, 15))
{
    private Actor? _eater;

    public Actor? Eater() => _eater;

    protected override void UpdateModule()
    {
        _eater ??= Enemies(OID.DevouredEater).FirstOrDefault();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actor(_eater, ArenaColor.Enemy);
    }
}
