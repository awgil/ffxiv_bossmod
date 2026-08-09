namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

sealed class Teleport(BossModule module) : Components.CastCounter(module, (uint)AID.Teleport);
sealed class TeraSlash(BossModule module) : Components.CastCounter(module, (uint)AID.TeraSlash);
sealed class DoomArc(BossModule module) : Components.RaidwideCast(module, (uint)AID.DoomArc);
sealed class UnbridledRage(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(100f, 4f), (uint)IconID.UnbridledRage, (uint)AID.UnbridledRageAOE, 5.9d);
sealed class DarkNova(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.DarkNova, 6f);

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015u, NameID = 13653u, SortOrder = 8, PlanLevel = 100)]
public sealed class A14ShadowLord(WorldState ws, Actor primary) : BossModule(ws, primary, new(150f, 800f), new ArenaBoundsCircle(30f));