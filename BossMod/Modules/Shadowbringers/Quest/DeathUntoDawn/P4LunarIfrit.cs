namespace BossMod.Shadowbringers.Quest.DeathUntoDawn.P4;

public enum OID : uint
{
    Boss = 0x3202,
    Helper = 0x233C,
    InfernalNail = 0x3205,
}

public enum AID : uint
{
    RadiantPlume1 = 24057, // Helper->self, 7.0s cast, range 8 circle
    Hellfire = 24058, // Boss->self, 36.0s cast, range 40 circle
    Hellfire1 = 24059, // Boss->self, 28.0s cast, range 40 circle
    CrimsonCyclone = 24054, // 3203->self, 4.5s cast, range 49 width 18 rect
    Explosion = 24046, // 3204->self, 5.0s cast, range 80 width 10 cross
    AgonyOfTheDamned1 = 24062, // Helper->self, 0.7s cast, range 40 circle
}

class Hellfire(BossModule module) : Components.RaidwideCast(module, AID.Hellfire);
class Hellfire1(BossModule module) : Components.RaidwideCast(module, AID.Hellfire1);
class AgonyOfTheDamned(BossModule module) : Components.RaidwideCast(module, AID.AgonyOfTheDamned1);
class RadiantPlume(BossModule module) : Components.StandardAOEs(module, AID.RadiantPlume1, new AOEShapeCircle(8));
class CrimsonCyclone(BossModule module) : Components.StandardAOEs(module, AID.CrimsonCyclone, new AOEShapeRect(49, 9), maxCasts: 3);
class InfernalNail(BossModule module) : Components.Adds(module, (uint)OID.InfernalNail, 5);
class Explosion(BossModule module) : Components.StandardAOEs(module, AID.Explosion, new AOEShapeCross(80, 5), maxCasts: 2);

class LunarIfritStates : StateMachineBuilder
{
    public LunarIfritStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RadiantPlume>()
            .ActivateOnEnter<CrimsonCyclone>()
            .ActivateOnEnter<Hellfire>()
            .ActivateOnEnter<Hellfire1>()
            .ActivateOnEnter<InfernalNail>()
            .ActivateOnEnter<AgonyOfTheDamned>()
            .ActivateOnEnter<Explosion>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69602, NameID = 10041)]
public class LunarIfrit(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20));
