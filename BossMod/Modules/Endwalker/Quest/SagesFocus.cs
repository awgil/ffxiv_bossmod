namespace BossMod.Endwalker.Quest.SagesFocus;

public enum OID : uint
{
    Boss = 0x3587,
    Helper = 0x233C,
    Mahaud = 0x3586,
    Loifa = 0x3588,
}

public enum AID : uint
{
    TripleThreat = 26535, // Boss->3589, 8.0s cast, single-target
    ChiBomb = 26536, // Boss->self, 5.0s cast, single-target
    Explosion = 26537, // 358D->self, 5.0s cast, range 6 circle
    ArmOfTheScholar = 26543, // Boss->self, 5.0s cast, range 5 circle
    Nouliths = 26538, // 3588->self, 5.0s cast, single-target
    Noubelea = 26541, // 3588->self, 5.0s cast, single-target
    Noubelea1 = 26542, // 358E->self, 5.0s cast, range 50 width 4 rect
    DemiblizzardIII = 26545, // 3586->self, 5.0s cast, single-target
    DemiblizzardIII1 = 26546, // Helper->self, 5.0s cast, range -40 donut
    Demigravity = 26539, // 3586->location, 5.0s cast, range 6 circle
    Demigravity1 = 26550, // Helper->location, 5.0s cast, range 6 circle
    DemifireIII = 26547, // 3586->self, 5.0s cast, single-target
    DemifireIII1 = 26548, // Helper->self, 5.6s cast, range 40 circle
    DemifireII = 26552, // Mahaud->self, 7.0s cast, single-target
    DemifireII1 = 26553, // Helper->player/3589, 5.0s cast, range 5 circle
    DemifireII2 = 26554, // Helper->location, 5.0s cast, range 14 circle
}

class DemifireSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.DemifireII1, 5);
class DemifireII(BossModule module) : Components.StandardAOEs(module, AID.DemifireII2, 14);
class DemifireIII(BossModule module) : Components.RaidwideCast(module, AID.DemifireIII1);
class Noubelea(BossModule module) : Components.StandardAOEs(module, AID.Noubelea1, new AOEShapeRect(50, 2));
class Demigravity(BossModule module) : Components.StandardAOEs(module, AID.Demigravity, 6);
class Demigravity1(BossModule module) : Components.StandardAOEs(module, AID.Demigravity1, 6);
class Demiblizzard(BossModule module) : Components.StandardAOEs(module, AID.DemiblizzardIII1, new AOEShapeDonut(10, 40));
class TripleThreat(BossModule module) : Components.SingleTargetCast(module, AID.TripleThreat);
class Explosion(BossModule module) : Components.StandardAOEs(module, AID.Explosion, new AOEShapeCircle(6));
class ArmOfTheScholar(BossModule module) : Components.StandardAOEs(module, AID.ArmOfTheScholar, new AOEShapeCircle(5));

class AncelRockfistStates : StateMachineBuilder
{
    public AncelRockfistStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TripleThreat>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<ArmOfTheScholar>()
            .ActivateOnEnter<Noubelea>()
            .ActivateOnEnter<Demiblizzard>()
            .ActivateOnEnter<Demigravity>()
            .ActivateOnEnter<Demigravity1>()
            .ActivateOnEnter<DemifireIII>()
            .ActivateOnEnter<DemifireII>()
            .ActivateOnEnter<DemifireSpread>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69604, NameID = 10732)]
public class AncelRockfist(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -82.17f), new ArenaBoundsCircle(18.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
