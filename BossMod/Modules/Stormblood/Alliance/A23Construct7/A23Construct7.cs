namespace BossMod.Stormblood.Alliance.A23Construct7;

class Destroy1(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Destroy1));
class Destroy2(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Destroy2));
class Accelerate(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Accelerate), 6);
class Compress1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Compress1), new AOEShapeRect(104.5f, 3.5f));
class Compress2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Compress2), new AOEShapeCross(100, 3.5f));

class Pulverize2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Pulverize2), 12);
class Dispose1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Dispose1), new AOEShapeCone(100, 45.Degrees()));
class Dispose3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Dispose3), new AOEShapeCone(100, 45.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.Construct, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 550, NameID = 7237)]
public class A23Construct7(WorldState ws, Actor primary) : BossModule(ws, primary, new(800, -141), new ArenaBoundsSquare(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Construct711))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Construct712))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Construct713))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
