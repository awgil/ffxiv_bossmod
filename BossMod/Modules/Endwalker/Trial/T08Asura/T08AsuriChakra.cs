namespace BossMod.Endwalker.Trial.T08Asura;

class AsuriChakra() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.AsuriChakra), new AOEShapeCircle(5));
class Chakra1() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.Chakra1), new AOEShapeDonut(6, 8));
class Chakra2() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.Chakra2), new AOEShapeDonut(9, 11));
class Chakra3() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.Chakra3), new AOEShapeDonut(12, 14));
class Chakra4() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.Chakra4), new AOEShapeDonut(15, 17));
class Chakra5() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.Chakra4), new AOEShapeDonut(18, 20));