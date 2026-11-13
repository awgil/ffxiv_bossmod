namespace BossMod.Endwalker.Dungeon.D08StigmaDreamscape.D081ProtoOmega;

public enum OID : uint
{
    Boss = 0x3417, // R8.990, x1
    Helper = 0x233C, // R0.500, Helper type
    MarkIIGuidedMissile = 0x3418, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    SideCannonsLeft = 25376, // Boss->self, 7.0s cast, range 60 180-degree cone
    SideCannonsRight = 25377, // Boss->self, 7.0s cast, range 60 180-degree cone
    GuidedMissile = 25382, // Boss->self, 3.0s cast, single-target, tethered homing missiles
    IronKiss = 25383, // MarkIIGuidedMissile->self, no cast, range 3 circle
    ChemicalMissile = 25384, // Boss->self, 3.0s cast, single-target, visual
    Burn = 25385, // Helper->player, no cast, range 6 circle, non-tank spreads
    ElectricSlide = 25386, // Boss->players, 5.0s cast, range 6 circle, stack + knockback 15
    MustardBomb = 25387, // Boss->player, 5.0s cast, range 5 circle, tankbuster
}

class SideCannons(BossModule module) : Components.GroupedAOEs(module, [AID.SideCannonsLeft, AID.SideCannonsRight], new AOEShapeCone(60, 90.Degrees()));
class ElectricSlide(BossModule module) : Components.StackWithCastTargets(module, AID.ElectricSlide, 6, 4, 4);
class ElectricSlideKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.ElectricSlide, 15, stopAtWall: true);
class MustardBomb(BossModule module) : Components.BaitAwayCast(module, AID.MustardBomb, new AOEShapeCircle(5), true);

class D081ProtoOmegaStates : StateMachineBuilder
{
    public D081ProtoOmegaStates(BossModule module) : base(module)
    {
        // TODO: Rear / Forward Interceptors AIDs unknown; Chemical Missile spreads have no cast, lava puddle OID unknown; edge bleed zone size unknown
        TrivialPhase()
            .ActivateOnEnter<SideCannons>()
            .ActivateOnEnter<ElectricSlide>()
            .ActivateOnEnter<ElectricSlideKnockback>()
            .ActivateOnEnter<MustardBomb>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 784, NameID = 10401)]
public class D081ProtoOmega(ModuleInit init) : BossModule(init, new(-144, -136), new ArenaBoundsSquare(20));
