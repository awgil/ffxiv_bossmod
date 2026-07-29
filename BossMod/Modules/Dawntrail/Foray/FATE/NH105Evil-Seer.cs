using System.ComponentModel;

namespace BossMod.Dawntrail.Foray.FATE.NH105EvilSeer;

public enum OID : uint {
    EvilSeer = 0x4BA7,
    Helper = 0x233C,
    EvilSeer1 = 0x4BAA, // R0.500, x0 (spawn during fight)
    AccursedOrb = 0x4BA8, // R2.000, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 47146, // EvilSeer->player, no cast, single-target
    Ability = 45338, // EvilSeer->player, no cast, single-target
    AllEyes = 47147, // EvilSeer->self, 3.0+0.5s cast, range 30 circle
    JettaturaCast = 47150, // EvilSeer->self, 3.0s cast, single-target
    Jettatura = 47151, // 4BAA->location, 4.0s cast, range 8 circle
    ColdStare = 47149, // EvilSeer->self, 4.0s cast, range 40 90.000-degree cone
    SeeNoEvil = 47148, // EvilSeer->self, 5.0s cast, range 30 circle
    SinisterSight = 47152, // 4BA8->location, 5.0s cast, range 50 circle
}

sealed class AllEyes(BossModule module) : Components.RaidwideCast(module, (uint)AID.AllEyes);
sealed class Jettatura(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Jettatura, new AOEShapeCircle(8.0f));
sealed class ColdStare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ColdStare, new AOEShapeCone(40.0f, 45.0f.Degrees()));
sealed class SeeNoEvil(BossModule module) : Components.CastGaze(module, (uint)AID.SeeNoEvil);
sealed class SinisterSight(BossModule module) : Components.CastGaze(module, (uint)AID.SinisterSight);

[SkipLocalsInit]
sealed class EvilSeerStates : StateMachineBuilder {
    public EvilSeerStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<AllEyes>()
            .ActivateOnEnter<SeeNoEvil>()
            .ActivateOnEnter<Jettatura>()
            .ActivateOnEnter<ColdStare>()
            .ActivateOnEnter<SinisterSight>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(EvilSeerStates),
    ConfigType = null, // replace null with typeof(EvilSeerConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = null, // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.EvilSeer,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14726u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class EvilSeer(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);
