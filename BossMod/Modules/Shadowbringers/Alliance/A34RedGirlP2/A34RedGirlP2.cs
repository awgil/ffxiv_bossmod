namespace BossMod.Shadowbringers.Alliance.A34RedGirlP2;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9950)]
public class A34RedGirlP2(WorldState ws, Actor primary) : BossModule(ws, primary, new(845, -850), new ArenaBoundsCircle(20));
