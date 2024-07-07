namespace BossMod.Dawntrail.Dungeon.D06Alexandria.D063Eliminator;

public enum OID : uint
{
    Boss = 0x41CE, // R6.001, x1
    Helper = 0x233C, // R0.500, x20, 523 type
    Elimbit = 0x41D0, // R2.000, x1
    EliminationClaw = 0x41CF, // R2.000, x1
    LightningGenerator = 0x41D1, // R3.000, x6
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
}

public enum AID : uint
{
    Teleport = 36763, // Boss->location, no cast, single-target
    AutoAttack = 36764, // Boss->player, no cast, single-target

    Disruption = 36765, // Boss->self, 5.0s cast, range 60 circle // Raidwide

    Partition1 = 36766, // Boss->self, no cast, single-target
    Partition2 = 36767, // Boss->self, no cast, single-target
    Partition3 = 36768, // Boss->self, 4.3+0.7s cast, single-target
    Partition4 = 39007, // Helper->self, 5.0s cast, range 40 180.000-degree cone
    Partition5 = 39238, // Helper->self, 7.0s cast, range 40 180.000-degree cone
    Partition6 = 39249, // Helper->self, 7.0s cast, range 40 180.000-degree cone
    Partition7 = 39599, // Boss->self, 6.2+0.6s cast, single-target
    Partition8 = 39600, // Boss->self, 6.2+0.6s cast, single-target

    Subroutine1 = 36781, // Boss->self, 3.0s cast, single-target // Hand or Orb add commands
    Subroutine2 = 36775, // Boss->self, 3.0s cast, single-target
    Subroutine3 = 36772, // Boss->self, 3.0s cast, single-target

    ReconfiguredPartition1 = 39248, // Boss->self, 1.2+5.6s cast, single-target
    ReconfiguredPartition2 = 39247, // Boss->self, 1.2+5.6s cast, single-target

    Terminate1 = 39615, // Helper->self, 7.0s cast, range 40 width 10 rect
    Terminate2 = 36773, // EliminationClaw->self, 6.2+0.6s cast, single-target
    UnknownAbility1 = 36774, // Boss->self, no cast, single-target

    HaloOfDestruction1 = 39616, // Helper->self, 7.0s cast, range ?-40 donut
    HaloOfDestruction2 = 36776, // Elimbit->self, 6.4+0.4s cast, single-target

    UnknownAbility2 = 36777, // Boss->self, no cast, single-target
    UnknownAbility3 = 36778, // Helper->player, no cast, single-target

    Overexposure1 = 36779, // Boss->self, 4.3+0.7s cast, single-target
    Overexposure2 = 36780, // Helper->self, no cast, range 40 width 6 rect // Party stack

    Electray = 39243, // Helper->player, 5.0s cast, range 6 circle

    UnknownAbility4 = 36788, // Boss->self, no cast, single-target
    HoloArk1 = 36789, // Boss->self, no cast, single-target
    HoloArk2 = 36790, // Helper->self, no cast, range 60 circle
    UnknownAbility5 = 36791, // LightningGenerator->Boss, no cast, single-target

    Compression1 = 36792, // EliminationClaw->location, 5.3s cast, single-target
    Compression2 = 36793, // Helper->self, 6.0s cast, range 6 circle

    Impact = 36794, // Helper->self, 6.0s cast, range 60 circle

    LightOfSalvation1 = 36782, // Elimbit->self, 6.0s cast, single-target
    UnknownAbility6 = 36783, // Helper->player, 5.9s cast, single-target
    LightOfSalvation2 = 36784, // Helper->self, no cast, range 40 width 6 rect
    LightOfDevotion1 = 36785, // EliminationClaw->self, 5.0s cast, single-target
    UnknownAbility7 = 36786, // Helper->player, no cast, single-target
    LightOfDevotion2 = 36787, // Helper->self, no cast, range 40 width 6 rect

    Elimination1 = 36795, // Boss->self, 4.0s cast, single-target
    Elimination2 = 36796, // Helper->self, no cast, range 60 circle

    Explosion = 39239, // Helper->self, 8.5s cast, range 50 width 8 rect
}

public enum SID : uint
{
    UnknownStatus1 = 2056, // none->Boss, extra=0x2B6/0x2B7
    UnknownStatus2 = 2193, // none->Boss, extra=0x2FA/0x2FB
    Invincibility = 1570, // none->player, extra=0x0
    VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x2/0x3
    LightningResistanceDownII = 2095, // Helper->player, extra=0x0

}

public enum IconID : uint
{
    Spreadmarker = 139, // player
    Icon534 = 534, // Helper
}

class Disruption(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Disruption));
class Partition4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Partition4), new AOEShapeCone(40, 90.Degrees()));
class Partition5(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Partition5), new AOEShapeCone(40, 90.Degrees()));
class Partition6(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Partition6), new AOEShapeCone(40, 90.Degrees()));
class Terminate1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Terminate1), new AOEShapeRect(40, 5));
class HaloOfDestruction1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HaloOfDestruction1), new AOEShapeDonut(5, 40));
class Electray(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Electray), 6);
class Explosion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion), new AOEShapeRect(50, 4, 50));
class Impact(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Impact), 15, stopAtWall: true, kind: Kind.AwayFromOrigin);
class Compression2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Compression2), new AOEShapeCircle(6));

class D063EliminatorStates : StateMachineBuilder
{
    public D063EliminatorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Disruption>()
            .ActivateOnEnter<Partition4>()
            .ActivateOnEnter<Partition5>()
            .ActivateOnEnter<Partition6>()
            .ActivateOnEnter<Terminate1>()
            .ActivateOnEnter<HaloOfDestruction1>()
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<Compression2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 827, NameID = 12729)]
public class D063Eliminator(WorldState ws, Actor primary) : BossModule(ws, primary, new(-759, -648), new ArenaBoundsSquare(14))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.LightningGenerator), ArenaColor.Enemy);
    }
}
