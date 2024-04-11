namespace BossMod.Endwalker.Trial.T08Asura;

class AsuriChakra(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AsuriChakra), new AOEShapeCircle(5));
class Chakra1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Chakra1), new AOEShapeDonut(6, 8));
class Chakra2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Chakra2), new AOEShapeDonut(9, 11));
class Chakra3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Chakra3), new AOEShapeDonut(12, 14));
class Chakra4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Chakra4), new AOEShapeDonut(15, 17));
class Chakra5(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Chakra4), new AOEShapeDonut(18, 20));
