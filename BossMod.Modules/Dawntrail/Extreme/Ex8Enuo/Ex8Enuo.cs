namespace BossMod.Dawntrail.Extreme.Ex8Enuo;

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1116, NameID = 14749, PlanLevel = 100)]
public class Ex8Enuo(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    public Actor? LoomingShadow() => Enemies(OID.LoomingShadow).FirstOrDefault();

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies(OID.LoomingShadow), ArenaColor.Enemy);
    }
}

