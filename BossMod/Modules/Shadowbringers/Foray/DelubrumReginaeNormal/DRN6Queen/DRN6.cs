namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN6Queen;

class NorthswainsGlow(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.NorthswainsGlowAOE), new AOEShapeCircle(20));
class GodsSaveTheQueen(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.GodsSaveTheQueenAOE));

class OptimalPlaySword(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.OptimalPlaySword), new AOEShapeCircle(10));
class OptimalPlayShield(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.OptimalPlayShield), new AOEShapeDonut(5, 60));
class OptimalPlayCone(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.OptimalPlayCone), new AOEShapeCone(60, 135.Degrees()));
class PawnOff(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PawnOffReal), new AOEShapeCircle(20));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9863)]
public class DRN6Queen(WorldState ws, Actor primary) : BossModule(ws, primary, new(-272, -415), new ArenaBoundsSquare(25)); // note: arena swaps between circle and square ArenaBoundsCircle(new(-272, -415), 25));
