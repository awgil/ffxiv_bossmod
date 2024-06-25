namespace BossMod.Heavensward.Alliance.A13Cuchulainn;

class CorrosiveBile1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CorrosiveBile1), new AOEShapeCone(25, 45.Degrees()));
class FlailingTentacles2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FlailingTentacles2), new AOEShapeRect(39, 3.5f));
class Beckon(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Beckon), new AOEShapeCone(36.875f, 30.Degrees()));
class BileBelow(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BileBelow));
class Pestilence(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Pestilence));
class BlackLung(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BlackLung));
class GrandCorruption(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GrandCorruption));
class FlailingTentacles2Knockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.FlailingTentacles2), 30, stopAtWall: true);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 120, NameID = 4626)]
public class A13Cuchulainn(WorldState ws, Actor primary) : BossModule(ws, primary, new(288, 138.5f), new ArenaBoundsCircle(29.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Foobar), ArenaColor.Enemy);
    }
}