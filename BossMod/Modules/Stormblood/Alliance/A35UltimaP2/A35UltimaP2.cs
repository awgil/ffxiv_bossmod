namespace BossMod.Stormblood.Alliance.A35UltimaP2;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7909)]
public class A35UltimaP2(WorldState ws, Actor primary) : BossModule(ws, primary, new(600, -600), new ArenaBoundsSquare(40));
