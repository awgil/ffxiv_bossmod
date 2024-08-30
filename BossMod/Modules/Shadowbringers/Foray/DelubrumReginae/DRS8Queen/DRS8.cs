namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

class NorthswainsGlow(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.NorthswainsGlowAOE), new AOEShapeCircle(20));
class CleansingSlashSecond(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.CleansingSlashSecond));
class GodsSaveTheQueen(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.GodsSaveTheQueenAOE));

// note: apparently there is no 'front unseen' status
class QueensShot(BossModule module) : Components.CastWeakpoint(module, ActionID.MakeSpell(AID.QueensShot), new AOEShapeCircle(60), 0, (uint)SID.BackUnseen, (uint)SID.LeftUnseen, (uint)SID.RightUnseen);
class TurretsTourUnseen(BossModule module) : Components.CastWeakpoint(module, ActionID.MakeSpell(AID.TurretsTourUnseen), new AOEShapeRect(50, 2.5f), 0, (uint)SID.BackUnseen, (uint)SID.LeftUnseen, (uint)SID.RightUnseen);

class OptimalOffensive(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.OptimalOffensive), 2.5f);

// note: there are two casters (as usual in bozja content for raidwides)
// TODO: not sure whether it ignores immunes, I assume so...
class OptimalOffensiveKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.OptimalOffensiveKnockback), 10, true, 1);

class OptimalPlaySword(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.OptimalPlaySword), new AOEShapeCircle(10));
class OptimalPlayShield(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.OptimalPlayShield), new AOEShapeDonut(5, 60));
class OptimalPlayCone(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.OptimalPlayCone), new AOEShapeCone(60, 135.Degrees()));
class PawnOff(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PawnOffReal), new AOEShapeCircle(20));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761, NameID = 9863, PlanLevel = 80)]
public class DRS8(WorldState ws, Actor primary) : BossModule(ws, primary, new(-272, -415), new ArenaBoundsCircle(25)); // note: initially arena is square, but it quickly changes to circle
