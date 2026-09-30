namespace BossMod.Heavensward.Trial.T03Thordan;

public enum OID : uint
{
    Boss = 0x1073, // R3.800, King Thordan
    SerZephirin = 0x1074, // R2.200
    SerAdelphel = 0x1075, // R2.200
    SerJanlenoux = 0x1076, // R2.200
    SerVellguine = 0x1077, // R2.200
    SerPaulecrain = 0x1078, // R2.200
    SerIgnasse = 0x1079, // R2.200
    SerGrinnaux = 0x107A, // R2.200
    SerHermenost = 0x107B, // R2.200
    SerGuerrique = 0x107C, // R2.200
    SerCharibert = 0x107D, // R2.200
    SerHaumeric = 0x107E, // R2.200
    SerNoudenet = 0x107F, // R2.200
    Ascalon = 0x1080, // R3.800
    MeteorCircle = 0x1082, // R1.000
    CometCircle = 0x1083, // R1.000
    Helper = 0x1084, // R0.500
}

public enum AID : uint
{
    AutoAttackKnight = 870, // SerAdelphel/Janlenoux->player, no cast, single-target
    Attack = 4190, // Boss->self, no cast, range 8 cone
    TheDragonsEye = 4200, // Boss->self, 4.0s cast, single-target
    TheDragonsGaze = 4201, // Boss->self, 5.0s cast, range 80+R circle gaze
    UltimateEnd = 4202, // Boss->self, no cast, range 80 circle
    TheLightOfAscalon = 4203, // Helper->self, no cast, range 80 circle
    BroadSwing = 4204, // Boss->self, no cast, cleave
    SacredCross = 4205, // SerZephirin->self, 15.0s cast, range 80+R circle
    DimensionalCollapseVisual = 4212, // SerGrinnaux->self, 5.5s cast, single-target
    DimensionalCollapse = 4213, // Helper->self, 6.0s cast, range 3 circle
    ConvictionVisual = 4214, // SerHermenost->self, 5.2s cast, single-target
    Conviction = 4215, // Helper->self, 8.0s cast, range 2 circle
    EternalConviction = 4216, // Helper->self, no cast, range 80 circle
    HeavyImpactVisual = 4217, // SerGuerrique->self, no cast, single-target
    HeavyImpact1 = 4218, // Helper->self, 3.0s cast, range 6 circle
    HeavyImpact2 = 4219, // Helper->self, 3.0s cast, range 12 circle
    HeavyImpact3 = 4220, // Helper->self, 3.0s cast, range 18 circle
    HeavyImpact4 = 4221, // Helper->self, 3.0s cast, range 27 circle
    SpiralThrust = 4222, // SerVellguine->self, 3.0s cast, range 52+R width 12 rect
    SpiralPierce = 4223, // SerPaulecrain->self, 4.0s cast, width 12 charge
    AscalonMight = 4225, // Helper->self, 6.0s cast, range 80+R circle
    SkywardLeap = 4226, // SerIgnasse->self, no cast, range 80 circle stack
    HeavensflameVisual = 4227, // SerCharibert->self, 2.5s cast, single-target
    Heavensflame1 = 4228, // Helper->self, 3.0s cast, range 3 circle
    Heavensflame2 = 4229, // Helper->self, 3.0s cast, range 4 circle
    Heavensflame3 = 4230, // Helper->self, 3.0s cast, range 5 circle
    Heavensflame4 = 4231, // Helper->self, 3.0s cast, range 6 circle
    HolyChain = 4232, // Helper->self, no cast, range 80
    HiemalStormVisual = 4233, // SerHaumeric->self, 2.5s cast, single-target
    HiemalStorm = 4234, // Helper->location, no cast, range 6 circle
    HolyMeteor = 4235, // SerNoudenet->self, 3.0s cast, single-target
}

class TheDragonsGaze(BossModule module) : Components.CastGaze(module, AID.TheDragonsGaze);
class SacredCross(BossModule module) : Components.RaidwideCast(module, AID.SacredCross);
class DimensionalCollapse(BossModule module) : Components.StandardAOEs(module, AID.DimensionalCollapse, 3);
class Conviction(BossModule module) : Components.StandardAOEs(module, AID.Conviction, 2);
class HeavyImpact1(BossModule module) : Components.StandardAOEs(module, AID.HeavyImpact1, 6);
class HeavyImpact2(BossModule module) : Components.StandardAOEs(module, AID.HeavyImpact2, 12);
class HeavyImpact3(BossModule module) : Components.StandardAOEs(module, AID.HeavyImpact3, 18);
class HeavyImpact4(BossModule module) : Components.StandardAOEs(module, AID.HeavyImpact4, 27);
class SpiralThrust(BossModule module) : Components.StandardAOEs(module, AID.SpiralThrust, new AOEShapeRect(54.2f, 6));
class SpiralPierce(BossModule module) : Components.BaitAwayChargeCast(module, AID.SpiralPierce, 6); // TODO: verify charge half-width vs width-12 sheet data
class Heavensflame1(BossModule module) : Components.StandardAOEs(module, AID.Heavensflame1, 3);
class Heavensflame2(BossModule module) : Components.StandardAOEs(module, AID.Heavensflame2, 4);
class Heavensflame3(BossModule module) : Components.StandardAOEs(module, AID.Heavensflame3, 5);
class Heavensflame4(BossModule module) : Components.StandardAOEs(module, AID.Heavensflame4, 6);
class AscalonMight(BossModule module) : Components.RaidwideCast(module, AID.AscalonMight);
// TODO: still missing Attack/BroadSwing cleaves, UltimateEnd/LightOfAscalon, SkywardLeap stack, HolyChain, HiemalStorm, HolyMeteor/meteor circles, Conviction towers, and a real state machine
class Knights(BossModule module) : Components.AddsMulti(module, [
    OID.SerZephirin, OID.SerAdelphel, OID.SerJanlenoux, OID.SerVellguine, OID.SerPaulecrain, OID.SerIgnasse,
    OID.SerGrinnaux, OID.SerHermenost, OID.SerGuerrique, OID.SerCharibert, OID.SerHaumeric, OID.SerNoudenet,
    OID.MeteorCircle, OID.CometCircle
], 1);

class T03ThordanStates : StateMachineBuilder
{
    public T03ThordanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheDragonsGaze>()
            .ActivateOnEnter<SacredCross>()
            .ActivateOnEnter<DimensionalCollapse>()
            .ActivateOnEnter<Conviction>()
            .ActivateOnEnter<HeavyImpact1>()
            .ActivateOnEnter<HeavyImpact2>()
            .ActivateOnEnter<HeavyImpact3>()
            .ActivateOnEnter<HeavyImpact4>()
            .ActivateOnEnter<SpiralThrust>()
            .ActivateOnEnter<SpiralPierce>()
            .ActivateOnEnter<Heavensflame1>()
            .ActivateOnEnter<Heavensflame2>()
            .ActivateOnEnter<Heavensflame3>()
            .ActivateOnEnter<Heavensflame4>()
            .ActivateOnEnter<AscalonMight>()
            .ActivateOnEnter<Knights>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 90, NameID = 3632)] // TODO: clear Incomplete after knight timeline + missing mechanics above
public class T03Thordan(ModuleInit init) : BossModule(init, new(0, 0), new ArenaBoundsCircle(20));
