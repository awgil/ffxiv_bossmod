namespace BossMod.Stormblood.Alliance.A14Argath;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 281, NameID = 6925)]
public class A14Argath(WorldState ws, Actor primary) : BossModule(ws, primary, new(106, -385), new ArenaBoundsCircle(20));
