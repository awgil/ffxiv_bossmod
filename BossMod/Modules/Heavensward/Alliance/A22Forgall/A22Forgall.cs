namespace BossMod.Heavensward.Alliance.A22Forgall;

class BrandOfTheFallen(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.BrandOfTheFallen), 6, 8);
class MegiddoFlame2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MegiddoFlame2), new AOEShapeRect(50, 4));
class DarkEruption2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.DarkEruption2), 6);

class MortalRay(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MortalRay), new AOEShapeCone(20, 22.5f.Degrees()));
class Mow(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Mow), new AOEShapeCone(13.8f, 60.Degrees()));
class TailDrive(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TailDrive), new AOEShapeCone(30, 45.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 168, NameID = 4878)]
public class A22Forgall(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -416), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.SummonedDahak), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.SummonedSuccubus), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.SummonedHaagenti), ArenaColor.Enemy);
    }
}
