namespace BossMod.RealmReborn.Alliance.A24Xande;

class KnucklePress(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.KnucklePress), new AOEShapeCircle(10));
class BurningRave1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BurningRave1), 8);
class BurningRave2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BurningRave2), 8);
class AncientQuake(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AncientQuake));
class AncientQuaga(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AncientQuaga));
class AuraCannon(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AuraCannon), new AOEShapeRect(60, 5));
//class Stackmarker(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stackmarker, ActionID.MakeSpell(AID.KnucklePress), 6, 2, 4);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 102, NameID = 2824)]
public class A24Xande(WorldState ws, Actor primary) : BossModule(ws, primary, new(-400, -200), new ArenaBoundsCircle(35))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.StonefallCircle), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.StarfallCircle), ArenaColor.Enemy);
    }
}
