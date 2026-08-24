namespace BossMod.Dawntrail.Alliance.A34AwAern;

public enum OID : uint
{
    Boss = 0x4DB6, // R4.500, x1
    Awzdei = 0x4DB7, // R2.300, x4
    Helper = 0x233C, // R0.500, x25, Helper type
}

public enum AID : uint
{
    BossAuto = 45307, // Boss->player, no cast, single-target
    AwzdeiAuto = 50477, // 4DB7->player, no cast, single-target
    GlacierSplitterVisual = 50104, // Boss->self, 2.9+0.6s cast, single-target
    GlacierSplitter = 50105, // Helper->self, 3.5s cast, range 60 30-degree cone
    OpticInduration = 50106, // Awzdei->self, 3.5s cast, range 60 30-degree cone
    StaticFilament = 50487, // Awzdei->location, 4.0s cast, range 8 circle
    AuroralWindCast = 50501, // Boss->self, 5.0s cast, single-target
    AuroralWind = 50502, // Helper->players, 5.0s cast, range 6 circle
    ImpactStreamCast = 50485, // Boss->self, 4.0s cast, single-target
    ImpactStream = 50486, // Helper->self, 4.0s cast, range 80 circle
}

class Awzdei(BossModule module) : Components.Adds(module, (uint)OID.Awzdei);
class GlacierSplitter(BossModule module) : Components.StandardAOEs(module, AID.GlacierSplitter, new AOEShapeCone(60, 15.Degrees()));
class OpticInduration(BossModule module) : Components.StandardAOEs(module, AID.OpticInduration, new AOEShapeCone(60, 15.Degrees()));
class StaticFilament(BossModule module) : Components.StandardAOEs(module, AID.StaticFilament, 8);
class AuroralWind(BossModule module) : Components.SpreadFromCastTargets(module, AID.AuroralWind, 6);
class ImpactStream(BossModule module) : Components.RaidwideCast(module, AID.ImpactStream);

class A34AwaernStates : StateMachineBuilder
{
    public A34AwaernStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Awzdei>()
            .ActivateOnEnter<GlacierSplitter>()
            .ActivateOnEnter<OpticInduration>()
            .ActivateOnEnter<StaticFilament>()
            .ActivateOnEnter<AuroralWind>()
            .ActivateOnEnter<ImpactStream>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117, NameID = 14838)]
public class A34Awaern(WorldState ws, Actor primary) : BossModule(ws, primary, new(-720, 720), new ArenaBoundsSquare(28));
