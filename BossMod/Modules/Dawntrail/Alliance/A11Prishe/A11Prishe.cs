namespace BossMod.Dawntrail.Alliance.A11Prishe;

sealed class NullifyingDropkick(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.NullifyingDropkick, 6f);
sealed class Holy(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Holy, 6f);
sealed class BanishgaIV(BossModule module) : Components.RaidwideCast(module, (uint)AID.BanishgaIV);
sealed class Banishga(BossModule module) : Components.RaidwideCast(module, (uint)AID.Banishga);

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015u, NameID = 13351u, SortOrder = 2, PlanLevel = 100)]
public sealed class A11Prishe(WorldState ws, Actor primary) : BossModule(ws, primary, new(800f, 400f), new ArenaBoundsSquare(35f));
