namespace BossMod.RealmReborn.Alliance.A11BoneDragon;

class Apocalypse(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Apocalypse), 6);
class EvilEye(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.EvilEye), new AOEShapeCone(105, 60.Degrees()));
class Stone(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Stone));
class Level5Petrify(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Level5Petrify), new AOEShapeCone(7.8f, 45.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 706)]
public class A11BoneDragon(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Circle(new(-450, 30), 15), new Rectangle(new(-450, 0), 20, 10, 90.Degrees())]);
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Platinal), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.RottingEye), ArenaColor.Enemy);
    }
}