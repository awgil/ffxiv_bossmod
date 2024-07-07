namespace BossMod.Shadowbringers.Alliance.A33RedGirlP1;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9920)]
public class A33RedGirlP1(WorldState ws, Actor primary) : BossModule(ws, primary, new(845, -850), new ArenaBoundsCircle(30));
