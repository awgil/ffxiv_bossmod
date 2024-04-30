namespace BossMod.Endwalker.Savage.P4S1Hesperos;

// state related to elegant evisceration mechanic (dual hit tankbuster)
// TODO: consider showing some tank swap / invul hint...
public class ElegantEvisceration(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.ElegantEviscerationSecond));

[ConfigDisplay(Order = 0x141, Parent = typeof(EndwalkerConfig))]
public class P4S1Config() : CooldownPlanningConfigNode(90);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 801, NameID = 10744, SortOrder = 1)]
public class P4S1(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));
