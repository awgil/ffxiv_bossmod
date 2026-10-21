namespace BossMod.Stormblood.Quest.TheWillOfTheMoon.P1Sadu;

public enum OID : uint
{
    Boss = 0x24A0,
    Helper = 0x233C,
    TheShatteredStar = 0x252E, // R1.000, x0 (spawn during fight)
    KhunShavar = 0x252F, // R1.820, x0 (spawn during fight)
    StellarChuluu = 0x2530, // R1.800, x0 (spawn during fight)
    StarfallCircle = 0x2531, // R1.000, x0 (spawn during fight)
    DotharliChuluu = 0x252D, // R1.800, x0 (spawn during fight)
}

public enum AID : uint
{
    FireAuto = 9123, // Boss->player, no cast, single-target
    DispellingWind = 13223, // Boss->self, 3.0s cast, range 40+R width 8 rect
    WhisperOfLivesPast = 13226, // TheShatteredStar->self, 3.5s cast, range 6-12 donut
    KhunAuto = 872, // KhunShavar->player, no cast, single-target
    TheStoneSpeaks = 13247, // Boss->self, 3.0s cast, single-target
    StellarAuto = 9120, // StellarChuluu->player, 1.0s cast, single-target
    Epigraph1 = 13222, // StellarChuluu->self, 3.0s cast, range 45+R width 8 rect
    ImmortalMettle = 13245, // Boss->self, no cast, single-target
    Epigraph2 = 13225, // DotharliChuluu->self, 3.0s cast, range 45+R width 8 rect
    Tornado = 13228, // KhunShavar->location, 5.0s cast, range 6 circle
    AncientBlizzard = 13227, // KhunShavar->self, 3.0s cast, range 40+R 45-degree cone
    FallingDusk = 13224, // Boss->location, 60.0s cast, range 25 circle, burn me to death mommy
}

public enum SID : uint
{
    BurningSoul = 1613, // none->Boss, extra=0x3/0x2/0x1/0xA
    Invincibility = 775, // none->Boss, extra=0x0
}

class DispellingWind(BossModule module) : Components.StandardAOEs(module, AID.DispellingWind, new AOEShapeRect(40, 4));
class Epigraph1(BossModule module) : Components.StandardAOEs(module, AID.Epigraph1, new AOEShapeRect(45, 4));
class Epigraph2(BossModule module) : Components.StandardAOEs(module, AID.Epigraph2, new AOEShapeRect(45, 4));
class WhisperOfLivesPast(BossModule module) : Components.StandardAOEs(module, AID.WhisperOfLivesPast, new AOEShapeDonut(6, 12));
class AncientBlizzard(BossModule module) : Components.StandardAOEs(module, AID.AncientBlizzard, new AOEShapeCone(40, 22.5f.Degrees()));
class Tornado(BossModule module) : Components.StandardAOEs(module, AID.Tornado, 6);
class KhunShavar(BossModule module) : Components.AddsPointless(module, (uint)OID.KhunShavar);
class StellarChuluu(BossModule module) : Components.Adds(module, (uint)OID.StellarChuluu, forbidDots: true);
class Invincibility(BossModule module) : Components.InvincibleStatus(module, (uint)SID.Invincibility);

class SaduHeavensflameStates : StateMachineBuilder
{
    public SaduHeavensflameStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DispellingWind>()
            .ActivateOnEnter<Epigraph2>()
            .ActivateOnEnter<Epigraph1>()
            .ActivateOnEnter<WhisperOfLivesPast>()
            .ActivateOnEnter<AncientBlizzard>()
            .ActivateOnEnter<Tornado>()
            .ActivateOnEnter<KhunShavar>()
            .ActivateOnEnter<StellarChuluu>()
            .ActivateOnEnter<Invincibility>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68683, NameID = 6152)]
public class SaduHeavensflame(ModuleInit init) : BossModule(init, new(-223.165f, 519), new ArenaBoundsCircle(19.5f));
