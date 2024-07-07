namespace BossMod.Dawntrail.Dungeon.D04Vanguard.D043Zander;

public enum OID : uint
{
    Boss = 0x411E, // R2.100, x1
    Boss2 = 0x41BA, // R2.500, x1
    Helper = 0x233C, // R0.500, x12, 523 type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/Boss2->player, no cast, single-target

    Burst1 = 36575, // Helper->self, 10.0s cast, range 20 width 40 rect
    Burst2 = 36591, // Helper->self, 11.0s cast, range 20 width 40 rect

    Electrothermia = 36594, // Boss->self, 5.0s cast, range 60 circle //raidwide
    SaberRush = 36595, // Boss->player, 5.0s cast, single-target
    Screech = 36596, // Boss2->self, 5.0s cast, range 60 circle //raidwide
    ShadeShot = 36597, // Boss2->player, 5.0s cast, single-target

    SlitherbaneForeguard1 = 36589, // Boss2->self, 4.0s cast, range 20 width 4 rect
    SlitherbaneForeguard2 = 36592, // Helper->self, 4.5s cast, range 20 180.000-degree cone

    SlitherbaneRearguard1 = 36590, // Boss2->self, 4.0s cast, range 20 width 4 rect
    SlitherbaneRearguard2 = 36593, // Helper->self, 4.5s cast, range 20 180.000-degree cone

    SoulbaneSaber = 36574, // Boss->self, 3.0s cast, range 20 width 4 rect
    SoulbaneShock = 37922, // Helper->player, 5.0s cast, range 5 circle
    Syntheslean = 37198, // Boss2->self, 4.0s cast, range 19 90.000-degree cone

    Syntheslither1 = 36579, // Boss2->location, 4.0s cast, single-target
    Syntheslither2 = 36580, // Helper->self, 5.0s cast, range 19 90.000-degree cone
    Syntheslither3 = 36581, // Helper->self, 5.6s cast, range 19 90.000-degree cone
    Syntheslither4 = 36582, // Helper->self, 6.2s cast, range 19 90.000-degree cone
    Syntheslither5 = 36583, // Helper->self, 6.8s cast, range 19 90.000-degree cone
    Syntheslither6 = 36584, // Boss2->location, 4.0s cast, single-target
    Syntheslither7 = 36585, // Helper->self, 5.0s cast, range 19 90.000-degree cone
    Syntheslither8 = 36586, // Helper->self, 5.6s cast, range 19 90.000-degree cone
    Syntheslither9 = 36587, // Helper->self, 6.2s cast, range 19 90.000-degree cone
    Syntheslither10 = 36588, // Helper->self, 6.8s cast, range 19 90.000-degree cone

    UnknownAbility1 = 36576, // Boss->self, no cast, single-target
    UnknownAbility2 = 36577, // Boss->self, no cast, single-target
    UnknownAbility3 = 36578, // Boss->self, no cast, single-target

    UnknownWeaponskill1 = 39240, // Helper->self, 10.5s cast, range 20 width 40 rect
    UnknownWeaponskill2 = 39241, // Helper->self, 11.5s cast, range 20 width 40 rect
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x2
}

public enum IconID : uint
{
    Icon218 = 218, // player
    Icon376 = 376, // player
}

class Electrothermia(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Electrothermia));
class Screech(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Screech));
class Burst1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Burst1), new AOEShapeRect(20, 20));
class Burst2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Burst2), new AOEShapeRect(20, 20));
class SaberRush(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.SaberRush));
class ShadeShot(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.ShadeShot));
class SlitherbaneForeguard1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SlitherbaneForeguard1), new AOEShapeRect(20, 2));
class SlitherbaneForeguard2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SlitherbaneForeguard2), new AOEShapeCone(20, 90.Degrees()));

class SlitherbaneRearguard1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SlitherbaneRearguard1), new AOEShapeRect(20, 2));
class SlitherbaneRearguard2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SlitherbaneRearguard2), new AOEShapeCone(20, 90.Degrees()));

class SoulbaneSaber(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SoulbaneSaber), new AOEShapeRect(20, 2));
class SoulbaneShock(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SoulbaneShock), 5);

class Syntheslean(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Syntheslean), new AOEShapeCone(19, 45.Degrees()));
class Syntheslither2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Syntheslither2), new AOEShapeCone(19, 45.Degrees()));
class Syntheslither3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Syntheslither3), new AOEShapeCone(19, 45.Degrees()));
class Syntheslither4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Syntheslither4), new AOEShapeCone(19, 45.Degrees()));
class Syntheslither5(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Syntheslither5), new AOEShapeCone(19, 45.Degrees()));
class Syntheslither7(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Syntheslither7), new AOEShapeCone(19, 45.Degrees()));
class Syntheslither8(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Syntheslither8), new AOEShapeCone(19, 45.Degrees()));
class Syntheslither9(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Syntheslither9), new AOEShapeCone(19, 45.Degrees()));
class Syntheslither10(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Syntheslither10), new AOEShapeCone(19, 45.Degrees()));

class UnknownWeaponskill1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.UnknownWeaponskill1), new AOEShapeRect(20, 20));
class UnknownWeaponskill2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.UnknownWeaponskill2), new AOEShapeRect(20, 20));

class D043ZanderStates : StateMachineBuilder
{
    public D043ZanderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Electrothermia>()
            .ActivateOnEnter<Screech>()
            .ActivateOnEnter<Burst1>()
            .ActivateOnEnter<Burst2>()
            .ActivateOnEnter<SaberRush>()
            .ActivateOnEnter<ShadeShot>()
            .ActivateOnEnter<SlitherbaneForeguard1>()
            .ActivateOnEnter<SlitherbaneForeguard2>()
            .ActivateOnEnter<SlitherbaneRearguard1>()
            .ActivateOnEnter<SlitherbaneRearguard2>()
            .ActivateOnEnter<SoulbaneSaber>()
            .ActivateOnEnter<SoulbaneShock>()
            .ActivateOnEnter<Syntheslean>()
            .ActivateOnEnter<Syntheslither2>()
            .ActivateOnEnter<Syntheslither3>()
            .ActivateOnEnter<Syntheslither4>()
            .ActivateOnEnter<Syntheslither5>()
            .ActivateOnEnter<Syntheslither7>()
            .ActivateOnEnter<Syntheslither8>()
            .ActivateOnEnter<Syntheslither9>()
            .ActivateOnEnter<Syntheslither10>()
            .ActivateOnEnter<UnknownWeaponskill1>()
            .ActivateOnEnter<UnknownWeaponskill2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.Boss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831, NameID = 12752)]
public class D043Zander(WorldState ws, Actor primary) : BossModule(ws, primary, new(90, -430), new ArenaBoundsCircle(17))
{
    private Actor? _boss2;

    public Actor? Boss() => PrimaryActor;
    public Actor? Boss2() => _boss2;

    protected override void UpdateModule()
    {
        _boss2 ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.Boss2).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_boss2, ArenaColor.Enemy);
    }
}
