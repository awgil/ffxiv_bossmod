namespace BossMod.Stormblood.Alliance.A31Mustadio;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7915)] // 7919 
public class A31Mustadio(WorldState ws, Actor primary) : BossModule(ws, primary, new(600, 290), new ArenaBoundsSquare(40));
