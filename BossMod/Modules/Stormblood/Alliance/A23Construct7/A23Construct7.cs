namespace BossMod.Stormblood.Alliance.A23Construct7;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 550, NameID = 7237)]
public class A23Construct7(WorldState ws, Actor primary) : BossModule(ws, primary, new(800, -141), new ArenaBoundsSquare(60));
