namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9143)] //9144 and 9145 also listed as Hobbes
public class A12Hobbes(WorldState ws, Actor primary) : BossModule(ws, primary, new(-805, -240), new ArenaBoundsCircle(30));
