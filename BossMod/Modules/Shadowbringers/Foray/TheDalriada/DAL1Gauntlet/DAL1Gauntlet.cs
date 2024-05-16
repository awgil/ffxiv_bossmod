namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Gauntlet;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 735, NameID = 9834)]
public class DAl1Gauntlet(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 278), new ArenaBoundsCircle(25));
