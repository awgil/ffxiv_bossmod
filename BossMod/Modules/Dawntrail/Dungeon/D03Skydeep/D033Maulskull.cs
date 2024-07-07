namespace BossMod.Dawntrail.Dungeon.D03Skydeep.D033Maulskull;

public enum OID : uint
{
    Boss = 0x41C7, // R19.980, x1
    Helper = 0x233C, // R0.500, x15, 523 type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 36678, // Boss->player, no cast, single-target

    Stonecarver1 = 36668, // Boss->self, 8.0s cast, single-target
    Stonecarver2 = 36669, // Boss->self, 8.0s cast, single-target
    Stonecarver3 = 36670, // Helper->self, 9.0s cast, range 40 width 20 rect
    Stonecarver4 = 36671, // Helper->self, 11.5s cast, range 40 width 20 rect
    Stonecarver5 = 36672, // Boss->self, no cast, single-target
    Stonecarver6 = 36673, // Boss->self, no cast, single-target

    Stonecarver7 = 36696, // Helper->self, 11.1s cast, range 40 width 20 rect
    Stonecarver8 = 36697, // Helper->self, 13.6s cast, range 40 width 20 rect
    Stonecarver9 = 36699, // Boss->self, no cast, single-target
    Stonecarver10 = 36700, // Boss->self, no cast, single-target

    UnknownAbility1 = 36674, // Boss->self, no cast, single-target

    Skullcrush = 36675, // Boss->self, 5.0+2.0s cast, single-target

    Impact1 = 36677, // Helper->self, 7.0s cast, range 60 circle // Knockback 18 AwayFromOrigin
    Impact2 = 36667, // Helper->self, 9.0s cast, range 60 circle // Knockback 18 AwayFromOrigin
    Impact3 = 36707, // Helper->self, 8.0s cast, range 60 circle // Knockback 20 AwayFromOrigin

    Skullcrush1 = 36675, // Boss->self, 5.0+2.0s cast, single-target
    Skullcrush2 = 36676, // Helper->self, 7.0s cast, range 10 circle
    Skullcrush3 = 36666, // Helper->self, 9.0s cast, range 10 circle

    UnknownAbility2 = 38664, // Boss->self, no cast, single-target
    Charcore = 36708, // Boss->self, no cast, single-target
    DestructiveHeat = 36709, // Helper->players, 7.0s cast, range 6 circle

    Maulwork1 = 36679, // Boss->self, 5.0s cast, single-target
    Maulwork2 = 36680, // Boss->self, 5.0s cast, single-target
    Maulwork3 = 36681, // Boss->self, 5.0s cast, single-target
    Maulwork4 = 36682, // Boss->self, 5.0s cast, single-target

    Landing = 36683, // Helper->location, 3.0s cast, range 8 circle

    Shatter1 = 36684, // Helper->self, 3.0s cast, range 40 width 20 rect // Center cleave
    Shatter2 = 36685, // Helper->self, 3.0s cast, range 45 width 22 rect // LR cleave
    Shatter3 = 36686, // Helper->self, 3.0s cast, range 45 width 22 rect // LR cleave

    DeepThunder1 = 36687, // Boss->self, 6.0s cast, single-target
    DeepThunder2 = 36690, // Helper->self, no cast, range 6 circle
    DeepThunder3 = 36691, // Boss->self, no cast, single-target

    UnknownAbility3 = 36688, // Helper->self, 9.0s cast, range 6 circle
    UnknownAbility4 = 36692, // Boss->self, no cast, single-target

    RingingBlows1 = 36694, // Boss->self, 7.0+2.0s cast, single-target
    RingingBlows2 = 36695, // Boss->self, 7.0+2.0s cast, single-target

    WroughtFire1 = 39121, // Boss->self, 4.0+1.0s cast, single-target
    WroughtFire2 = 39122, // Helper->player, 5.0s cast, range 6 circle

    ColossalImpact1 = 36704, // Boss->self, 6.0+2.0s cast, single-target
    ColossalImpact2 = 36705, // Boss->self, 6.0+2.0s cast, single-target
    ColossalImpact3 = 36706, // Helper->self, 8.0s cast, range 10 circle

    BuildingHeat = 36710, // Helper->players, 7.0s cast, range 6 circle

    Ashlayer1 = 36711, // Boss->self, 3.0+2.0s cast, single-target
    Ashlayer2 = 36712, // Helper->self, no cast, range 60 circle

    UnknownAbility5 = 36689, // Helper->self, 11.0s cast, range 6 circle
}

public enum IconID : uint
{
    Icon375 = 375, // player
    Icon344 = 344, // player
    Icon317 = 317, // player
}

class Stonecarver3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Stonecarver3), new AOEShapeRect(40, 10));
class Stonecarver4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Stonecarver4), new AOEShapeRect(40, 10));
class Stonecarver7(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Stonecarver7), new AOEShapeRect(40, 10));
class Stonecarver8(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Stonecarver8), new AOEShapeRect(40, 10));

class Impact1(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Impact1), 18, stopAtWall: true);
class Impact2(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Impact2), 18, stopAtWall: true);
class Impact3(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Impact3), 20, stopAtWall: true);

class Skullcrush2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Skullcrush2), new AOEShapeCircle(10));
class Skullcrush3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Skullcrush3), new AOEShapeCircle(10));

class DestructiveHeat(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.DestructiveHeat), 6);
class Landing(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Landing), 8);

class Shatter1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Shatter1), new AOEShapeRect(40, 10, 10));
class Shatter2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Shatter2), new AOEShapeRect(45, 11, 10));
class Shatter3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Shatter3), new AOEShapeRect(45, 11, 10));

class UnknownAbility3(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.UnknownAbility3), 6); //I believe this is the stack tower mechanic
class WroughtFire2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.WroughtFire2), 6);
class BuildingHeat(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.BuildingHeat), 6, 8);

class Ashlayer1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Ashlayer1));

class D033MaulskullStates : StateMachineBuilder
{
    public D033MaulskullStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Stonecarver3>()
            .ActivateOnEnter<Stonecarver4>()
            .ActivateOnEnter<Stonecarver7>()
            .ActivateOnEnter<Stonecarver8>()
            .ActivateOnEnter<Impact1>()
            .ActivateOnEnter<Impact2>()
            .ActivateOnEnter<Impact3>()
            .ActivateOnEnter<Skullcrush2>()
            .ActivateOnEnter<Skullcrush3>()
            .ActivateOnEnter<DestructiveHeat>()
            .ActivateOnEnter<Landing>()
            .ActivateOnEnter<Shatter1>()
            .ActivateOnEnter<Shatter2>()
            .ActivateOnEnter<Shatter3>()
            .ActivateOnEnter<UnknownAbility3>()
            .ActivateOnEnter<WroughtFire2>()
            .ActivateOnEnter<BuildingHeat>()
            .ActivateOnEnter<Ashlayer1>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 829, NameID = 12728)]
public class D033Maulskull(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, -430), new ArenaBoundsSquare(20));
