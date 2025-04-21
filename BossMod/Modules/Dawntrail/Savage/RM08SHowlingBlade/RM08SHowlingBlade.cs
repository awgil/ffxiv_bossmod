namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class ExtraplanarPursuit(BossModule module) : Components.RaidwideCastDelay(module, AID.ExtraplanarPursuitVisual, AID.ExtraplanarPursuit, 2.4f);

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1026, NameID = 13843)]
public class RM08SHowlingBlade(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(12));

