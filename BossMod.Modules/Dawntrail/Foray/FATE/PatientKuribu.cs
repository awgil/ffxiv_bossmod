namespace BossMod.Dawntrail.Foray.FATE.PatientKuribu;

public enum OID : uint
{
    Boss = 0x4D61,
    Helper = 0x233C,
    PatientKuribu = 0x4DCC, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50537, // Boss->player, no cast, single-target
    Glory = 49915, // Boss->self, 5.0s cast, range 50 90-degree cone

    EnsorcelStoneIII = 49906, // Boss->self, 5.0s cast, single-target
    StoneIII = 49907, // 4DCC->self, 4.0s cast, range 40 45-degree cone
    StoneIII1 = 50071, // 4DCC->self, 4.5s cast, range 40 45-degree cone

    EnsorcelAeroIII = 49905, // Boss->self, 5.0s cast, single-target
    AeroIII = 49908, // 4DCC->location, 4.0s cast, range 40 width 8 rect
    AeroIII1 = 50072, // 4DCC->location, 4.5s cast, range 40 width 8 rect

    HolyCast = 49911, // Boss->self, 3.0s cast, single-target
    HolyStart = 49912, // 4DCC->self, 5.0s cast, range 6 circle
    HolyNext = 49913, // 4DCC->location, 3.0s cast, range 6 circle

    ShortswordAndSorcery = 50118, // Boss->self, 5.0s cast, range 15 circle
    ShortswordAndSorcery1 = 50119, // PatientKuribu->self, 5.0s cast, range 15 circle
    LongswordAndSorcery = 50121, // PatientKuribu->self, 5.0s cast, range 10-25 donut
    LongswordAndSorcery1 = 50120, // PatientKuribu->self, 5.0s cast, range 10-25 donut
}

public enum SID : uint
{
    EnsorcelledAeroIII = 5374, // Boss->Boss, extra=0x0
    EnsorcelledStoneIII = 5375, // Boss->Boss, extra=0x0
}

class Glory(BossModule module) : Components.StandardAOEs(module, AID.Glory, new AOEShapeCone(50, 45.Degrees()));
class StoneIII(BossModule module) : Components.GroupedAOEs(module, [AID.StoneIII, AID.StoneIII1], new AOEShapeCone(40, 22.5f.Degrees()));
class AeroIII(BossModule module) : Components.GroupedAOEs(module, [AID.AeroIII, AID.AeroIII1], new AOEShapeRect(40, 4));
class Holy(BossModule module) : Components.GroupedAOEs(module, [AID.HolyStart, AID.HolyNext], new AOEShapeCircle(6));
class ShortswordAndSorcery(BossModule module) : Components.GroupedAOEs(module, [AID.ShortswordAndSorcery, AID.ShortswordAndSorcery1], new AOEShapeCircle(15));
class LongswordAndSorcery(BossModule module) : Components.GroupedAOEs(module, [AID.LongswordAndSorcery, AID.LongswordAndSorcery1], new AOEShapeDonut(10, 25));

class PatientKuribuStates : StateMachineBuilder
{
    public PatientKuribuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Glory>()
            .ActivateOnEnter<StoneIII>()
            .ActivateOnEnter<AeroIII>()
            .ActivateOnEnter<Holy>()
            .ActivateOnEnter<ShortswordAndSorcery>()
            .ActivateOnEnter<LongswordAndSorcery>();
    }
}

[ModuleInfo(Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14764)]
public class PatientKuribu(WorldState ws, Actor primary) : BossModule(ws, primary, new(-440, -790), new ArenaBoundsCircle(30));
