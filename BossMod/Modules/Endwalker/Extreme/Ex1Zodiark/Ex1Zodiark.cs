namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

// simple component tracking raidwide cast at the end of intermission
public class Apomnemoneumata(BossModule module) : Components.CastCounter(module, AID.ApomnemoneumataNormal);

public class Phlegethon(BossModule module) : Components.StandardAOEs(module, AID.PhlegetonAOE, 5);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 803, NameID = 10456, PlanLevel = 90)]
public class Ex1Zodiark(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));
