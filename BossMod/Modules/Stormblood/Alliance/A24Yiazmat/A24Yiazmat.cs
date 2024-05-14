namespace BossMod.Stormblood.Alliance.A24Yiazmat;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 550, NameID = 7070)]
public class A24Yiazmat(WorldState ws, Actor primary) : BossModule(ws, primary, new(800, -400), new ArenaBoundsCircle(30));
