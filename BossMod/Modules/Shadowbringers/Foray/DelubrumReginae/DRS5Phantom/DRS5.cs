namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS5Phantom;

class MaledictionOfAgony(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.MaledictionOfAgonyAOE));
class BloodyWraith(BossModule module) : Components.Adds(module, (uint)OID.BloodyWraith);
class MistyWraith(BossModule module) : Components.Adds(module, (uint)OID.MistyWraith);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761, NameID = 9755, PlanLevel = 80)]
public class DRS5(WorldState ws, Actor primary) : BossModule(ws, primary, new(202, -370), new ArenaBoundsSquare(24));
