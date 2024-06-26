namespace BossMod.Stormblood.Alliance.A21Famfrit;

class TidePod(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.TidePod));
class WaterIV(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WaterIV));
class DarkeningDeluge(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.DarkeningDeluge), 6);
class Tsunami9(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Tsunami9), 15, stopAtWall: true, kind: Kind.AwayFromOrigin);
class Materialize(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Materialize), new AOEShapeCircle(6));
class DarkRain2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DarkRain2), new AOEShapeCircle(8));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 550, NameID = 7245)]
public class A21Famfrit(WorldState ws, Actor primary) : BossModule(ws, primary, new(-200, 66.5f), new ArenaBoundsCircle(30));
