namespace BossMod.RealmReborn.Alliance.A14Phlegethon;

class MegiddoFlame2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MegiddoFlame2), 3);
class MegiddoFlame3(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MegiddoFlame3), 4);
class MegiddoFlame4(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MegiddoFlame4), 5);
class MegiddoFlame5(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MegiddoFlame5), 6);
class MoonfallSlash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MoonfallSlash), new AOEShapeCone(15, 60.Degrees()));
class VacuumSlash2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.VacuumSlash2), new AOEShapeCone(80, 22.5f.Degrees()));
class AncientFlare1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AncientFlare1), new AOEShapeCircle(35));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 732)]
public class A14Phlegethon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-110, 180), new ArenaBoundsCircle(35))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.IronClaws), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.IronGiant), ArenaColor.Enemy);
    }
}
