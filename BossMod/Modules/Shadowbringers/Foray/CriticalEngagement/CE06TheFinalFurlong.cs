namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE06TheFinalFurlong;

public enum OID : uint
{
    Boss = 0x2DBB, // R3.375, x1
    Helper = 0x233C, // R0.500, x20
    Monoceros = 0x2DB9, // R1.800, x1
    LlofiiTheForthright = 0x2DBA, // R0.500, x1
    GraspingRancor = 0x2DBC, // R1.600, spawn during fight
}

public enum AID : uint
{
    AutoAttackBoss = 6498, // Boss->player, no cast, single-target
    GraspingRancor = 20118, // Boss->self, 4.0s cast, single-target, visual (spawn hands)
    SpitefulGuillotine = 20119, // GraspingRancor->player, no cast, single-target, debuffs or kills if reaches tethered player
    SpiteWave = 20124, // Boss->self, 3.0s cast, single-target, visual (stack + puddles)
    HatefulMiasma = 20125, // Helper->players, 5.0s cast, range 6 circle stack
    PoisonedWords = 20126, // Helper->location, 5.0s cast, range 6 circle puddles
    TalonedGaze = 20127, // Boss->self, 4.0s cast, single-target, visual (front/back then sides)
    TalonedWings = 20128, // Boss->self, 4.0s cast, single-target, visual, (sides then front/back)
    CoffinNails = 20129, // Helper->self, 4.7s cast, range 60 90-degree cone aoe
    Stab = 20130, // Boss->player, 4.0s cast, single-target, tankbuster
    GripOfPoison = 20131, // Boss->self, 4.0s cast, range 60 circle, raidwide
    StepsOfDestruction = 21018, // Boss->self, 4.0s cast, single-target, visual (puddles)
    StepsOfDestructionAOE = 21019, // Helper->location, 4.0s cast, range 6 circle puddle
    AutoAttackMonoceros = 871, // Monoceros->Boss, no cast, single-target
    PurifyingLight = 20132, // Monoceros->location, 12.0s cast, range 12 circle (destroys hands)
    FabledHope = 20134, // Monoceros->self, 8.0s cast, range 10 circle, damage up on players
    Teleport = 20135, // Monoceros->location, no cast, single-target, teleport
    Ruin = 20142, // LlofiiTheForthright->Boss, 2.5s cast, single-target, autoattack
    Scupper = 21334, // LlofiiTheForthright->Boss, 2.0s cast, single-target, damage down on boss
}

public enum TetherID : uint
{
    Movable = 1, // GraspingRancor->player
    Frozen = 2, // GraspingRancor->player
    Unfreezable = 17, // GraspingRancor->player (appears if hand wasn't hit by aoe)
}

class GraspingRancor : Components.StandardAOEs
{
    private readonly IReadOnlyList<Actor> _hands;

    public GraspingRancor(BossModule module) : base(module, AID.PurifyingLight, 12)
    {
        Color = ArenaColor.SafeFromAOE;
        Risky = false;
        _hands = module.Enemies(OID.GraspingRancor);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (Casters.Count > 0)
        {
            var hand = _hands.FirstOrDefault(h => h.Tether.Target == actor.InstanceID);
            if (hand != null)
            {
                bool shouldBeFrozen = ((AOEShapeCircle)Shape).Check(hand.Position, Casters[0].CastInfo!.LocXZ);
                bool isFrozen = hand.Tether.ID == (uint)TetherID.Frozen;
                hints.Add(shouldBeFrozen ? "Face the hand!" : "Look away from hand and kite into safezone!", shouldBeFrozen != isFrozen);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var hand = _hands.FirstOrDefault(h => h.Tether.Target == pc.InstanceID);
        if (hand != null)
        {
            bool isFrozen = hand.Tether.ID == (uint)TetherID.Frozen;
            Arena.Actor(hand, ArenaColor.Object, true);
            Arena.AddLine(hand.Position, pc.Position, isFrozen ? ArenaColor.Safe : ArenaColor.Danger);
        }
    }
}

class HatefulMiasma(BossModule module) : Components.StackWithCastTargets(module, AID.HatefulMiasma, 6);
class PoisonedWords(BossModule module) : Components.StandardAOEs(module, AID.PoisonedWords, 6);
class TalonedGaze(BossModule module) : Components.CastHint(module, AID.TalonedGaze, "AOE front/back --> sides");
class TalonedWings(BossModule module) : Components.CastHint(module, AID.TalonedWings, "AOE sides --> front/back");
class CoffinNails(BossModule module) : Components.StandardAOEs(module, AID.CoffinNails, new AOEShapeCone(60, 45.Degrees()), 2);
class Stab(BossModule module) : Components.SingleTargetCast(module, AID.Stab);
class GripOfPoison(BossModule module) : Components.RaidwideCast(module, AID.GripOfPoison);
class StepsOfDestruction(BossModule module) : Components.StandardAOEs(module, AID.StepsOfDestructionAOE, 6);

class SpartoiStates : StateMachineBuilder
{
    public SpartoiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GraspingRancor>()
            .ActivateOnEnter<HatefulMiasma>()
            .ActivateOnEnter<PoisonedWords>()
            .ActivateOnEnter<TalonedGaze>()
            .ActivateOnEnter<TalonedWings>()
            .ActivateOnEnter<CoffinNails>()
            .ActivateOnEnter<Stab>()
            .ActivateOnEnter<GripOfPoison>()
            .ActivateOnEnter<StepsOfDestruction>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 735, NameID = 6)] // bnpcname=9405
public class Spartoi(WorldState ws, Actor primary) : BossModule(ws, primary, new(644, 228), new ArenaBoundsCircle(27));
