namespace BossMod.Endwalker.Dungeon.D03Vanaspati.D032Wrecker;

public enum OID : uint
{
    Boss = 0x33E9, // R=6.0
    Helper = 0x233C,
    QueerBubble = 0x3731, // R2.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AetherSiphonFire = 25145, // Boss->self, 3.0s cast, single-target
    AetherSiphonWater = 25146, // Boss->self, 3.0s cast, single-target
    AetherSprayFire = 25147, // Boss->location, 7.0s cast, range 30 circle Raidwide
    AetherSprayWater = 25148, // Boss->location, 7.0s cast, range 30 circle Knockback
    MeaninglessDestruction = 25153, // Boss->self, 5.0s cast, range 100 circle
    PoisonHeartVisual = 25151, // Boss->self, 5.0s cast, single-target
    PoisonHeartStack = 27851, // Helper->players, 5.0s cast, range 6 circle
    TotalWreck = 25154, // Boss->player, 5.0s cast, single-target
    UnholyWater = 27852, // Boss->self, 3.0s cast, single-target
    Withdraw = 27847, // 3731->player, 1.0s cast, single-target
}

public enum SID : uint
{
    FireResistanceUp = 1517, // 3731->player, extra=0x0
    WaterResistanceDownII = 1025, // 3731->player, extra=0x0
    Fetters = 1399, // 3731->player, extra=0xEC4
    Burns = 3065, // none->player, extra=0x0
    Burns2 = 3066, // none->player, extra=0x0
    VulnerabilityUp = 1789, // Boss->player, extra=0x1
}

public enum IconID : uint
{
    Icon_218 = 218, // player
    Icon_161 = 161, // player
}

class MeaninglessDestruction(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MeaninglessDestruction));
class PoisonHeartStack(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.PoisonHeartStack), 6, 8);
class TotalWreck(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.TotalWreck));
class AetherSprayWater(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.AetherSprayWater), 13, shape: new AOEShapeCircle(30));

class D032WreckerStates : StateMachineBuilder
{
    public D032WreckerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetherSprayWater>()
            .ActivateOnEnter<TotalWreck>()
            .ActivateOnEnter<PoisonHeartStack>()
            .ActivateOnEnter<MeaninglessDestruction>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 789, NameID = 10718)]
public class D032Wrecker(WorldState ws, Actor primary) : BossModule(ws, primary, new(-295, -355), new ArenaBoundsCircle(20));
