namespace BossMod.Dawntrail.Alliance.A22OmegaTheOne;

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1058, NameID = 14230)]
public class A22OmegaTheOne(WorldState ws, Actor primary) : BossModule(ws, primary, new(800, 800), new ArenaBoundsRect(20, 23.8f))
{
    public Actor? Ultima() => _ultima;
    private Actor? _ultima;

    protected override void UpdateModule()
    {
        _ultima ??= Enemies(OID.UltimaTheFeared).FirstOrDefault();
    }
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_ultima, ArenaColor.Enemy);
    }
}
