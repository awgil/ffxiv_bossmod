using BossMod.Dawntrail.Dungeon.D08Deadwalk.D081Leonogg;

namespace BossMod.Dawntrail.Dungeon.D08Deadwalk.D082Jack;

public enum OID : uint
{
    Boss = 0x41CA, // R4.160, x1
    Helper = 0x233C, // R0.500, x25, 523 type
    Actor1e8536 = 0x1E8536, // R2.000, x0 (spawn during fight), EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    Balloons = 0x1EBA22, // R0.500, x1, EventObj type
    Shortcut = 0x1E873C, // R0.500, x0 (spawn during fight), EventObj type
    Skywheel = 0x4142, // R0.500, x1
    SpectralSamovar = 0x41CB, // R2.880, x13
    StrayPhantagenitrix = 0x41D2, // R2.100, x3
    StrayRascal = 0x4139, // R1.300, x1
    UnknownActor = 0x41D5, // R1.500, x4
}

public enum AID : uint
{
    AutoAttack = 37169, // Boss->player, no cast, single-target

    TroublingTeacups = 36716, // Boss->self, 3.0s cast, single-target // Spawns teacups
    TeaAwhirl = 36717, // Boss->self, 6.0s cast, single-target // Ghost(s) tether teacup and enters, teacups spin then possesed teacup explodes in AOE
    TricksomeTreat = 36720, // StrayPhantagenitrix->self, 3.0s cast, range 19 circle // TeaAwhirl AOE

    ToilingTeapots = 36722, // Boss->self, 3.0s cast, single-target // Spawns 13 teacups

    Puppet = 36721, // StrayPhantagenitrix->location, 4.0s cast, single-target
    PipingPour = 36723, // SpectralSamovar->location, 2.0s cast, single-target // Spreading AOE

    MadTeaParty = 36724, // Helper->self, no cast, range 0 circle // DOT applied to players in puddles

    LastDrop = 36726, // Boss->player, 5.0s cast, single-target // Tankbuster

    SordidSteam = 36725, // Boss->self, 5.0s cast, range 40 circle // Raidwide
}

public enum SID : uint
{
    AreaOfInfluenceUp = 1909, // none->Helper, extra=0x1/0x2/0x3/0x4
    Bleeding = 3078, // Helper->player, extra=0x0
    VulnerabilityUp = 1789, // StrayPhantagenitrix->player, extra=0x1/0x2
}

public enum IconID : uint
{
    Tankbuster = 218, // player
}

public enum TetherID : uint
{
    CupTether = 276, // UnknownActor->StrayPhantagenitrix
}

class TricksomeTreat(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TricksomeTreat), new AOEShapeCircle(19));
class SordidSteam(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SordidSteam));
class LastDrop(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.LastDrop));

class D082JackStates : StateMachineBuilder
{
    public D082JackStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TricksomeTreat>()
            .ActivateOnEnter<SordidSteam>()
            .ActivateOnEnter<LastDrop>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 981, NameID = 12760)]
public class D082Jack(WorldState ws, Actor primary) : BossModule(ws, primary, new(17, -170), new ArenaBoundsCircle(20));
