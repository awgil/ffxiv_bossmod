namespace BossMod.Endwalker.Criterion.C03AAI.C031Ketuduke;

class TidalRoar(BossModule module, AID aid) : Components.CastCounter(module, ActionID.MakeSpell(aid));
class NTidalRoar(BossModule module) : TidalRoar(module, AID.NTidalRoarAOE);
class STidalRoar(BossModule module) : TidalRoar(module, AID.STidalRoarAOE);

class BubbleNet(BossModule module, AID aid) : Components.CastCounter(module, ActionID.MakeSpell(aid));
class NBubbleNet1(BossModule module) : BubbleNet(module, AID.NBubbleNet1AOE);
class SBubbleNet1(BossModule module) : BubbleNet(module, AID.SBubbleNet1AOE);
class NBubbleNet2(BossModule module) : BubbleNet(module, AID.NBubbleNet2AOE);
class SBubbleNet2(BossModule module) : BubbleNet(module, AID.SBubbleNet2AOE);

class Hydrobomb(BossModule module, AID aid) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(aid), 5);
class NHydrobomb(BossModule module) : Hydrobomb(module, AID.NHydrobombAOE);
class SHydrobomb(BossModule module) : Hydrobomb(module, AID.SHydrobombAOE);

public abstract class C031Ketuduke(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsSquare(20));

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979, NameID = 12605, SortOrder = 5)]
public class C031NKetuduke(WorldState ws, Actor primary) : C031Ketuduke(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980, NameID = 12605, SortOrder = 5)]
public class C031SKetuduke(WorldState ws, Actor primary) : C031Ketuduke(ws, primary);
