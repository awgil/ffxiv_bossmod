namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

// simple component tracking raidwide cast at the end of intermission
public class Apomnemoneumata(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.ApomnemoneumataNormal));

public class Phlegethon(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.PhlegetonAOE), 5);

[ConfigDisplay(Order = 0x010, Parent = typeof(EndwalkerConfig))]
public class Ex1ZodiarkConfig() : CooldownPlanningConfigNode(90);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 803, NameID = 10456)]
public class Ex1Zodiark(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));
