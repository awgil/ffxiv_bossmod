namespace BossMod.Endwalker.Dungeon.D07Smileton.D073BigCheese;

public enum OID : uint
{
    Boss = 0x34D3, // R19.600, x1
    Helper = 0x233C, // R0.500, Helper type
    Bomb = 0x3585, // R0.500, x2, conveyor explosive
    ExcavationBomb = 0x38CF, // R0.500, x2, placed bomb
    ExcavationBomb2 = 0x3741, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 26444, // Boss->player, no cast, single-target
    ExplosivesDistribution = 26446, // Boss->self, 3.0s cast, single-target, visual (conveyor explosives)
    IronKiss = 26445, // Bomb->location, 1.5s cast, range 16 circle
    DispenseExplosives = 27696, // Boss->self, 3.0s cast, single-target, visual (placed bombs)
    ExplosivePower = 27697, // Boss->self, 3.0s cast, single-target, detonates placed bombs
    Excavated = 27698, // ExcavationBomb->self, no cast, range 8 circle
    ExcavatedVisual = 27700, // ExcavationBomb->self, no cast, single-target
    RightDisassembler = 26447, // Boss->self, 8.0s cast, range 30 width 10 rect
    LeftDisassembler = 26448, // Boss->self, 8.0s cast, range 30 width 10 rect
    PiercingMissile = 26449, // Boss->player, 5.0s cast, single-target, tankbuster
    ElectricArc = 26451, // Boss->players, 5.0s cast, range 8 circle stack
    LevelingMissile = 26452, // Boss->self, 5.0s cast, single-target, visual
    LevelingMissileAOE = 26453, // Helper->player, 5.0s cast, range 6 circle spread
}

class IronKiss(BossModule module) : Components.StandardAOEs(module, AID.IronKiss, new AOEShapeCircle(16));
class Excavated(BossModule module) : Components.Voidzone(module, 8, OID.ExcavationBomb);
class Disassembler(BossModule module) : Components.GroupedAOEs(module, [AID.RightDisassembler, AID.LeftDisassembler], new AOEShapeRect(30, 5));
class PiercingMissile(BossModule module) : Components.SingleTargetCast(module, AID.PiercingMissile);
class ElectricArc(BossModule module) : Components.StackWithCastTargets(module, AID.ElectricArc, 8, 4, 4);
class LevelingMissile(BossModule module) : Components.SpreadFromCastTargets(module, AID.LevelingMissileAOE, 6);

class D073BigCheeseStates : StateMachineBuilder
{
    public D073BigCheeseStates(BossModule module) : base(module)
    {
        // TODO: predict conveyor explosives from the arrows (Iron Kiss only shows 1.5s ahead); Violent Discharge raidwide AID unknown
        TrivialPhase()
            .ActivateOnEnter<IronKiss>()
            .ActivateOnEnter<Excavated>()
            .ActivateOnEnter<Disassembler>()
            .ActivateOnEnter<PiercingMissile>()
            .ActivateOnEnter<ElectricArc>()
            .ActivateOnEnter<LevelingMissile>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 794, NameID = 10336)]
public class D073BigCheese(ModuleInit init) : BossModule(init, new(-22, -44), new ArenaBoundsRect(14.5f, 10));
