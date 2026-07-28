namespace BossMod.Dawntrail.Savage.M12S2Lindwurm;

sealed class ArcadiaAflame(BossModule module) : Components.RaidwideCast(module, (uint)AID.ArcadiaAflame);
sealed class IdyllicDreamRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.IdyllicDream);
sealed class LindwurmsMeteor(BossModule module) : Components.RaidwideCast(module, (uint)AID.LindwurmsMeteor);
sealed class ArcadianHell5x(BossModule module) : Components.RaidwideCast(module, (uint)AID.ArcadianHell4x);
sealed class ArcadianHell9x(BossModule module) : Components.RaidwideCast(module, (uint)AID.ArcadianHell8x);

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
GroupType = BossModuleInfo.GroupType.CFC,
StatesType = typeof(M12S2LindwurmStates),
ConfigType = typeof(M12S2LindwurmConfig),
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = typeof(TetherID),
PrimaryActorOID = (uint)OID.Boss,
Contributors = "BossMod Team, ported by Topas",
GroupID = 1075u,
NameID = 14379u,
SortOrder = 1,
PlanLevel = 100)]
public sealed class M12S2TheLindwurm : BossModule
{
    public M12S2TheLindwurm(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private M12S2TheLindwurm(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    public static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(100f, 100f), 20f, 60)]);
        return (arena.Center, arena);
    }
}
