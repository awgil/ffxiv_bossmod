namespace BossMod.Endwalker.Dungeon.D08Stigma.D081ProtoOmega;

public enum OID : uint
{
    Boss = 0x3417, // R=8.99
    Helper = 0x233C,
    MarkIIGuidedMissile = 0x3418, // R1.000, x0 (spawn during fight)
    Actor1eb24e = 0x1EB24E, // R0.500, x0 (spawn during fight), EventObj type
    Actor1e8d9b = 0x1E8D9B, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Burn = 25385, // Helper->player, no cast, range 6 circle
    ChemicalMissile = 25384, // Boss->self, 3.0s cast, single-target
    ElectricSlide = 25386, // Boss->players, 5.0s cast, range 6 circle //Stack+Knockback
    GuidedMissile = 25382, // Boss->self, 3.0s cast, single-target //Tethered bait away
    IronKiss = 25383, // MarkIIGuidedMissile->self, no cast, range 3 circle 
    MustardBomb = 25387, // Boss->player, 5.0s cast, range 5 circle
    SideCannons1 = 25376, // Boss->self, 7.0s cast, range 60 180-degree cone
    SideCannons2 = 25377, // Boss->self, 7.0s cast, range 60 180-degree cone
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // MarkIIGuidedMissile->player, extra=0x1
    Burns = 2194, // none->player, extra=0x0
    Bleeding = 2088, // Boss->player, extra=0x0

}

public enum IconID : uint
{
    Icon139 = 139, // player
    Icon289 = 289, // player
    Icon218 = 218, // player
}

public enum TetherID : uint
{
    Tether17 = 17, // MarkIIGuidedMissile->player
}
class ElectricSlideKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.ElectricSlide), 15, stopAtWall: true);
class ElectricSlide(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.ElectricSlide), 6, 8);
class IronKiss(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IronKiss), new AOEShapeCircle(3));

class SideCannons1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SideCannons1), new AOEShapeCone(60, 90.Degrees()));
class SideCannons2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SideCannons2), new AOEShapeCone(60, 90.Degrees()));

class MustardBomb(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.MustardBomb));

class D081ProtoOmegaStates : StateMachineBuilder
{
    public D081ProtoOmegaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectricSlide>()
            .ActivateOnEnter<ElectricSlideKnockback>()
            .ActivateOnEnter<IronKiss>()
            .ActivateOnEnter<SideCannons1>()
            .ActivateOnEnter<SideCannons2>()
            .ActivateOnEnter<MustardBomb>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 784, NameID = 10401)]
public class D081ProtoOmega(WorldState ws, Actor primary) : BossModule(ws, primary, new(-144, -136), new ArenaBoundsSquare(20));
