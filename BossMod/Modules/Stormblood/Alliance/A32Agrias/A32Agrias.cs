namespace BossMod.Stormblood.Alliance.A32Agrias;

class DivineLight(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DivineLight));
class NorthswainsStrikeEphemeralKnight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.NorthswainsStrikeEphemeralKnight), new AOEShapeRect(60, 3));
class CleansingFlameSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.CleansingFlameSpread), 6);
class HallowedBoltAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HallowedBoltAOE), new AOEShapeCircle(15));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7916)]
public class A32Agrias(WorldState ws, Actor primary) : BossModule(ws, primary, new(600, -54), new ArenaBoundsCircle(30));
