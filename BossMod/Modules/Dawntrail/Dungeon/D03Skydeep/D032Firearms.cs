namespace BossMod.Dawntrail.Dungeon.D03Skydeep.D032Firearms;

public enum OID : uint
{
    Boss = 0x4184, // R4.620, x1
    Helper = 0x233C, // R0.500, x20, 523 type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x4, EventObj type
    Actor1ea2ef = 0x1EA2EF, // R0.500, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    DynamicDominance = 36448, // Boss->self, 5.0s cast, range 70 circle
    MirrorManeuver = 39139, // Boss->self, 3.0s cast, single-target
    UnknownAbility = 36451, // Boss->location, no cast, single-target

    ThunderlightBurstVisual = 36443, // Boss->self, 8.0s cast, single-target
    ThunderlightBurstAOE = 36445, // Helper->self, 10.9s cast, range 35 circle

    ThunderlightBurst1 = 38581, // Helper->self, 8.2s cast, range 42 width 8 rect
    ThunderlightBurst2 = 38582, // Helper->self, 8.2s cast, range 49 width 8 rect
    ThunderlightBurst3 = 38583, // Helper->self, 8.2s cast, range 35 width 8 rect
    ThunderlightBurst4 = 38584, // Helper->self, 8.2s cast, range 36 width 8 rect

    AncientArtillery = 36442, // Boss->self, 3.0s cast, single-target
    EmergentArtillery = 39000, // Boss->self, 3.0s cast, single-target

    Artillery1 = 38660, // Helper->self, 8.5s cast, range 10 width 10 rect
    Artillery2 = 38661, // Helper->self, 8.5s cast, range 10 width 10 rect
    Artillery3 = 38662, // Helper->self, 8.5s cast, range 10 width 10 rect
    Artillery4 = 38663, // Helper->self, 8.5s cast, range 10 width 10 rect

    Pummel = 36447, // Boss->player, 5.0s cast, single-target

    ThunderlightFlurry = 36450, // Helper->player, 5.0s cast, range 6 circle
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x2/0x3
    Burns1 = 3065, // none->player, extra=0x0
    Burns2 = 3066, // none->player, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 218, // player
    Spreadmarker = 139, // player
}

class DynamicDominance(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DynamicDominance));

class ThunderlightBurstAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThunderlightBurstAOE), new AOEShapeCircle(35));
class ThunderlightBurst1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThunderlightBurst1), new AOEShapeRect(42, 4));
class ThunderlightBurst2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThunderlightBurst2), new AOEShapeRect(49, 4));
class ThunderlightBurst3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThunderlightBurst3), new AOEShapeRect(35, 4));
class ThunderlightBurst4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThunderlightBurst4), new AOEShapeRect(36, 4));

class Artillery1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Artillery1), new AOEShapeRect(10, 5));
class Artillery2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Artillery2), new AOEShapeRect(10, 5));
class Artillery3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Artillery3), new AOEShapeRect(10, 5));
class Artillery4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Artillery4), new AOEShapeRect(10, 5));

class Pummel(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Pummel));
class ThunderlightFlurry(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.ThunderlightFlurry), 6);

class D032FirearmsStates : StateMachineBuilder
{
    public D032FirearmsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DynamicDominance>()
            .ActivateOnEnter<ThunderlightBurstAOE>()
            .ActivateOnEnter<ThunderlightBurst1>()
            .ActivateOnEnter<ThunderlightBurst2>()
            .ActivateOnEnter<ThunderlightBurst3>()
            .ActivateOnEnter<ThunderlightBurst4>()
            .ActivateOnEnter<Artillery1>()
            .ActivateOnEnter<Artillery2>()
            .ActivateOnEnter<Artillery3>()
            .ActivateOnEnter<Artillery4>()
            .ActivateOnEnter<Pummel>()
            .ActivateOnEnter<ThunderlightFlurry>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 829, NameID = 12888)]
public class D032Firearms(WorldState ws, Actor primary) : BossModule(ws, primary, new(-85, -155), new ArenaBoundsSquare(20));
