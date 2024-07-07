using BossMod.Dawntrail.Dungeon.D05Origenics.D051Herpekaris;

namespace BossMod.Dawntrail.Dungeon.D05Origenics.D053Ambrose;

public enum OID : uint
{
    Boss = 0x417D, // R4.998, x1
    Helper = 0x233C, // R0.500, x53, 523 type

    Cahciua = 0x418F, // R0.960, x1
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Electrolance = 0x4180, // R1.380, x1
    Superfluity = 0x417F, // R1.800, x6
    OrigenicsEyeborg = 0x417E, // R4.000, x3
    InformationTerminal1 = 0x1EBA26, // R0.500, x0 (spawn during fight), EventObj type
    InformationTerminal2 = 0x1EBA25, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // Superfluity/OrigenicsEyeborg->player, no cast, single-target

    PsychicWave = 36436, // Boss->self, 5.0s cast, range 80 circle
    UnknownWeaponskill = 36439, // Boss->location, no cast, single-target
    OverwhelmingCharge = 39233, // Boss->self, 5.0s cast, range 26 180.000-degree cone

    Psychokinesis1 = 36427, // Boss->self, 10.0s cast, single-target
    Psychokinesis2 = 36428, // Helper->self, 10.0s cast, range 70 width 13 rect

    ExtrasensoryField = 36432, // Boss->self, 7.0s cast, single-target

    ExtrasensoryExpulsion1 = 36434, // Helper->self, 7.0s cast, range 15 width 20 rect
    ExtrasensoryExpulsion2 = 36433, // Helper->self, 7.0s cast, range 20 width 15 rect

    VoltaicSlash = 36437, // Boss->player, 5.0s cast, single-target
    PsychokineticCharge = 39055, // Boss->self, 7.0s cast, single-target

    OverwhelmingCharge1 = 36435, // Boss->self, no cast, single-target
    OverwhelmingCharge2 = 39072, // Helper->self, 9.8s cast, range 26 180.000-degree cone

    Electrolance = 36429, // Boss->location, 6.0s cast, range 22 circle
    UnknownAbility = 38953, // Helper->location, 2.5s cast, width 10 rect charge

    Psychokinesis = 38929, // Boss->self, 8.0s cast, single-target

    Rush = 38954, // Electrolance->location, no cast, width 10 rect charge

    ElectrolanceAssimilation1 = 36430, // Boss->self, 0.5s cast, single-target
    ElectrolanceAssimilation2 = 36431, // Helper->self, 1.0s cast, range 33 width 10 rect

    WhorlOfTheMind = 36438, // Helper->player, 5.0s cast, range 5 circle
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Boss/Helper->player, extra=0x1
    UnknownStatus = 2056, // none->Electrolance/Boss, extra=0x2D0/0x2D1
    Electrocution1 = 3073, // none->player, extra=0x0
    Electrocution2 = 3074, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon218 = 218, // player
    Icon376 = 376, // player
}

public enum TetherID : uint
{
    Tether266 = 266, // Boss->Electrolance
}

class PsychicWave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PsychicWave));
class OverwhelmingCharge(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.OverwhelmingCharge), new AOEShapeCone(26, 90.Degrees()));
class Psychokinesis2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Psychokinesis2), new AOEShapeRect(70, 6.5f));
class ExtrasensoryExpulsion1(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.ExtrasensoryExpulsion1), 20, stopAtWall: true, kind: Kind.DirForward);
class ExtrasensoryExpulsion2(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.ExtrasensoryExpulsion2), 20, stopAtWall: true, kind: Kind.DirForward);
class VoltaicSlash(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.VoltaicSlash));
class OverwhelmingCharge2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.OverwhelmingCharge2), new AOEShapeCone(26, 90.Degrees()));
class Electrolance(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Electrolance), 22);
class UnknownAbility(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.UnknownAbility), 5);
class ElectrolanceAssimilation2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ElectrolanceAssimilation2), new AOEShapeRect(33, 5));
class WhorlOfTheMind(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.WhorlOfTheMind), 5);

class D053AmbroseStates : StateMachineBuilder
{
    public D053AmbroseStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PsychicWave>()
            .ActivateOnEnter<OverwhelmingCharge>()
            .ActivateOnEnter<Psychokinesis2>()
            .ActivateOnEnter<ExtrasensoryExpulsion1>()
            .ActivateOnEnter<ExtrasensoryExpulsion2>()
            .ActivateOnEnter<VoltaicSlash>()
            .ActivateOnEnter<OverwhelmingCharge2>()
            .ActivateOnEnter<Electrolance>()
            .ActivateOnEnter<UnknownAbility>()
            .ActivateOnEnter<ElectrolanceAssimilation2>()
            .ActivateOnEnter<WhorlOfTheMind>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 825, NameID = 12695)]
public class D053Ambrose(WorldState ws, Actor primary) : BossModule(ws, primary, new(190, 0), new ArenaBoundsRect(16, 20));
