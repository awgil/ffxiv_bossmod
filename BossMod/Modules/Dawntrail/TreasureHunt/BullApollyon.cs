namespace BossMod.Dawntrail.TreasureHunt.BullApollyon;

public enum OID : uint
{
    Boss = 0x4305, // R7.000, x1
    Helper = 0x233C, // R0.500, x16, Helper type
    // The following 5 need to be killed in order from 1->5 to maximize the rewards
    TuraliOnion = 0x4300, // R0.840, // Icon #1
    TuraliEggplant = 0x4301, // R0.840, x0 // Icon #2
    TuraliGarlic = 0x4302, // R0.840, x0 // Icon #3
    TuraliTomato = 0x4303, // R0.840, x0 // Icon #4
    TuligoraQueen = 0x4304, // R0.840, x0 // Icon #5
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x6, EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Blade = 38261, // Boss->player, 5.0s cast, single-target

    BlazingBlastBoss = 38259, // Boss->self, 3.0s cast, single-target
    BlazingBlastHelper = 38260, // Helper->location, 3.0s cast, range 6 circle

    BlazingBreathBoss = 38257, // Boss->self, 2.3+0.7s cast, single-target
    BlazingBreathHelper = 38258, // Helper->player, 3.0s cast, range 44 width 10 rect

    CrossfireBladeInitialBoss = 38253, // Boss->self, 4.0s cast, range 20 width 10 cross
    CrossfireBladeInitialHelper = 38254, // Helper->self, 11.0s cast, range 20 width 10 cross
    CrossfireBladeFollowUps = 38255, // Helper->self, 2.5s cast, range 40 width 5 rect

    FlameBladeInitalBoss = 38249, // Boss->self, 4.0s cast, range 40 width 10 rect
    FlameBladeInitialHelper = 38250, // Helper->self, 11.0s cast, range 40 width 10 rect
    FlameBladeHelperFollowUps = 38251, // Helper->self, 2.5s cast, range 40 width 5 rect

    HeirloomScream = 32304, // TuraliTomato->self, 3.5s cast, range 7 circle
    PluckAndPrune = 32302, // TuraliEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // TuligoraQueen->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // TuraliGarlic->self, 3.5s cast, range 7 circle

    PyreburstCast = 38262, // Boss->self, 5.0s cast, single-target
    PyreburstTelegraph = 38263, // Helper->self, no cast, range 60 circle, Raidwide?
    UnknownAbility1 = 38334, // Boss->location, no cast, single-target
    UnknownAbility2 = 38248, // Boss->self, no cast, single-target
}

class Blade(BossModule module) : Components.SingleTargetCast(module, AID.Blade);
class BlazingBlast(BossModule module) : Components.StandardAOEs(module, AID.BlazingBlastHelper, 6);
class BlazingBreath(BossModule module) : Components.StandardAOEs(module, AID.BlazingBreathHelper, new AOEShapeRect(44, 5));
class CrossfireBladeBoss(BossModule module) : Components.StandardAOEs(module, AID.CrossfireBladeInitialBoss, new AOEShapeRect(20, 5, 20));
class CrossfireBladeHelper(BossModule module) : Components.StandardAOEs(module, AID.CrossfireBladeInitialHelper, new AOEShapeRect(20, 5, 20));
class CrossfireFollowup(BossModule module) : Components.StandardAOEs(module, AID.CrossfireBladeFollowUps, new AOEShapeRect(20, 2.5f, 20));
class FlameBladeBoss(BossModule module) : Components.StandardAOEs(module, AID.FlameBladeInitalBoss, new AOEShapeRect(20, 5, 20));
class FlameBladeHelper(BossModule module) : Components.StandardAOEs(module, AID.FlameBladeInitialHelper, new AOEShapeRect(20, 5, 20));
class FlameBladeFollowup(BossModule module) : Components.StandardAOEs(module, AID.FlameBladeHelperFollowUps, new AOEShapeRect(20, 2.5f, 20));
class PyreBurst(BossModule module) : Components.RaidwideCast(module, AID.PyreburstCast);

class BonusAdds(BossModule module) : Components.AddsMulti(module, [OID.TuligoraQueen, OID.TuraliTomato, OID.TuraliGarlic, OID.TuraliEggplant, OID.TuraliOnion])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.TuligoraQueen => 6,
                OID.TuraliTomato => 5,
                OID.TuraliGarlic => 4,
                OID.TuraliEggplant => 3,
                OID.TuraliOnion => 2,
                OID.Boss => 1,
                _ => 0
            };
    }
};
class HeirloomScream(BossModule module) : Components.StandardAOEs(module, AID.HeirloomScream, new AOEShapeCircle(7));
class PluckAndPrune(BossModule module) : Components.StandardAOEs(module, AID.Pollen, new AOEShapeCircle(7));
class Pollen(BossModule module) : Components.StandardAOEs(module, AID.Pollen, new AOEShapeCircle(7));
class PungentPirouette(BossModule module) : Components.StandardAOEs(module, AID.PungentPirouette, new AOEShapeCircle(7));

class BullApollyonStates : StateMachineBuilder
{
    public BullApollyonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Blade>()
            .ActivateOnEnter<BlazingBlast>()
            .ActivateOnEnter<BlazingBreath>()
            .ActivateOnEnter<CrossfireBladeBoss>()
            .ActivateOnEnter<CrossfireBladeHelper>()
            .ActivateOnEnter<CrossfireFollowup>()
            .ActivateOnEnter<FlameBladeBoss>()
            .ActivateOnEnter<FlameBladeHelper>()
            .ActivateOnEnter<FlameBladeFollowup>()
            .ActivateOnEnter<PyreBurst>()
            .ActivateOnEnter<BonusAdds>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<PungentPirouette>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 993, NameID = 13247)]
public class BullApollyon(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -372), new ArenaBoundsCircle(20));
