namespace BossMod.Dawntrail.Variant.V03DaryaSeaMaid;

class PiercingPlunge(BossModule module) : Components.RaidwideCast(module, AID.PiercingPlunge);

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1084, NameID = 14291)]
public class V03DaryaTheSeaMaid(WorldState ws, Actor primary) : BossModule(ws, primary, new(375, 530), new ArenaBoundsSquare(20));
